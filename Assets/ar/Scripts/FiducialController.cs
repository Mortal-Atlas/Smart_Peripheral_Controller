using UnityEngine;
using UnityEngine.EventSystems; // Required for Touch Interfaces

// Notice we added IPointerUpHandler to detect the end of the swipe!
public class FiducialController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Vector2 touchStartPos;
    private float touchStartTime;

    void Start()
    {
        // 1. Wake up the S24 Ultra's hardware gyroscope
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            Debug.Log("S24 Ultra Gyroscope Online.");
        }
        else
        {
            Debug.LogWarning("Gyroscope not supported on this device.");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 2. Record the exact pixel and millisecond your thumb hits the glass
        touchStartPos = eventData.position;
        touchStartTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 3. The exact moment you lift your thumb (The Cast!)
        Vector2 touchEndPos = eventData.position;
        float duration = Time.time - touchStartTime;

        // --- CALCULATE POWER (Thumb Swipe Velocity) ---
        float distance = Vector2.Distance(touchStartPos, touchEndPos);
        
        // Prevent division by zero if you just tapped instantly
        float safeDuration = duration > 0.01f ? duration : 0.01f; 
        float rawSwipeSpeed = distance / safeDuration;

        // Normalize the speed to a 0.0 to 1.0 multiplier for the Quest 3 physics engine
        // (You may need to tweak the 5000f divisor based on how hard you swipe)
        float normalizedPower = Mathf.Clamp(rawSwipeSpeed / 5000f, 0.1f, 1.0f);

        // --- CALCULATE ANGLE (Hardware Gyroscope) ---
        // Read the pitch of the physical phone. If gyro fails, default to 45 degrees.
        float phonePitch = Input.gyro.enabled ? Input.gyro.attitude.eulerAngles.x : 45.0f;

        // --- CONSTRUCT AND FIRE THE PAYLOAD ---
        string jsonPayload = $"{{\"action\":\"cast\", \"power\":{normalizedPower:F2}, \"angle\":{phonePitch:F1}}}";
        
        Debug.Log("🔥 Firing Sensor Fusion Payload: " + jsonPayload);

        // Blast it across the Tailscale VPN to the Pi
        if (NetworkController.Instance != null)
        {
            NetworkController.Instance.PublishCommand("home/quest3/action", jsonPayload);
        }
    }
}