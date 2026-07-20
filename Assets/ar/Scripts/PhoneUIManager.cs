using UnityEngine;

public class PhoneUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject standardUIPanel;
    public GameObject trackpadBlackScreen;

    void Start()
    {
        // FAILSAFE: Always force the standard UI on when the app boots up,
        // just in case we have no Wi-Fi or left the wrong screen on in the Editor!
        if (standardUIPanel != null) standardUIPanel.SetActive(true);
        if (trackpadBlackScreen != null) trackpadBlackScreen.SetActive(false);
    }

    // Called by the MQTT Bridge when the VR headset is confirmed ON
    public void SwitchToTrackpad()
    {
        if (standardUIPanel != null) standardUIPanel.SetActive(false);
        if (trackpadBlackScreen != null) trackpadBlackScreen.SetActive(true);
        Debug.Log("[UI] Swapped to VR Trackpad Mode");
    }

    // Called by the MQTT Bridge when the VR headset is confirmed OFF
    public void SwitchToStandardUI()
    {
        if (trackpadBlackScreen != null) trackpadBlackScreen.SetActive(false);
        if (standardUIPanel != null) standardUIPanel.SetActive(true);
        Debug.Log("[UI] Swapped to Standard Button Mode");
    }
}