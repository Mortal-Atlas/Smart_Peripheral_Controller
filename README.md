Margo Companion - Phone Controller 📱

Welcome to the peripheral companion app for the Margo VR project. This application runs on the Samsung Galaxy S24 Ultra, which is physically mounted to the Meta Quest 3 controller.

Instead of relying on clunky virtual laser pointers, this app turns a high-end smartphone into a zero-latency haptic trackpad and wireless sensor suite for the VR headset.

🛠️ Tech Stack

Platform: Android (Samsung S24 Ultra)

Networking: MQTT Client, Tailscale mesh VPN

Camera: Local IP Webcam Server (MJPEG over HTTP)

📡 Core Responsibilities

Haptic Trackpad: Captures raw 0.0 to 1.0 X/Y coordinate touch data and broadcasts it over MQTT to drive the VR cursor.

Camera Streaming: Hosts an ultra-low latency HTTP MJPEG stream (/shot.jpg) that the VR headset decodes and projects holographically.

Physical Triggers: Maps hardware volume/power buttons to specific MQTT event payloads.

🔮 Future Implementations (Phone Client)

Native Camera API Integration: Replace the generic IP Webcam server with a custom camera wrapper that can capture high-res stills on command (when the VR user pulls the trigger) and save them to the Pi for the Gemini Vision API.

Advanced Touch Gestures (Fishing): Calculate raw swipe velocity and acceleration magnitude on the phone side, sending a clean Vector2 payload to the VR app for throwing the fishing bobber.

Multi-Touch Combat Zones: Divide the phone screen into distinct, invisible interaction zones (e.g., Top Left = Light Attack, Bottom Right = Heavy Attack) that publish discrete tactical commands for the VR combat app.

Haptic Feedback Hooks: Subscribe to MQTT topics to trigger the S24's vibration motors (e.g., when a fish "bites" the bobber in VR, the physical phone vibrates violently).
