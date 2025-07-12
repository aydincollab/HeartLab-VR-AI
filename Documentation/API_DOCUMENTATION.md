# HeartLab VR AI - API Documentation

Complete API reference for all HeartLab VR AI components and systems.

## Table of Contents
- [AI Integration](#ai-integration)
- [VR Interaction](#vr-interaction)
- [Heart Model System](#heart-model-system)
- [UI Management](#ui-management)
- [Data Structures](#data-structures)
- [Events and Callbacks](#events-and-callbacks)

## AI Integration

### HuggingFaceAPI

Main class for interacting with Hugging Face medical AI models.

#### Properties
```csharp
public static HuggingFaceAPI Instance { get; private set; }
public bool IsConfigured { get; }
```

#### Events
```csharp
public event Action<string> OnResponseReceived;
public event Action<string> OnErrorOccurred;
```

#### Methods
```csharp
// Send medical query with anatomical context
public void SendMedicalQuery(string query, string anatomicalContext = "")

// Configure API credentials
public void SetAPIKey(string newApiKey)

// Check if API is properly configured
public bool IsConfigured()
```

#### Usage Example
```csharp
var aiAPI = HuggingFaceAPI.Instance;

// Configure API
aiAPI.SetAPIKey("your_api_key");

// Send query
aiAPI.SendMedicalQuery("Sol ventrikül ne işe yarar?", "sol_ventrikul");

// Handle response
aiAPI.OnResponseReceived += (response) => {
    Debug.Log($"AI Response: {response}");
    textToSpeech.Speak(response);
};
```

### MedicalPromptManager

Manages medical prompts and context for AI interactions.

#### Methods
```csharp
// Create contextualized medical prompt
public string CreateMedicalPrompt(string userQuery, string anatomicalPart = "")

// Get offline fallback response
public string GetOfflineFallback(string query, string anatomicalPart = "")

// Get educational content for anatomical part
public string GetAnatomicalEducation(string anatomicalPart)

// Add anatomical context information
public void AddAnatomicalContext(string partName, string description)

// Get available anatomical parts
public List<string> GetAvailableAnatomicalParts()
```

#### Usage Example
```csharp
var promptManager = GetComponent<MedicalPromptManager>();

// Create enhanced prompt
string enhancedPrompt = promptManager.CreateMedicalPrompt(
    "Kalp nasıl çalışır?", 
    "sol_ventrikul"
);

// Get educational content
string education = promptManager.GetAnatomicalEducation("sol_ventrikul");
```

### TurkishMedicalTerms

Manages Turkish medical terminology and translations.

#### Nested Classes
```csharp
[System.Serializable]
public class MedicalTerm
{
    public string turkish;
    public string english;
    public string definition;
    public string category;
    public string pronunciation;
}
```

#### Methods
```csharp
// Get relevant terms for query
public string GetRelevantTerms(string query)

// Translate medical term
public string TranslateTerm(string term)

// Get term definition
public string GetDefinition(string term)

// Get pronunciation guide
public string GetPronunciation(string term)

// Get terms by category
public List<string> GetTermsByCategory(string category)

// Search terms by keyword
public List<MedicalTerm> SearchTerms(string keyword)
```

#### Usage Example
```csharp
var medicalTerms = GetComponent<TurkishMedicalTerms>();

// Get relevant terms
string relevantTerms = medicalTerms.GetRelevantTerms("ventrikül");

// Translate term
string translation = medicalTerms.TranslateTerm("sol ventrikül");

// Get definition
string definition = medicalTerms.GetDefinition("mitral kapak");
```

### ResponseCache

Manages AI response caching for improved performance.

#### Nested Classes
```csharp
[System.Serializable]
public class CachedResponse
{
    public string query;
    public string anatomicalContext;
    public string response;
    public DateTime timestamp;
    public int accessCount;
}
```

#### Properties
```csharp
public int CacheSize { get; }
public int CacheHits { get; private set; }
public int CacheMisses { get; private set; }
```

#### Methods
```csharp
// Get cached response if available
public string GetCachedResponse(string query, string anatomicalContext = "")

// Cache new response
public void CacheResponse(string query, string anatomicalContext, string response)

// Clean expired entries
public void CleanExpiredEntries()

// Clear entire cache
public void ClearCache()

// Get cache statistics
public string GetCacheStats()
```

## VR Interaction

### VRInputManager

Central manager for VR input and interactions.

#### Properties
```csharp
public static VRInputManager Instance { get; private set; }
public bool IsHandTrackingActive { get; private set; }
public bool IsGazeTrackingActive { get; private set; }
public Transform CurrentInteractedObject { get; private set; }
```

#### Events
```csharp
public event Action<Vector3> OnHandPointingStarted;
public event Action OnHandPointingEnded;
public event Action<string> OnVoiceCommandDetected;
public event Action<Transform> OnHandGrabStarted;
public event Action OnHandGrabEnded;
```

#### Methods
```csharp
// Enable/disable input systems
public void SetHandTrackingEnabled(bool enabled)
public void SetGazeInputEnabled(bool enabled)
public void SetVoiceInputEnabled(bool enabled)

// Trigger haptic feedback
public void TriggerHapticFeedback(float intensity = 0.5f, float duration = 0.1f)

// Get input state information
public string GetInputState()
```

### GazeTracker

Handles gaze tracking for anatomical part selection.

#### Properties
```csharp
public Transform CurrentGazeTarget { get; private set; }
public Vector3 CurrentGazePoint { get; private set; }
public bool IsGazing { get; }
```

#### Events
```csharp
public event Action<Transform, Vector3> OnGazeHit;
public event Action OnGazeLost;
public event Action<Transform> OnGazeSelect;
```

#### Methods
```csharp
// Force gaze selection
public void ForceGazeSelect()

// Get current gaze ray
public Ray GetGazeRay()

// Get dwell progress (0-1)
public float GetDwellProgress()

// Configure gaze settings
public void SetGazeDistance(float distance)
public void SetDwellTime(float time)
public void SetSmoothTracking(bool enabled)
public void SetSmoothingFactor(float factor)

// Reset gaze state
public void ResetGazeState()
```

### VoiceRecognition

Handles voice recognition using Azure Speech Services.

#### Properties
```csharp
public bool IsListening { get; private set; }
public bool IsConfigured { get; }
public float LastConfidenceScore { get; private set; }
```

#### Events
```csharp
public event Action<string> OnSpeechRecognized;
public event Action<string> OnSpeechRecognitionError;
public event Action OnListeningStarted;
public event Action OnListeningStopped;
```

#### Methods
```csharp
// Control listening
public void StartListening()
public void StopListening()

// Configure Azure Speech
public void SetAzureConfig(string key, string region)
public void SetRecognitionLanguage(string language)

// Get status information
public string GetRecognitionStatus()
```

### TextToSpeech

Handles text-to-speech synthesis for AI responses.

#### Properties
```csharp
public bool IsSpeaking { get; private set; }
public bool IsConfigured { get; }
```

#### Events
```csharp
public event Action OnSpeechStarted;
public event Action OnSpeechCompleted;
public event Action<string> OnSpeechError;
```

#### Methods
```csharp
// Speech synthesis
public void Speak(string text)
public void StopSpeaking()

// Configuration
public void SetAzureConfig(string key, string region)
public void SetVoice(string voice)
public void SetSpeechParameters(float rate, float pitch, float volume)

// Cache management
public void ClearCache()

// Testing
public void TestTTS()

// Status information
public string GetTTSStatus()
```

## Heart Model System

### HeartPartManager

Manages heart parts and their interactions.

#### Properties
```csharp
public static HeartPartManager Instance { get; private set; }
public HeartPart CurrentSelectedPart { get; private set; }
public List<HeartPart> HeartParts { get; private set; }
```

#### Events
```csharp
public event Action<HeartPart> OnPartSelected;
public event Action<HeartPart> OnPartHighlighted;
public event Action OnPartDeselected;
```

#### Methods
```csharp
// Part selection and highlighting
public void SelectPart(HeartPart heartPart)
public void HighlightPart(HeartPart heartPart)
public void DeselectCurrentPart()

// Part queries
public HeartPart GetPartByName(string partName)
public List<HeartPart> GetPartsByType(HeartPartType partType)

// Batch operations
public void HighlightPartsByType(HeartPartType partType)
public void ResetAllParts()

// Animation control
public void SetHeartRate(float bpm)
public void SetPulseAnimationEnabled(bool enabled)

// Statistics
public string GetHeartModelStats()
```

### HeartPart

Represents a heart part with its properties and state.

#### Properties
```csharp
public string PartName { get; private set; }
public string DisplayName { get; private set; }
public HeartPartType PartType { get; private set; }
public Transform Transform { get; private set; }
public Renderer Renderer { get; private set; }
public HeartPartDetector Detector { get; private set; }
public bool IsSelected { get; private set; }
public bool IsHighlighted { get; private set; }
```

#### Methods
```csharp
// State management
public void SetSelected(bool selected)
public void SetHighlighted(bool highlighted)
```

### HeartPartType

Enumeration for categorizing heart parts.

```csharp
public enum HeartPartType
{
    Chamber,    // Ventricles and Atria
    Valve,      // Heart valves
    Vessel,     // Arteries and veins
    Muscle,     // Heart muscle tissue
    Other       // Other anatomical structures
}
```

### HeartPartDetector

Detects interactions with individual heart parts.

#### Properties
```csharp
public string PartName { get; }
public string DisplayName { get; }
public HeartPartType PartType { get; }
public bool IsInteractable { get; }
public bool IsCurrentlySelected { get; private set; }
public bool IsCurrentlyHighlighted { get; private set; }
```

#### Events
```csharp
public event Action OnPartSelected;
public event Action OnPartHighlighted;
public event Action OnPartDeselected;
```

#### Methods
```csharp
// Interaction events
public void OnGazeEnter()
public void OnGazeExit()
public void OnClick()
public void OnHandPointing()
public void OnHandStopPointing()

// Visual state management
public void HighlightPart()
public void UnhighlightPart()
public void SelectPart()
public void DeselectPart()
public void ResetPart()

// Configuration
public void SetInteractable(bool interactable)

// Information
public string GetPartInfo()
```

### AnatomicalMapping

Maps anatomical structures and their relationships.

#### Nested Classes
```csharp
[System.Serializable]
public class AnatomicalStructure
{
    public string name;
    public string displayName;
    public string turkishName;
    public HeartPartType type;
    public string description;
    public string function;
    public Vector3 position;
    public List<string> connectedParts;
    public Dictionary<string, string> properties;
}
```

#### Properties
```csharp
public int TotalStructures { get; }
```

#### Methods
```csharp
// Structure queries
public AnatomicalStructure GetStructure(string structureName)
public List<string> GetStructuresByType(HeartPartType type)
public List<string> GetConnectedStructures(string structureName)

// Information retrieval
public string GetDetailedDescription(string structureName, string language = "tr")
public List<string> GetBloodFlowPath(string from, string to)

// Validation
public bool StructureExists(string structureName)
public List<string> GetAllStructureNames()

// Statistics
public string GetMappingStats()
```

## UI Management

### VRUIManager

Manages VR user interface elements and interactions.

#### Properties
```csharp
public static VRUIManager Instance { get; private set; }
```

#### Events
```csharp
public event Action OnUIShown;
public event Action OnUIHidden;
```

#### Methods
```csharp
// Label management
public void ShowPartLabel(string partName, Vector3 worldPosition)
public void HidePartLabel()

// Educational content
public void ShowEducationalContent(string partName, string content)
public void HideEducationalContent()

// UI visibility
public void ShowUI()
public void HideUI()
public void ToggleUI()

// Configuration
public void SetUIDistance(float distance)
public void SetFollowUser(bool follow)

// Notifications
public void ShowNotification(string message, float duration = 3f)

// Status
public string GetUIStatus()
```

### EducationalPanel

Manages educational content display in VR.

#### Properties
```csharp
public bool IsVisible { get; private set; }
public bool IsPlayingAudio { get; private set; }
public string CurrentContent { get; private set; }
```

#### Events
```csharp
public event Action<string> OnContentRequested;
public event Action OnPanelClosed;
public event Action<string> OnAudioRequested;
```

#### Methods
```csharp
// Content management
public void ShowContent(string partName, string content)
public void HideContent()
public void UpdateContent(string newContent)

// Audio control
public void ToggleAudio()
public void PlayAudio()
public void StopAudio()

// Configuration
public void SetTypingSpeed(float speed)
public void SetTypingEffectEnabled(bool enabled)
public void SetMaxContentLength(int length)

// Status
public string GetPanelStatus()
```

## Data Structures

### Common Data Types

#### Medical Query Result
```csharp
public class MedicalQueryResult
{
    public string Query { get; set; }
    public string Response { get; set; }
    public float ConfidenceScore { get; set; }
    public string AnatomicalContext { get; set; }
    public DateTime Timestamp { get; set; }
    public bool FromCache { get; set; }
}
```

#### VR Interaction Data
```csharp
public class VRInteractionData
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Transform TargetObject { get; set; }
    public float InteractionTime { get; set; }
    public InteractionType Type { get; set; }
}

public enum InteractionType
{
    Gaze,
    HandPointing,
    HandGrab,
    VoiceCommand,
    Touch
}
```

#### Audio Configuration
```csharp
public class AudioConfiguration
{
    public string Language { get; set; } = "tr-TR";
    public string VoiceName { get; set; } = "tr-TR-EmelNeural";
    public float SpeechRate { get; set; } = 1.0f;
    public float SpeechPitch { get; set; } = 1.0f;
    public float SpeechVolume { get; set; } = 0.8f;
    public bool EnableCaching { get; set; } = true;
}
```

## Events and Callbacks

### Global Event System

#### Medical AI Events
```csharp
// AI query processing
public static event Action<string> OnMedicalQueryStarted;
public static event Action<string, float> OnMedicalQueryCompleted;
public static event Action<string> OnMedicalQueryFailed;

// Cache events
public static event Action<string> OnResponseCached;
public static event Action<string> OnCacheHit;
public static event Action OnCacheCleared;
```

#### VR Interaction Events
```csharp
// Gaze tracking
public static event Action<Transform> OnGazeTargetChanged;
public static event Action<float> OnGazeDwellProgress;

// Hand tracking
public static event Action<bool> OnHandTrackingStateChanged;
public static event Action<Vector3> OnHandPositionUpdated;

// Voice recognition
public static event Action<string, float> OnSpeechRecognized;
public static event Action OnListeningStateChanged;
```

#### Heart Model Events
```csharp
// Part interactions
public static event Action<string> OnHeartPartSelected;
public static event Action<string> OnHeartPartHighlighted;
public static event Action OnHeartPartDeselected;

// Animation events
public static event Action<float> OnHeartRateChanged;
public static event Action<bool> OnPulseAnimationToggled;
```

#### UI Events
```csharp
// Panel events
public static event Action<string> OnEducationalPanelOpened;
public static event Action OnEducationalPanelClosed;
public static event Action<string> OnContentDisplayed;

// Audio events
public static event Action<string> OnTextToSpeechStarted;
public static event Action OnTextToSpeechCompleted;
public static event Action<string> OnAudioError;
```

### Event Usage Examples

#### Subscribing to Events
```csharp
public class EducationController : MonoBehaviour
{
    private void OnEnable()
    {
        HeartPartManager.Instance.OnPartSelected += HandlePartSelection;
        HuggingFaceAPI.Instance.OnResponseReceived += HandleAIResponse;
        VRInputManager.Instance.OnVoiceCommandDetected += HandleVoiceCommand;
    }

    private void OnDisable()
    {
        if (HeartPartManager.Instance != null)
            HeartPartManager.Instance.OnPartSelected -= HandlePartSelection;
        if (HuggingFaceAPI.Instance != null)
            HuggingFaceAPI.Instance.OnResponseReceived -= HandleAIResponse;
        if (VRInputManager.Instance != null)
            VRInputManager.Instance.OnVoiceCommandDetected -= HandleVoiceCommand;
    }

    private void HandlePartSelection(HeartPart part)
    {
        string educationalContent = GetEducationalContent(part.PartName);
        VRUIManager.Instance.ShowEducationalContent(part.DisplayName, educationalContent);
    }

    private void HandleAIResponse(string response)
    {
        VRUIManager.Instance.ShowEducationalContent("AI Response", response);
        TextToSpeech.Instance.Speak(response);
    }

    private void HandleVoiceCommand(string command)
    {
        HuggingFaceAPI.Instance.SendMedicalQuery(command, GetCurrentAnatomicalContext());
    }
}
```

---

**For more detailed implementation examples, see the individual component files in the Assets/Scripts/ directory.**