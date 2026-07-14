using UnityEngine;
using System.Text;

[System.Serializable]
public class TouchPayload
{
    public float tx, ty; // Touch X, Y (0 to 1)
    public bool isTouching;
}

public class TouchpadBroadcaster : MonoBehaviour
{
    public NetworkController networkController;

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            // Normalize coordinates: 0.0 to 1.0
            float normX = touch.position.x / Screen.width;
            float normY = touch.position.y / Screen.height;

            TouchPayload payload = new TouchPayload { tx = normX, ty = normY, isTouching = true };
            networkController.PublishTelemetry("rika/phone/touch", JsonUtility.ToJson(payload));
  
            // Local Feedback: Vibrate the phone physically
            Handheld.Vibrate();
        }
    }
}