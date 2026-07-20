using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PhoneChatManager : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("The TextMeshProUGUI element that displays the chat history.")]
    public TextMeshProUGUI chatHistoryText;
    
    [Tooltip("The TMP_InputField where you type your message to Margo.")]
    public TMP_InputField chatInputField;

    [Tooltip("The ScrollRect component that contains the chat history.")]
    public ScrollRect chatScrollRect;

    [Header("Settings")]
    public float typeDelay = 0.03f; // Speed of the typewriter effect

    private Queue<string> messageQueue = new Queue<string>();
    private bool isTyping = false;

    private void Start()
    {
        // CLEAR the placeholder text as soon as the app boots!
        if (chatHistoryText != null)
        {
            chatHistoryText.text = "";
        }

        if (PhoneMqttBridge.Instance != null)
        {
            PhoneMqttBridge.Instance.OnChatReceived += HandleAIResponse;
        }
        else
        {
            Debug.LogError("[ChatManager] Could not find PhoneMqttBridge in the scene!");
        }
    }

    private void OnDestroy()
    {
        if (PhoneMqttBridge.Instance != null)
        {
            PhoneMqttBridge.Instance.OnChatReceived -= HandleAIResponse;
        }
    }

    private void HandleAIResponse(string message)
    {
        // Format the message and add it to the queue
        string formattedMessage = $"\n\n<b>Margo:</b> {message}";
        messageQueue.Enqueue(formattedMessage);

        // If we aren't already typing something, start the coroutine
        if (!isTyping)
        {
            StartCoroutine(TypewriterEffect());
        }
    }

    public void SendPrompt()
    {
        if (chatInputField != null && !string.IsNullOrWhiteSpace(chatInputField.text))
        {
            string textToSend = chatInputField.text;
            
            // Instantly append your own message (no typewriter for the user)
            if (chatHistoryText != null)
            {
                chatHistoryText.text += $"\n\n<b>You:</b> {textToSend}";
            }
            
            // Fire it over the network
            PhoneMqttBridge.Instance.PublishPrompt(textToSend);
            
            // Clear input
            chatInputField.text = ""; 
            
            // Force the scrollbar to the bottom instantly
            StartCoroutine(ForceScrollDown());
        }
    }

    private IEnumerator TypewriterEffect()
    {
        isTyping = true;

        // CRASH PREVENTION: Make sure the UI slot is actually filled in the Inspector!
        if (chatHistoryText == null)
        {
            Debug.LogError("[ChatManager] ERROR: You forgot to assign the Chat History Text in the Inspector!");
            isTyping = false;
            yield break; // Stop the coroutine before it crashes
        }

        while (messageQueue.Count > 0)
        {
            string currentMessage = messageQueue.Dequeue();
            
            // Type out character by character
            foreach (char c in currentMessage)
            {
                chatHistoryText.text += c;
                
                // Keep the scrollbar pushed to the bottom while typing
                if (chatScrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    chatScrollRect.verticalNormalizedPosition = 0f;
                }
                
                yield return new WaitForSeconds(typeDelay);
            }
        }

        isTyping = false;
    }

    private IEnumerator ForceScrollDown()
    {
        yield return new WaitForEndOfFrame();
        if (chatScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }
}