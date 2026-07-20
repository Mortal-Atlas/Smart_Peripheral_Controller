using UnityEngine;
using TMPro;
using System;

public class PhoneDashboard : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI weatherText;

    [Header("AR Toggle")]
    public PhoneCameraManager cameraManager;
    private bool isMargoSummoned = false;

    [Header("Network")]
    public PhoneMqttBridge mqttBridge;

    private float minuteTimer = 0f;

    void Start()
    {
        if (mqttBridge == null) mqttBridge = PhoneMqttBridge.Instance;
        
        // Initial updates
        UpdateTime();
        if (weatherText != null) weatherText.text = "72°F | Clear"; 
    }

    void Update()
    {
        minuteTimer += Time.deltaTime;
        if (minuteTimer > 1f)
        {
            UpdateTime();
            minuteTimer = 0f;
        }
    }

    private void UpdateTime()
    {
        if (timeText != null)
        {
            timeText.text = DateTime.Now.ToString("h:mm tt");
        }
    }

    // --- QUICK ACTION BUTTON EVENTS ---
    
    public void OnQuickFeedPressed()
    {
        Debug.Log("[Dashboard] Quick Feed activated.");
        if (mqttBridge != null) mqttBridge.PublishMessage("rika/pet/action", "quick_feed");
    }

    public void OnQuickEquipPressed()
    {
        Debug.Log("[Dashboard] Quick Equip activated.");
        if (mqttBridge != null) mqttBridge.PublishMessage("rika/pet/action", "quick_equip");
    }

    public void OnRestoreEnergyPressed()
    {
        Debug.Log("[Dashboard] Restoring Energy.");
        if (mqttBridge != null) mqttBridge.PublishMessage("rika/pet/action", "restore_energy");
    }

    public void OnSummonTogglePressed()
    {
        isMargoSummoned = !isMargoSummoned;
        Debug.Log($"[Dashboard] Summon toggled: {isMargoSummoned}");
        
        // 1. Toggle local Android camera
        if (cameraManager != null)
        {
            cameraManager.ToggleCamera();
        }

        // 2. Tell VR Headset to show/hide Margo physically
        string commandStr = isMargoSummoned ? "materialize" : "poof";
        string payload = $"{{\"command\": \"{commandStr}\"}}";
        
        if (mqttBridge != null) mqttBridge.PublishMessage("rika/commands", payload);
    }
}