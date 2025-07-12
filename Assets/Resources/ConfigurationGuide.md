# HeartLab VR AI Configuration

## Environment Variables

Set these environment variables for proper configuration:

### Azure Speech Services
```bash
export AZURE_SPEECH_KEY="your_azure_speech_key_here"
export AZURE_SPEECH_REGION="westeurope"
```

### Hugging Face API
```bash
export HUGGINGFACE_API_KEY="your_huggingface_api_key_here"
```

## Unity Configuration

### Package Dependencies
The following packages are required and should be automatically installed via the package manifest:

- **XR Interaction Toolkit**: com.unity.xr.interaction.toolkit (2.5.2)
- **XR Management**: com.unity.xr.management (4.4.0)
- **Oculus XR Plugin**: com.unity.xr.oculus (4.1.2)
- **OpenXR Plugin**: com.unity.xr.openxr (1.8.2)
- **Input System**: com.unity.inputsystem (1.7.0)
- **TextMeshPro**: com.unity.textmeshpro (3.0.6)
- **Universal Render Pipeline**: com.unity.render-pipelines.universal (14.0.8)
- **Newtonsoft JSON**: com.unity.nuget.newtonsoft-json (3.2.1)

### Build Settings

#### Meta Quest / Android Settings
```yaml
Target Platform: Android
Architecture: ARM64
Scripting Backend: IL2CPP
Api Compatibility Level: .NET Standard 2.1
Target API Level: Android API level 29 (Android 10.0)
Minimum API Level: Android API level 29 (Android 10.0)
```

#### VR Settings
```yaml
VR SDKs Enabled:
  - Oculus
  - OpenXR
Virtual Reality Supported: true
Stereo Rendering Mode: Single Pass Instanced
```

#### Performance Settings
```yaml
Graphics Jobs: Enabled
Multithreaded Rendering: Enabled
Static Batching: Enabled
Dynamic Batching: Enabled
GPU Skinning: Enabled
```

## Script Configuration

### HuggingFaceAPI.cs
```csharp
// Configure in inspector or via code
apiKey = "your_api_key"; // Or set via environment variable
baseUrl = "https://api-inference.huggingface.co/models/";
medicalModel = "microsoft/BiomedNLP-PubMedBERT-base-uncased-abstract-fulltext";
requestTimeout = 10f;
```

### VoiceRecognition.cs
```csharp
// Configure Azure Speech Services
subscriptionKey = "your_subscription_key"; // Or set via environment variable
serviceRegion = "westeurope";
recognitionLanguage = "tr-TR";
```

### TextToSpeech.cs
```csharp
// Configure Azure TTS
subscriptionKey = "your_subscription_key"; // Or set via environment variable
serviceRegion = "westeurope";
voiceName = "tr-TR-EmelNeural";
```

## Asset Configuration

### Materials
Create materials for heart part highlighting:

1. **DefaultMaterial**: Base material for heart parts
2. **HighlightMaterial**: Yellow tinted material for hover state
3. **SelectionMaterial**: Green tinted material with emission for selected state

### Audio
Place audio files in `Assets/Resources/Audio/`:
- Heartbeat sounds
- UI feedback sounds
- Notification sounds

### Prefabs
Required prefab structure:

```
Assets/Prefabs/
├── HeartModel/
│   ├── CompleteHeart.prefab
│   ├── LeftVentricle.prefab
│   ├── RightVentricle.prefab
│   ├── LeftAtrium.prefab
│   ├── RightAtrium.prefab
│   └── HeartValves.prefab
├── VR/
│   ├── VRRig.prefab
│   ├── HandControllers.prefab
│   └── GazePointer.prefab
└── UI/
    ├── EducationalPanel.prefab
    ├── PartLabel.prefab
    └── VRCanvas.prefab
```

## Performance Optimization

### VR Optimization
```yaml
Target Frame Rate: 90 FPS
Texture Quality: Medium (for mobile VR)
Shadow Quality: Soft Shadows
Anti-Aliasing: 4x MSAA
VR Eye Texture Resolution: 2048x2048
```

### Memory Management
```yaml
Texture Streaming: Enabled
Mesh Compression: Enabled
Audio Compression: Vorbis
Maximum Cache Size: 2 GB
```

### CPU Optimization
```yaml
Physics Update Rate: 50 Hz
Fixed Timestep: 0.02 seconds
Maximum Allowed Timestep: 0.1 seconds
Script Call Optimization: Fast but no Exceptions
```

## Network Configuration

### API Endpoints
```yaml
Hugging Face API: https://api-inference.huggingface.co/models/
Azure Speech API: https://{region}.api.cognitive.microsoft.com/
```

### Timeout Settings
```yaml
API Request Timeout: 10 seconds
Speech Recognition Timeout: 30 seconds
TTS Generation Timeout: 15 seconds
```

## Development Tools

### Debugging
Enable these for development:
```csharp
Debug.Log() calls in development builds
VR performance overlay
Network request logging
AI response caching
```

### Testing
Test configurations:
```yaml
Test Mode: Enable fallback responses
Simulation Mode: Enable voice recognition simulation
Performance Mode: Enable FPS counter and memory usage
```

## Deployment

### Meta Quest Store
1. Set up Oculus developer account
2. Configure app metadata
3. Enable Oculus entitlement checking
4. Test on Quest device
5. Submit for review

### Standalone VR
1. Configure OpenXR providers
2. Test with SteamVR
3. Optimize for PC VR headsets
4. Package with required redistributables

## Security

### API Key Management
- Never commit API keys to version control
- Use environment variables in production
- Implement key rotation procedures
- Monitor API usage and costs

### Data Privacy
- Implement GDPR compliance for EU users
- Secure storage for user preferences
- Optional telemetry with user consent
- Clear data deletion procedures

## Troubleshooting

### Common Issues

#### VR Not Working
1. Check XR Management settings
2. Verify VR SDK installation
3. Ensure proper USB connection (Quest)
4. Update graphics drivers

#### AI Not Responding
1. Verify API keys are set
2. Check internet connectivity
3. Monitor API rate limits
4. Test with fallback responses

#### Performance Issues
1. Reduce texture quality
2. Optimize polygon count
3. Enable GPU instancing
4. Use LOD systems

#### Audio Issues
1. Check microphone permissions
2. Verify Azure Speech configuration
3. Test with different audio devices
4. Monitor audio latency

### Support Contacts
- Technical Support: [support@heartlabvr.com]
- API Issues: [api@heartlabvr.com]
- VR Hardware: [hardware@heartlabvr.com]