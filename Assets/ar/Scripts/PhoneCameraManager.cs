using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class PhoneCameraManager : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage arDisplaySquare; 
    
    [Header("AR Foundation")]
    [Tooltip("Drag the AR Session GameObject from your Hierarchy here")]
    public ARSession arSession;

    private bool isCameraActive = false;

    void Start()
    {
        // Start with the box looking like a powered-off screen
        if (arDisplaySquare != null)
        {
            arDisplaySquare.color = Color.black;
        }

        // Keep AR off on boot to save battery until summoned!
        if (arSession != null)
        {
            arSession.enabled = false;
        }

        // We moved this to Start() and added a null check to prevent the execution order crash!
        if (PhoneMqttBridge.Instance != null)
        {
            PhoneMqttBridge.Instance.OnSnapRequested += ExecuteSnap;
        }
    }

    public void ToggleCamera()
    {
        isCameraActive = !isCameraActive;

        if (isCameraActive)
        {
            if (arSession != null) arSession.enabled = true;
            if (arDisplaySquare != null) arDisplaySquare.color = Color.white; 
            Debug.Log("[AR Camera] Tracking Initialized.");
        }
        else
        {
            if (arSession != null) arSession.enabled = false;
            // Note: We don't make the texture 'null' anymore because the Render Texture stays assigned!
            if (arDisplaySquare != null) arDisplaySquare.color = Color.black; 
            Debug.Log("[AR Camera] Tracking Powered Down.");
        }
    }

    public void ExecuteSnap()
    {
        if (!isCameraActive || arDisplaySquare == null || arDisplaySquare.texture == null) 
        {
            Debug.LogWarning("[AR Camera] Cannot snap! AR window is off.");
            return;
        }

        // Extract the active Render Texture from the UI square
        RenderTexture rt = arDisplaySquare.texture as RenderTexture;
        if (rt != null)
        {
            // Set the active Render Texture so we can read its pixels
            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = rt;

            // Create a new blank 2D texture and copy the pixels over
            Texture2D snap = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            snap.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            snap.Apply();

            // Restore the old state
            RenderTexture.active = currentActiveRT;

            // Compress to JPG and send to Gemini!
            byte[] imageBytes = snap.EncodeToJPG(50);
            Destroy(snap);

            if (PhoneMqttBridge.Instance != null)
            {
                PhoneMqttBridge.Instance.PublishImage(imageBytes);
                Debug.Log("[AR Camera] AR Frame captured and sent to Gemini.");
            }
        }
    }

    private void OnDestroy()
    {
        // Always clean up our listeners so we don't cause memory leaks
        if (PhoneMqttBridge.Instance != null)
        {
            PhoneMqttBridge.Instance.OnSnapRequested -= ExecuteSnap;
        }
    }
}