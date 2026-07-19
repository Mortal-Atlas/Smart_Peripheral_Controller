using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

[System.Serializable]
public class VoiceSettings
{
    public float stability = 0.5f;
    public float similarity_boost = 0.75f;
}

[System.Serializable]
public class TTSRequestData
{
    public string text;
    // We use the flash/monolingual model for the lowest latency possible
    public string model_id = "eleven_monolingual_v1"; 
    public VoiceSettings voice_settings;
}

public class PhoneElevenLabsTTS : MonoBehaviour
{
    [Header("ElevenLabs Config")]
    [Tooltip("The ID of the specific voice you want to use (Default is a British female voice)")]
    public string voiceId = "EXAVITQu4vr4xnSDxMaL"; // Example Voice ID, change as needed
    
    // Hidden from the Inspector, loaded securely at runtime so it never goes to GitHub
    private string apiKey = "";

    [Header("Audio")]
    [Tooltip("The AudioSource that will play the voice on the phone")]
    public AudioSource audioSource;

    private void Awake()
    {
        LoadAPIKey();
    }

    private void LoadAPIKey()
    {
        // Unity's Resources.Load automatically looks in any folder named "Resources"
        TextAsset keyFile = Resources.Load<TextAsset>("elevenlabs_secret");
        
        if (keyFile != null)
        {
            apiKey = keyFile.text.Trim(); // .Trim() removes any accidental spaces or newlines
            Debug.Log("[ElevenLabs] API Key loaded successfully from Resources.");
        }
        else
        {
            Debug.LogError("[ElevenLabs] API Key missing! Please create a text file at Assets/Resources/elevenlabs_secret.txt");
        }
    }

    private void OnEnable()
    {
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

    private void HandleAIResponse(string text)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogWarning("[ElevenLabs] Cannot synthesize speech. API key is missing.");
            return;
        }

        // Start the web request to download the audio file
        StartCoroutine(SynthesizeAndPlay(text));
    }

    private IEnumerator SynthesizeAndPlay(string text)
    {
        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}";

        // Package up the text and settings into an object
        TTSRequestData requestData = new TTSRequestData
        {
            text = text,
            model_id = "eleven_monolingual_v1",
            voice_settings = new VoiceSettings()
        };

        // Convert the object to a JSON string, and then into raw bytes
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        // Create the POST request
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            
            // We tell Unity to expect an MP3 (MPEG) audio file back
            www.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
            
            // Set the necessary headers for ElevenLabs
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("xi-api-key", apiKey);
            www.SetRequestHeader("Accept", "audio/mpeg");

            // Send the request and wait for the download to finish
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[ElevenLabs] API Error: {www.error}\n{www.downloadHandler.text}");
            }
            else
            {
                // Extract the audio clip from the downloaded data
                AudioClip downloadedClip = DownloadHandlerAudioClip.GetContent(www);
                
                // Play it immediately through the phone's speakers
                if (audioSource != null && downloadedClip != null)
                {
                    audioSource.clip = downloadedClip;
                    audioSource.Play();
                }
            }
        }
    }
}