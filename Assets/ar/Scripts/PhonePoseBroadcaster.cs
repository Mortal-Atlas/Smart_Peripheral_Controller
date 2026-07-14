using UnityEngine;
using M2MqttUnity;
using System.Text;

[System.Serializable]
public class PhonePoseData
{
    public float px, py, pz;
    public float qx, qy, qz, qw;
}

public class PhonePoseBroadcaster : MonoBehaviour
{
    [Header("MQTT Setup")]
    [Tooltip("Drag the GameObject holding your NetworkController here")]
    public NetworkController networkController;

    [Header("AR Setup")]
    [Tooltip("Drag the new 'ScreenCenter' child object here, NOT the camera!")]
    public Transform phoneCenterPivot;

    [Tooltip("Use this to fix flipped axes! If the phone is backwards, try setting Y to 180.")]
    public Vector3 rotationCorrection = new Vector3(0, 180, 0);

    void Update()
    {
        // Safety check
        if (networkController == null || phoneCenterPivot == null)
            return;

        // Apply our correction offset to flip the phone around so it faces the right way
        Quaternion correctedRotation = phoneCenterPivot.rotation * Quaternion.Euler(rotationCorrection);

        // Blast the data every single frame. The S24 Ultra can easily handle this.
        PhonePoseData pose = new PhonePoseData
        {
            px = phoneCenterPivot.position.x,
            py = phoneCenterPivot.position.y,
            pz = phoneCenterPivot.position.z,
            qx = correctedRotation.x,
            qy = correctedRotation.y,
            qz = correctedRotation.z,
            qw = correctedRotation.w
        };

        string json = JsonUtility.ToJson(pose);
        networkController.PublishTelemetry("rika/phone/pose", json);
    }
}