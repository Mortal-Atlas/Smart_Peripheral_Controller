using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhoneCameraManager : MonoBehaviour
{
    [Header("AR Window")]
    [Tooltip("Drag your UI RawImage here to display the camera feed.")]
    public RawImage arDisplaySquare;

    private WebCamTexture backCamera;
    private bool isCameraReady = false;

    void Start()
    {
        PhoneMqttBridge.Instance.OnSnapRequested += ExecuteSnap;
        StartCoroutine(InitializeCamera());
    }

    private IEnumerator InitializeCamera()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogError("[Camera] Permission denied. I am blind.");
            yield break;
        } // <--- THIS bracket was missing!

        // Find the back camera
        foreach (var device in WebCamTexture.devices)
        {
            if (!device.isFrontFacing)
            {
                // Increased resolution slightly to make the AR window look good on the S24
                backCamera = new WebCamTexture(device.name, 1280, 720, 30);
                
                // Pipe the camera feed directly into your UI Square
                if (arDisplaySquare != null)
                {
                    arDisplaySquare.texture = backCamera;
                }

                backCamera.Play(); 
                isCameraReady = true;

                // Adjust orientation so the camera feed isn't sideways
                if (arDisplaySquare != null)
                {
                    arDisplaySquare.rectTransform.localEulerAngles = new Vector3(0, 0, -backCamera.videoRotationAngle);
                }

                Debug.Log("[Camera] S24 Ultra optics primed and AR window active.");
                break;
            }
        }
    }

    private void ExecuteSnap()
    {
        if (!isCameraReady || !backCamera.isPlaying) return;

        Texture2D snap = new Texture2D(backCamera.width, backCamera.height);
        snap.SetPixels(backCamera.GetPixels());
        snap.Apply();

        byte[] imageBytes = snap.EncodeToJPG(50);
        Destroy(snap);

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