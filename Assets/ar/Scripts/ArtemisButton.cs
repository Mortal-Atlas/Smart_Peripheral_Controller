using UnityEngine;
using UnityEngine.UI;

public class ArtemisButton : MonoBehaviour
{
    [Header("Home Assistant MQTT Settings")]
    [Tooltip("The exact MQTT topic that Home Assistant is listening to for this plug.")]
    public string mqttTopic = "rika/haos/artemis/toggle";
    
    [Tooltip("The command to send. Usually 'toggle', 'on', or 'off'.")]
    public string commandPayload = "toggle";

    /// <summary>
    /// Link this method to your UI Button's OnClick() event in the Inspector.
    /// </summary>
    public void OnButtonToggled()
    {
        if (PhoneMqttBridge.Instance != null)
        {
            // Instantly blasts the toggle command to the Raspberry Pi
            PhoneMqttBridge.Instance.PublishMessage(mqttTopic, commandPayload);
            
            // Optional: Give the phone a tiny physical buzz so you know you tapped it
            #if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
            #endif
        }
        else
        {
            Debug.LogWarning("[Artemis] Cannot toggle plug. MQTT Bridge is offline.");
        }
    }
}