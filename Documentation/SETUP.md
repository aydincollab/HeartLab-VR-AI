# HeartLab VR AI - Setup Guide

This guide will walk you through the complete setup process for the HeartLab VR AI system.

## Prerequisites

### Software Requirements
- **Unity 2022.3 LTS** or later
- **Visual Studio 2022** or **Visual Studio Code** with C# extension
- **Git** for version control
- **Android SDK** (for Meta Quest builds)
- **SteamVR** (for PC VR testing)

### Hardware Requirements

#### Minimum VR Hardware
- **Meta Quest 2** or newer
- **PC VR Headset** (Index, Vive, Rift S, etc.)
- **6DOF Controllers** with haptic feedback
- **Room-scale tracking space** (2m x 2m minimum)

#### Recommended PC Specifications
```yaml
CPU: Intel i7-8700K / AMD Ryzen 7 2700X or better
GPU: NVIDIA GTX 1070 / AMD RX 580 or better
RAM: 16 GB DDR4
Storage: 10 GB available space (SSD recommended)
USB: USB 3.0 ports for VR headset connection
```

### Cloud Services (Optional)
- **Azure Cognitive Services** account for speech features
- **Hugging Face** account for AI model access

## Step 1: Unity Setup

### 1.1 Install Unity Hub
```bash
# Download from Unity official website
https://unity3d.com/get-unity/download

# Or via command line (Windows)
winget install Unity.UnityHub

# Or via Homebrew (macOS)
brew install --cask unity-hub
```

### 1.2 Install Unity 2022.3 LTS
1. Open Unity Hub
2. Click "Installs" tab
3. Click "Install Editor"
4. Select "Unity 2022.3 LTS"
5. Choose modules:
   - **Android Build Support** (for Meta Quest)
   - **Windows Build Support** (for PC VR)
   - **Universal Windows Platform Build Support** (for HoloLens)

### 1.3 Configure Unity for VR Development
```csharp
// Unity preferences setup
Edit > Preferences > External Tools
  - External Script Editor: Visual Studio 2022
  - Android SDK: [Path to Android SDK]
  - Android NDK: [Path to Android NDK]
  - JDK: [Path to JDK 11+]
```

## Step 2: Project Setup

### 2.1 Clone Repository
```bash
# Clone the project
git clone https://github.com/aydincollab/HeartLab-VR-AI.git
cd HeartLab-VR-AI

# Open in Unity Hub
# Add project folder to Unity Hub and open with Unity 2022.3 LTS
```

### 2.2 Package Installation
Unity should automatically install required packages via the package manifest. If not:

```csharp
// Window > Package Manager
// Install these packages:
- XR Interaction Toolkit (2.5.2+)
- XR Management (4.4.0+)
- Oculus XR Plugin (4.1.2+)
- OpenXR Plugin (1.8.2+)
- Input System (1.7.0+)
- TextMeshPro (3.0.6+)
- Universal Render Pipeline (14.0.8+)
- Newtonsoft Json (3.2.1+)
```

### 2.3 Import Required Assets
```bash
# Download additional assets (if needed)
# Place 3D heart models in Assets/Models/
# Place audio files in Assets/Resources/Audio/
# Place textures in Assets/Textures/
```

## Step 3: VR Configuration

### 3.1 XR Management Setup
1. **Edit > Project Settings > XR Plug-in Management**
2. **Enable "Initialize XR on Startup"**
3. **Select target platforms:**
   - **Android**: Enable Oculus
   - **PC**: Enable OpenXR and/or Oculus

### 3.2 XR Interaction Toolkit Setup
1. **Window > XR > XR Interaction Toolkit > Samples**
2. **Import "Starter Assets"**
3. **Import "XR Device Simulator"** (for testing without VR)

### 3.3 Input System Configuration
1. **Edit > Project Settings > Player > Configuration**
2. **Set "Active Input Handling" to "Input System Package (New)"**
3. **Restart Unity when prompted**

### 3.4 Meta Quest Specific Setup
```csharp
// Edit > Project Settings > Player > Android
Target API Level: API Level 29 (Android 10.0)
Minimum API Level: API Level 29 (Android 10.0)
Scripting Backend: IL2CPP
Target Architectures: ARM64
```

## Step 4: Cloud Services Setup (Optional)

### 4.1 Azure Speech Services
1. **Create Azure Account**: https://azure.microsoft.com/
2. **Create Speech Service Resource**
3. **Get API Key and Region**
4. **Set Environment Variables**:
```bash
export AZURE_SPEECH_KEY="your_speech_key_here"
export AZURE_SPEECH_REGION="westeurope"
```

### 4.2 Hugging Face API
1. **Create Account**: https://huggingface.co/
2. **Generate API Token**: Settings > Access Tokens
3. **Set Environment Variable**:
```bash
export HUGGINGFACE_API_KEY="your_api_key_here"
```

### 4.3 Configure in Unity
```csharp
// Add API keys to components or configure via script
HuggingFaceAPI.Instance.SetAPIKey("your_key");
VoiceRecognition.Instance.SetAzureConfig("your_key", "your_region");
TextToSpeech.Instance.SetAzureConfig("your_key", "your_region");
```

## Step 5: Build Configuration

### 5.1 Android/Meta Quest Build
```csharp
// File > Build Settings
Platform: Android
  
// Player Settings
Product Name: HeartLab VR AI
Company Name: Your Company
Bundle Identifier: com.yourcompany.heartlabvr

// XR Settings
Virtual Reality Supported: Checked
Virtual Reality SDKs: Oculus

// Graphics Settings
Graphics APIs: OpenGLES3, Vulkan
Multithreaded Rendering: Checked
Static Batching: Checked
Dynamic Batching: Checked
```

### 5.2 PC VR Build
```csharp
// File > Build Settings
Platform: Windows

// Player Settings
Product Name: HeartLab VR AI
Company Name: Your Company

// XR Settings
Virtual Reality Supported: Checked
Virtual Reality SDKs: OpenXR, Windows Mixed Reality

// Graphics Settings
Graphics APIs: Direct3D11, Direct3D12
Multithreaded Rendering: Checked
```

## Step 6: Scene Setup

### 6.1 Create Main Scene
1. **Create new scene**: File > New Scene
2. **Add XR Rig**: GameObject > XR > XR Origin (VR)
3. **Configure XR Rig**:
   - Set tracking origin mode to "Floor"
   - Enable hand tracking (if supported)
   - Configure locomotion system

### 6.2 Add Heart Model
```csharp
// In scene hierarchy
1. Create empty GameObject "HeartModel"
2. Add HeartPartManager component
3. Add child objects for each heart part
4. Add HeartPartDetector to each part
5. Configure materials and colliders
```

### 6.3 Add AI System
```csharp
// Create GameObject "AIManager"
1. Add HuggingFaceAPI component
2. Add MedicalPromptManager component
3. Add TurkishMedicalTerms component
4. Add ResponseCache component
```

### 6.4 Add VR UI
```csharp
// Create Canvas with VRUIManager
1. Create Canvas GameObject
2. Set Render Mode to "World Space"
3. Add VRUIManager component
4. Add EducationalPanel prefab as child
```

## Step 7: Testing

### 7.1 Editor Testing
1. **Window > XR > XR Device Simulator**
2. **Enable simulator in XR Management**
3. **Play scene and test with mouse/keyboard**

### 7.2 VR Hardware Testing
```bash
# Meta Quest via USB
1. Enable Developer Mode on Quest
2. Connect via USB cable
3. Build and Run to device

# PC VR via SteamVR
1. Start SteamVR
2. Connect VR headset
3. Play in editor or build standalone
```

### 7.3 Functionality Testing
- **Gaze Tracking**: Look at heart parts to highlight them
- **Voice Commands**: Speak medical questions in Turkish
- **Hand Tracking**: Point at parts with VR controllers or hands
- **AI Responses**: Verify API connections and responses
- **Audio Output**: Test TTS with Turkish medical terms

## Step 8: Performance Optimization

### 8.1 Unity Performance Settings
```csharp
// Edit > Project Settings > Quality
- Shadow Quality: Medium
- Texture Quality: Medium  
- Anti-Aliasing: 4x Multi Sampling
- VSync Count: Don't Sync (for VR)
```

### 8.2 VR-Specific Optimizations
```csharp
// Target frame rate for VR
Application.targetFrameRate = 90; // Meta Quest
Application.targetFrameRate = 120; // High-end PC VR

// Render pipeline optimizations
- Use Forward Rendering
- Enable GPU Instancing
- Optimize shader variants
- Use texture streaming
```

### 8.3 Memory Optimization
```csharp
// Configure garbage collection
- Use object pooling for UI elements
- Cache frequently used assets
- Optimize texture sizes for VR
- Use compression for audio files
```

## Step 9: Deployment

### 9.1 Meta Quest Deployment
```bash
# Via Unity
1. File > Build and Run
2. Select connected Quest device
3. Install and launch automatically

# Via ADB (manual)
adb install HeartLabVRAI.apk
adb shell am start -n com.yourcompany.heartlabvr/.UnityPlayerActivity
```

### 9.2 PC VR Deployment
```bash
# Standalone executable
1. Build executable
2. Include VR runtime redistributables
3. Create installer with dependencies

# Steam VR distribution
1. Create Steam app entry
2. Upload build to Steam
3. Configure VR compatibility tags
```

## Step 10: Troubleshooting

### Common Issues

#### VR Not Working
```yaml
Problem: VR headset not detected
Solution: 
  - Check XR Management settings
  - Verify VR runtime installation
  - Update graphics drivers
  - Check USB connections
```

#### Performance Issues
```yaml
Problem: Low frame rate in VR
Solution:
  - Reduce texture quality
  - Optimize polygon count
  - Enable GPU instancing
  - Use LOD systems
```

#### Audio Not Working
```yaml
Problem: Voice recognition not responding
Solution:
  - Check microphone permissions
  - Verify Azure API configuration
  - Test with fallback responses
  - Check network connectivity
```

#### Build Errors
```yaml
Problem: Android build fails
Solution:
  - Update Android SDK/NDK
  - Check Java version (JDK 11+)
  - Clear Unity cache
  - Restart Unity and rebuild
```

## Support Resources

### Documentation
- **Unity XR Documentation**: https://docs.unity3d.com/Manual/XR.html
- **Meta Quest Development**: https://developer.oculus.com/
- **Azure Speech Services**: https://docs.microsoft.com/azure/cognitive-services/speech-service/
- **Hugging Face API**: https://huggingface.co/docs/api-inference/

### Community
- **Unity Forums**: https://forum.unity.com/
- **VR Development Discord**: [Various VR dev communities]
- **Meta Developer Forums**: https://forums.oculusvr.com/
- **Project GitHub Issues**: https://github.com/aydincollab/HeartLab-VR-AI/issues

### Professional Support
- **Technical Support**: support@heartlabvr.com
- **Setup Assistance**: setup@heartlabvr.com
- **Training Services**: training@heartlabvr.com

---

**Next Steps**: Once setup is complete, proceed to the [API Documentation](API_DOCUMENTATION.md) for detailed component information.