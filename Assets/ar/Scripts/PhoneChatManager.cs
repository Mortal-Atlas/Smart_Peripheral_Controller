using UnityEngine;
using TMPro;

public class PhoneChatManager : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("The TextMeshProUGUI element that displays the chat history.")]
    public TextMeshProUGUI chatHistoryText;
    
    [Tooltip("The TMP_InputField where you type your message to Margo.")]
    public TMP_InputField chatInputField;

    private void OnEnable()
    {
        // Listen to the bridge for incoming AI responses
        if (PhoneMqttBridge.Instance != null)
        {
            PhoneMqttBridge.Instance.OnChatReceived += HandleAIResponse;
        }
    }

    private void OnDisable()
    {
        if (PhoneMqttBridge.Instance != null)
        {
            PhoneMqttBridge.Instance.OnChatReceived -= HandleAIResponse;
        }
    }

    private void HandleAIResponse(string message)
    {
        if (chatHistoryText != null)
        {
            // Append Margo's response to the history log
            chatHistoryText.text += $"\n\n<b>Margo:</b> {message}";
        }
    }

    // Link this method to your physical UI Send Button's OnClick() event!
    public void SendPrompt()
    {
        if (chatInputField != null && !string.IsNullOrWhiteSpace(chatInputField.text))
        {
            string textToSend = chatInputField.text;
            
            if (chatHistoryText != null)
            {
                // Instantly append your own message to the history so you know it sent
                chatHistoryText.text += $"\n\n<b>You:</b> {textToSend}";
            }

            // Fire it over the network to the Raspberry Pi
            PhoneMqttBridge.Instance.PublishPrompt(textToSend);
            
            // Clear the input box so you can type the next message
            chatInputField.text = ""; 
        }
    }
}