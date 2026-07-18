using UnityEngine;

public class PhoneUIManager : MonoBehaviour
{
    [Header("UI Modes")]
    [Tooltip("The parent GameObject containing all your standard app buttons.")]
    public GameObject standardUIPanel;
    
    [Tooltip("The pitch-black canvas panel that acts as the VR trackpad.")]
    public GameObject trackpadBlackScreen;

    [Header("Components to Toggle")]
    [Tooltip("Drag your Main Camera here so we can turn off the trackpad script when VR is off.")]
    public PhoneTrackpad trackpadScript;

    private void Start()
    {
        PhoneMqttBridge.Instance.OnVRStatusChanged += HandleVRStatusChange;
        
        // Default to Standard UI on boot until the broker tells us otherwise
        SetUIMode(false);
    }

    private void OnDestroy()
    {
        if (PhoneMqttBridge.Instance != null)
        {
            PhoneMqttBridge.Instance.OnVRStatusChanged -= HandleVRStatusChange;
        }
    }

    private void HandleVRStatusChange(bool isVrOnline)
    {
        SetUIMode(isVrOnline);
    }

    private void SetUIMode(bool vrIsActive)
    {
        if (standardUIPanel != null) 
            standardUIPanel.SetActive(!vrIsActive); // On when VR is off
            
        if (trackpadBlackScreen != null) 
            trackpadBlackScreen.SetActive(vrIsActive); // On when VR is on

        if (trackpadScript != null)
            trackpadScript.enabled = vrIsActive; // Only capture thumb swipes when VR is active
    }
}