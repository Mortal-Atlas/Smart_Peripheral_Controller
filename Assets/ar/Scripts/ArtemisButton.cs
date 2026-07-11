using UnityEngine;

public class ArtemisButton : MonoBehaviour
{
    [Header("MQTT Settings")]
    [Tooltip("The MQTT topic Home Assistant will listen to")]
    public string topic = "vr/artemis/toggle";
    
    [Tooltip("The message to send")]
    public string payload = "TOGGLE";

    // This is the function the standard UI button will trigger
    public void ToggleSwitch()
    {
        // We use the Singleton instance of the NetworkController we built earlier
        if (NetworkController.Instance != null)
        {
            NetworkController.Instance.PublishCommand(topic, payload);
            Debug.Log("Artemis UI Button Pressed: Command sent to broker.");
        }
        else
        {
            Debug.LogError("NetworkController is missing from the scene!");
        }
    }
}