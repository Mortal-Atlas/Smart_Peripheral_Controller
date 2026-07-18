using UnityEngine;
using System.Collections;

public class PhoneCameraManager : MonoBehaviour
{
    private WebCamTexture backCamera;
    private bool isCameraReady = false;

    void Start()
    {
        // Subscribe to the network bridge's SNAP event
        PhoneMqttBridge.Instance.OnSnapRequested += ExecuteSnap;

        StartCoroutine(InitializeCamera());
    }

    private IEnumerator InitializeCamera()
    {
        // Wait for user to grant Android permissions
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogError("[Camera] Permission denied. I am blind.");
            yield break;
        }

        // Find the back camera
        foreach (var device in WebCamTexture.devices)
        {
            if (!device.isFrontFacing)
            {
                // Request a low resolution to keep the MQTT payload small and fast
                backCamera = new WebCamTexture(device.name, 800, 600, 30);
                backCamera.Play(); // Starts the camera in memory, but DOES NOT render to the screen!
                isCameraReady = true;
                Debug.Log("[Camera] S24 Ultra optics primed and running silently.");
                break;
            }
        }
    }

    private void ExecuteSnap()
    {
        if (!isCameraReady || !backCamera.isPlaying) return;

        // Create a Texture2D to read the pixels from the WebCamTexture
        Texture2D snap = new Texture2D(backCamera.width, backCamera.height);
        snap.SetPixels(backCamera.GetPixels());
        snap.Apply();

        // Encode to JPG with 50% quality to keep the file size tiny for network transfer
        byte[] imageBytes = snap.EncodeToJPG(50);
        
        // Destroy the Texture2D from memory immediately to prevent memory leaks
        Destroy(snap);

        // Send the raw bytes to the Pi via MQTT
        PhoneMqttBridge.Instance.PublishImage(imageBytes);
    }

    void OnDestroy()
    {
        if (PhoneMqttBridge.Instance != null)
        {
            PhoneMqttBridge.Instance.OnSnapRequested -= ExecuteSnap;
        }
        if (backCamera != null) backCamera.Stop();
    }
}