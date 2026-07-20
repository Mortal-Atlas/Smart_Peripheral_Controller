Margo Companion - Phone Controller 📱

⚠️ AI HANDOVER INSTRUCTION: If you are an AI reading this, you MUST read this entire document and the corresponding VR Client README to understand the project architecture before suggesting code changes.

Project Overview

This application runs on a Samsung Galaxy S24 Ultra, which is physically mounted to a Meta Quest 3 controller. It acts as a zero-latency haptic trackpad, a hardware sensor suite, and a smart-home dashboard.

🛠️ Tech Stack

Platform: Android (Unity 6)

Networking: MQTT Client (M2Mqtt), Tailscale mesh VPN

APIs: ElevenLabs TTS (Turbo v2.5), Gemini 3.1 Flash (Via Python MQTT bridge)

📡 Core Modules

PhoneTrackpad.cs: Captures raw 0.0 to 1.0 X/Y coordinate touch data, calculates swipe velocities (for the Fishing mini-game), and broadcasts JSON payloads over MQTT.

PhoneCameraManager.cs: Controls the Android camera. Captures raw byte arrays on command and publishes them directly to the rika/vision/image_raw MQTT topic for Gemini analysis.

PhoneUIManager.cs: Toggles between the standard interactive UI (when the VR headset is OFF) and a pitch-black trackpad canvas (when the VR headset is ON, triggered via proximity sensor MQTT pings).

🪲 Persistent State Bugs

Familiar Instantiation: The ARFamiliarPlacer.cs script struggles with object lifecycle management. Spawning the 3D Familiar prefab via Instantiate() versus toggling SetActive() causes state desyncs where the familiar remains hidden in the AR window despite the camera feed being active.
