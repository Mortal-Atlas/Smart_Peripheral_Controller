using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;
using System.Text;
using System.Collections.Concurrent;

public class PhoneMqttBridge : M2MqttUnityClient
{
    public static PhoneMqttBridge Instance { get; private set; }
    public event Action OnSnapRequested;
    
    public event Action<bool> OnVRStatusChanged;
    public event Action<string> OnChatReceived;
    
    // Thread-safe queue for UI updates
    private ConcurrentQueue<Action> _mainThreadActions = new ConcurrentQueue<Action>();

    [Header("Margo Network")]
    public string margoBrokerIP = "100.98.214.51";
    public int margoBrokerPort = 1883;
    public string margoUser = "rika";
    public string margoPass = "12345";

    private float connectionTime = 0f;

    protected override void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        
        brokerAddress = margoBrokerIP;
        brokerPort = margoBrokerPort;
        mqttUserName = margoUser;
        mqttPassword = margoPass;
        base.Awake();
    }

    protected override void Update()
    {
        base.Update(); // CRITICAL: Lets M2Mqtt process network messages on the main thread!

        // Execute queued actions on the Main Thread
        while (_mainThreadActions.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        connectionTime = Time.time; // Mark the exact time we connected
        
        // 1. Tell the Pi to burn the old ghost sticky note
        client.Publish("vr/status", new byte[0], MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);

        // 2. Subscribe to the topics
        client.Subscribe(new string[] { "rika/phone/camera", "vr/status", "rika/response", "rika/phone/rumble" }, 
            new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
            
        // 3. Ping the network to see if the VR headset is currently active
        PublishMessage("vr/status/ping", "ping");
            
        Debug.Log("[Margo Bridge] Phone connected to the hive mind.");
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        string payload = Encoding.UTF8.GetString(message);
        
        if (topic == "vr/status")
        {
            if (string.IsNullOrEmpty(payload)) return;

            // BOOT GUARD: Ignore any status messages that arrive within 2 seconds of booting up.
            // This prevents the Mosquitto broker from feeding us a ghost sticky note before it gets deleted!
            if (Time.time - connectionTime < 2.0f)
            {
                Debug.LogWarning("[Margo Bridge] Ignored a ghost vr/status message during boot sequence.");
                return;
            }

            bool isVrOnline = (payload == "online");
            Debug.Log($"[Margo Bridge] VR Headset status changed to: {payload}");
            _mainThreadActions.Enqueue(() => OnVRStatusChanged?.Invoke(isVrOnline));
        }
        else if (topic == "rika/phone/camera" && payload == "SNAP")
        {
            _mainThreadActions.Enqueue(() => OnSnapRequested?.Invoke());
        }
        else if (topic == "rika/response")
        {
            _mainThreadActions.Enqueue(() => OnChatReceived?.Invoke(payload));
        }
        else if (topic == "rika/phone/rumble" && payload == "BITE")
        {
            _mainThreadActions.Enqueue(() => 
            {
                Debug.Log("🐟 BITE DETECTED! Vibrating physical phone!");
                #if UNITY_ANDROID && !UNITY_EDITOR
                Handheld.Vibrate(); // Triggers the heavy S24 hardware rumble
                #endif
            });
        }
    }

    public void PublishPrompt(string text)
    {
        if (client != null && client.IsConnected)
        {
            client.Publish("rika/prompt", Encoding.UTF8.GetBytes(text), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        }
    }

    public void PublishTouch(string jsonPayload)
    {
        if (client != null && client.IsConnected)
        {
            client.Publish("rika/phone/touch", Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        }
    }

    public void PublishCast(Vector2 velocity)
    {
        if (client != null && client.IsConnected)
        {
            string json = JsonUtility.ToJson(velocity);
            client.Publish("rika/game/fishing/cast", Encoding.UTF8.GetBytes(json), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        }
    }

    public void PublishImage(byte[] imageBytes)
    {
        if (client != null && client.IsConnected)
        {
            // Sending raw bytes directly over MQTT. Fast and clean.
            client.Publish("rika/vision/image_raw", imageBytes, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            Debug.Log($"[Margo Bridge] Uploaded {imageBytes.Length} bytes to Gemini.");
        }
    }

    public void PublishMessage(string topic, string payload, bool retain = false)
    {
        if (client != null && client.IsConnected)
        {
            client.Publish(topic, Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, retain);
            Debug.Log($"[Margo Bridge] Published '{payload}' to '{topic}'");
        }
    }
}