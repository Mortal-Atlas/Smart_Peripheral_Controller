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
    public string model_id = "eleven_turbo_v2_5"; // Upgraded to the fastest AI chatbot model
    public VoiceSettings voice_settings;
}

public class PhoneElevenLabsTTS : MonoBehaviour
{
    [Header("ElevenLabs Config")]
    public string voiceId = "EXAVITQu4vr4xnSDxMaL"; 
    private string apiKey = "";

    [Header("Audio")]
    public AudioSource audioSource;

    private void Awake()
    {
        LoadAPIKey();
    }

    private void Start()
    {
        if (PhoneMqttBridge.Instance != null)
        {
            PhoneMqttBridge.Instance.OnChatReceived += HandleAIResponse;
        }
    }

    private void LoadAPIKey()
    {
        TextAsset keyFile = Resources.Load<TextAsset>("elevenlabs_secret");
        if (keyFile != null)
        {
            apiKey = keyFile.text.Trim(); 
        }
        else
        {
            Debug.LogError("[ElevenLabs] API Key missing! Assets/Resources/elevenlabs_secret.txt");
        }
    }

    private void OnDestroy()
    {
        if (PhoneMqttBridge.Instance != null)
        {
            PhoneMqttBridge.Instance.OnChatReceived -= HandleAIResponse;
        }
    }

    private void HandleAIResponse(string text)
    {
        if (string.IsNullOrEmpty(apiKey)) return;
        StartCoroutine(SynthesizeAndPlay(text));
    }

    private IEnumerator SynthesizeAndPlay(string text)
    {
        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}";

        TTSRequestData requestData = new TTSRequestData
        {
            text = text,
            model_id = "eleven_turbo_v2_5",
            voice_settings = new VoiceSettings()
        };

        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            www.method = "POST";
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            
            // FIX: Force Content-Type directly onto the Unity 6 UploadHandler
            www.uploadHandler.contentType = "application/json"; 
            
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("xi-api-key", apiKey);
            www.SetRequestHeader("Accept", "audio/mpeg");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ElevenLabs] API Error: {www.error}");
                
                // NEW: This intercepts ElevenLabs' specific JSON error message (e.g. "Voice not found")
                if (www.downloadHandler != null && !string.IsNullOrEmpty(www.downloadHandler.text))
                {
                    Debug.LogError($"[ElevenLabs] Detailed Response: {www.downloadHandler.text}");
                }
            }
            else
            {
                AudioClip downloadedClip = DownloadHandlerAudioClip.GetContent(www);
                if (audioSource != null && downloadedClip != null)
                {
                    audioSource.clip = downloadedClip;
                    audioSource.Play();
                }
            }
        }
    }
}