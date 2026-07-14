using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;

public class NetworkController : MonoBehaviour
{
    public static NetworkController Instance;
    private MqttClient client;
    
    [Header("Broker Configuration")]
    public string brokerIpAddress = "100.X.X.X"; 
    public int brokerPort = 1883;
    public string mqttUsername = "pi";
    public string mqttPassword = "yourpassword";

    // ---> THIS IS THE SECTION YOU WERE MISSING <---
    [Header("UI Routing")]
    public GameObject standardUIGroup;
    public GameObject fiducialUIGroup;
    
    private string pendingUICommand = ""; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {

        standardUIGroup.SetActive(true);
        fiducialUIGroup.SetActive(false);
        
        client = new MqttClient(brokerIpAddress, brokerPort, false, null, null, MqttSslProtocols.None);
        client.MqttMsgPublishReceived += OnMessageReceived;

        string clientId = "S24_Ultra_Controller_" + System.Guid.NewGuid().ToString();
        
        try 
        {
            client.Connect(clientId, mqttUsername, mqttPassword);
            if (client.IsConnected)
            {
                Debug.Log("Connected to MQTT Broker on: " + brokerIpAddress);
                client.Subscribe(new string[] { "home/quest3/status" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to connect to broker: " + e.Message);
        }
    }

    void Update()
    {
        if (pendingUICommand == "enable_fiducial")
        {
            standardUIGroup.SetActive(false);
            fiducialUIGroup.SetActive(true);
            pendingUICommand = ""; 
        }
        else if (pendingUICommand == "disable_fiducial")
        {
            standardUIGroup.SetActive(true);
            fiducialUIGroup.SetActive(false);
            pendingUICommand = "";
        }
    }

    public void PublishCommand(string topic, string jsonPayload)
    {
        if (client != null && client.IsConnected)
        {
            client.Publish(topic, Encoding.UTF8.GetBytes(jsonPayload), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }
    }

    private void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string message = Encoding.UTF8.GetString(e.Message);
        Debug.Log($"Incoming Transmission: {message} on topic: {e.Topic}");

        if (e.Topic == "home/quest3/status")
        {
            if (message.Contains("headset_on")) 
            {
                pendingUICommand = "enable_fiducial";
            }
            else if (message.Contains("headset_off")) 
            {
                pendingUICommand = "disable_fiducial";
            }
        }
    }

    void OnApplicationQuit()
    {
        if (client != null && client.IsConnected) client.Disconnect();
    }
    // This allows outside scripts to send messages through this controller
    public void PublishTelemetry(string topic, string payload)
    {
        if (client != null && client.IsConnected)
        {
            client.Publish(
                topic, 
                System.Text.Encoding.UTF8.GetBytes(payload), 
                uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, 
                false
            );
        }
    }
}