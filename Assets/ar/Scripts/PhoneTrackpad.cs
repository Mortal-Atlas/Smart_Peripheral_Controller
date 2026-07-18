using UnityEngine;

[System.Serializable]
public class PhoneTouchPayload
{
    public float tx;
    public float ty;
    public bool isTouching;
}

public class PhoneTrackpad : MonoBehaviour
{
    private PhoneTouchPayload payload = new PhoneTouchPayload();

    // Fishing Swipe Variables
    private Vector2 swipeStartPos;
    private float swipeStartTime;

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            // --- Fishing Swipe Detection ---
            if (touch.phase == TouchPhase.Began)
            {
                swipeStartPos = touch.position;
                swipeStartTime = Time.time;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                Vector2 swipeDelta = touch.position - swipeStartPos;
                float swipeTime = Time.time - swipeStartTime;
                
                // If it was a fast swipe (less than 0.5 seconds)
                if (swipeTime > 0.05f && swipeTime < 0.5f)
                {
                    Vector2 velocity = swipeDelta / swipeTime;
                    
                    // Normalize velocity based on screen size so it's consistent
                    velocity.x /= Screen.width;
                    velocity.y /= Screen.height;

                    // If it's a strong swipe (magnitude threshold)
                    if (velocity.magnitude > 1.5f)
                    {
                        PhoneMqttBridge.Instance.PublishCast(velocity);
                    }
                }
            }
            // -------------------------------

            // Normalize coordinates between 0.0 and 1.0 based on actual screen size
            payload.tx = touch.position.x / Screen.width;
            payload.ty = touch.position.y / Screen.height;
            payload.isTouching = (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled);

            SendTouchData();
        }
        else if (payload.isTouching) // Only send the 'release' frame once to save bandwidth
        {
            payload.isTouching = false;
            SendTouchData();
        }
    }

    private void SendTouchData()
    {
        string json = JsonUtility.ToJson(payload);
        PhoneMqttBridge.Instance.PublishTouch(json);
    }
}