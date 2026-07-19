using UnityEngine;
using TMPro;
using System;

public class PhoneDashboard : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI weatherText;

    [Header("Network")]
    public PhoneMqttBridge mqttBridge;

    private float minuteTimer = 0f;

    void Start()
    {
        if (mqttBridge == null) mqttBridge = PhoneMqttBridge.Instance;
        
        // Initial updates
        UpdateTime();
        weatherText.text = "72°F | Clear"; // Placeholder until we link your Pi's weather integration
    }

    void Update()
    {
        // Only update the clock string once every ~60 frames to save CPU
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
        // Sends a command to the headset to top off the familiar's health/hunger
        mqttBridge.PublishMessage("rika/pet/action", "quick_feed");
    }

    public void OnQuickEquipPressed()
    {
        Debug.Log("[Dashboard] Quick Equip activated.");
        // Sends a command to auto-equip the highest tier item
        mqttBridge.PublishMessage("rika/pet/action", "quick_equip");
    }

    public void OnRestoreEnergyPressed()
    {
        Debug.Log("[Dashboard] Restoring Energy.");
        mqttBridge.PublishMessage("rika/pet/action", "restore_energy");
    }
}