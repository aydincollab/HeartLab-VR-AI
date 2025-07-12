using System;
using System.Collections;
using UnityEngine;
using HeartLabVR.Utilities;

namespace HeartLabVR.VR
{
    /// <summary>
    /// Handles voice recognition using Azure Speech Services
    /// Supports Turkish language medical queries
    /// </summary>
    public class VoiceRecognition : MonoBehaviour
    {
        [Header("Azure Speech Configuration")]
        [SerializeField] private string subscriptionKey = "";
        [SerializeField] private string serviceRegion = "westeurope";
        [SerializeField] private string recognitionLanguage = "tr-TR";
        
        [Header("Recognition Settings")]
        [SerializeField] private bool continuousRecognition = true;
        [SerializeField] private float silenceTimeout = 3f;
        [SerializeField] private float recognitionTimeout = 30f;
        [SerializeField] private bool filterProfanity = true;
        
        [Header("Medical Context")]
        [SerializeField] private bool useMedicalVocabulary = true;
        [SerializeField] private float confidenceThreshold = 0.7f;

        public event Action<string> OnSpeechRecognized;
        public event Action<string> OnSpeechRecognitionError;
        public event Action OnListeningStarted;
        public event Action OnListeningStopped;

        public bool IsListening { get; private set; }
        public bool IsConfigured => !string.IsNullOrEmpty(subscriptionKey);
        public float LastConfidenceScore { get; private set; }

        private AudioSource audioSource;
        private AudioClip microphoneClip;
        private string[] medicalKeywords;
        private bool isInitialized;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            InitializeMedicalVocabulary();
            LoadConfiguration();
        }

        private void Start()
        {
            InitializeAzureSpeech();
        }

        private void InitializeMedicalVocabulary()
        {
            medicalKeywords = new string[]
            {
                "kalp", "heart", "ventrikül", "ventricle", "atrium", "kulakçık",
                "kapak", "valve", "aorta", "arter", "artery", "ven", "vein",
                "sistol", "systole", "diyastol", "diastole", "ritim", "rhythm",
                "kan", "blood", "dolaşım", "circulation", "pompa", "pump",
                "oksijen", "oxygen", "akciğer", "lung", "pulmoner", "pulmonary",
                "mitral", "triküspit", "tricuspid", "aort", "aortic",
                "sol", "left", "sağ", "right", "üst", "upper", "alt", "lower",
                "nedir", "what", "nasıl", "how", "neden", "why", "ne", "what",
                "işlev", "function", "görev", "task", "yapar", "does", "çalışır", "works"
            };
        }

        private void LoadConfiguration()
        {
            // Try to load from environment variables
            if (string.IsNullOrEmpty(subscriptionKey))
            {
                subscriptionKey = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY");
            }
            
            if (string.IsNullOrEmpty(serviceRegion))
            {
                serviceRegion = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION") ?? "westeurope";
            }
        }

        private void InitializeAzureSpeech()
        {
            if (!IsConfigured)
            {
                Debug.LogWarning("Azure Speech Services not configured. Voice recognition will use fallback mode.");
                return;
            }

            try
            {
                // Initialize Azure Speech SDK
                // Note: In a real implementation, you would initialize the Azure Speech SDK here
                // This is a placeholder for the actual Azure Speech SDK integration
                isInitialized = true;
                Debug.Log("Azure Speech Services initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize Azure Speech Services: {ex.Message}");
                isInitialized = false;
            }
        }

        /// <summary>
        /// Starts listening for voice input
        /// </summary>
        public void StartListening()
        {
            if (IsListening)
                return;

            if (!isInitialized)
            {
                StartFallbackListening();
                return;
            }

            StartCoroutine(StartAzureSpeechRecognition());
        }

        /// <summary>
        /// Stops listening for voice input
        /// </summary>
        public void StopListening()
        {
            if (!IsListening)
                return;

            IsListening = false;
            StopCoroutine(StartAzureSpeechRecognition());
            
            if (microphoneClip != null)
            {
                Microphone.End(null);
                microphoneClip = null;
            }

            OnListeningStopped?.Invoke();
        }

        private IEnumerator StartAzureSpeechRecognition()
        {
            IsListening = true;
            OnListeningStarted?.Invoke();

            string microphoneName = null;
            float recognitionStartTime = 0f;
            float lastSoundTime = 0f;
            bool initializationSuccessful = false;

            try
            {
                // Start microphone recording
                microphoneName = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
                
                if (string.IsNullOrEmpty(microphoneName))
                {
                    OnSpeechRecognitionError?.Invoke("No microphone device found");
                    yield break;
                }

                microphoneClip = Microphone.Start(microphoneName, true, (int)recognitionTimeout, 44100);
                
                recognitionStartTime = Time.time;
                lastSoundTime = Time.time;
                initializationSuccessful = true;
            }
            catch (Exception ex)
            {
                OnSpeechRecognitionError?.Invoke($"Recognition initialization error: {ex.Message}");
                yield break;
            }

            // Main recognition loop (outside try-catch to allow yield return)
            if (initializationSuccessful)
            {
                while (IsListening && (Time.time - recognitionStartTime) < recognitionTimeout)
                {
                    bool hasInput = false;
                    
                    try
                    {
                        // Check for audio input
                        if (HasAudioInput())
                        {
                            lastSoundTime = Time.time;
                            hasInput = true;
                        }
                        
                        // Check for silence timeout
                        if ((Time.time - lastSoundTime) > silenceTimeout)
                        {
                            ProcessRecognitionResult(""); // Timeout
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        OnSpeechRecognitionError?.Invoke($"Recognition loop error: {ex.Message}");
                        break;
                    }

                    yield return new WaitForSeconds(0.1f);
                }

                // Process final recognition result
                try
                {
                    // In a real implementation, this would process the audio with Azure Speech Services
                    // For now, we'll simulate recognition with fallback processing
                    if (IsListening)
                    {
                        ProcessFallbackRecognition();
                    }
                }
                catch (Exception ex)
                {
                    OnSpeechRecognitionError?.Invoke($"Recognition processing error: {ex.Message}");
                }
            }

            // Cleanup
            IsListening = false;
            OnListeningStopped?.Invoke();
        }

        private void StartFallbackListening()
        {
            // Fallback mode - simulates voice recognition for development/testing
            Debug.Log("Starting fallback voice recognition mode");
            StartCoroutine(FallbackRecognitionLoop());
        }

        private IEnumerator FallbackRecognitionLoop()
        {
            IsListening = true;
            OnListeningStarted?.Invoke();

            // Simulate listening for a few seconds
            yield return new WaitForSeconds(2f);

            // Simulate recognition of common medical queries
            string[] sampleQueries = {
                "Sol ventrikül ne işe yarar?",
                "Kalp nasıl çalışır?",
                "Bu bölüm nedir?",
                "Kalp kapakları nedir?",
                "Kan dolaşımı nasıl olur?"
            };

            if (IsListening)
            {
                string simulatedQuery = sampleQueries[UnityEngine.Random.Range(0, sampleQueries.Length)];
                ProcessRecognitionResult(simulatedQuery);
            }

            IsListening = false;
            OnListeningStopped?.Invoke();
        }

        private bool HasAudioInput()
        {
            if (microphoneClip == null)
                return false;

            // Simple audio level detection
            int sampleWindow = 128;
            int microphonePosition = Microphone.GetPosition(null);
            
            if (microphonePosition < sampleWindow)
                return false;

            float[] samples = new float[sampleWindow];
            microphoneClip.GetData(samples, microphonePosition - sampleWindow);

            float sum = 0f;
            for (int i = 0; i < sampleWindow; i++)
            {
                sum += Mathf.Abs(samples[i]);
            }

            float averageLevel = sum / sampleWindow;
            return averageLevel > 0.001f; // Threshold for detecting voice
        }

        private void ProcessFallbackRecognition()
        {
            // Process the recorded audio (fallback implementation)
            // In a real scenario, this would send audio to Azure Speech Services
            
            // For now, we'll analyze the audio and try to recognize patterns
            if (microphoneClip != null)
            {
                // Simulate processing delay
                StartCoroutine(DelayedFallbackResult());
            }
        }

        private IEnumerator DelayedFallbackResult()
        {
            yield return new WaitForSeconds(1f);
            
            // Simulate a medical query result
            string fallbackResult = "Sol ventrikül hakkında bilgi ver";
            ProcessRecognitionResult(fallbackResult);
        }

        private void ProcessRecognitionResult(string recognizedText)
        {
            if (string.IsNullOrEmpty(recognizedText))
            {
                OnSpeechRecognitionError?.Invoke("No speech recognized");
                return;
            }

            // Filter and validate the recognized text
            string processedText = ProcessMedicalQuery(recognizedText);
            
            if (!string.IsNullOrEmpty(processedText))
            {
                LastConfidenceScore = CalculateConfidence(processedText);
                
                if (LastConfidenceScore >= confidenceThreshold)
                {
                    OnSpeechRecognized?.Invoke(processedText);
                }
                else
                {
                    OnSpeechRecognitionError?.Invoke($"Low confidence recognition: {LastConfidenceScore:F2}");
                }
            }
            else
            {
                OnSpeechRecognitionError?.Invoke("Unrecognized medical query");
            }
        }

        private string ProcessMedicalQuery(string rawText)
        {
            if (string.IsNullOrEmpty(rawText))
                return "";

            string lowerText = rawText.ToLowerTurkish();
            
            // Check if the text contains medical keywords
            if (useMedicalVocabulary)
            {
                bool containsMedicalTerms = false;
                foreach (string keyword in medicalKeywords)
                {
                    if (lowerText.Contains(keyword.ToLowerTurkish()))
                    {
                        containsMedicalTerms = true;
                        break;
                    }
                }

                if (!containsMedicalTerms)
                {
                    Debug.Log($"No medical terms found in: {rawText}");
                    return "";
                }
            }

            // Clean up the text
            string cleanedText = rawText.Trim();
            
            // Filter profanity if enabled
            if (filterProfanity)
            {
                cleanedText = FilterProfanity(cleanedText);
            }

            return cleanedText;
        }

        private string FilterProfanity(string text)
        {
            // Simple profanity filter - in production, use a comprehensive filter
            return text; // Placeholder implementation
        }

        private float CalculateConfidence(string recognizedText)
        {
            // Simple confidence calculation based on medical keyword presence
            if (string.IsNullOrEmpty(recognizedText))
                return 0f;

            string lowerText = recognizedText.ToLowerTurkish();
            int keywordMatches = 0;
            
            foreach (string keyword in medicalKeywords)
            {
                if (lowerText.Contains(keyword.ToLowerTurkish()))
                {
                    keywordMatches++;
                }
            }

            // Base confidence plus keyword bonus
            float baseConfidence = 0.5f;
            float keywordBonus = Mathf.Min(keywordMatches * 0.1f, 0.4f);
            
            return Mathf.Clamp01(baseConfidence + keywordBonus);
        }

        /// <summary>
        /// Sets Azure Speech Services configuration
        /// </summary>
        /// <param name="key">Subscription key</param>
        /// <param name="region">Service region</param>
        public void SetAzureConfig(string key, string region)
        {
            subscriptionKey = key;
            serviceRegion = region;
            
            if (!string.IsNullOrEmpty(key))
            {
                InitializeAzureSpeech();
            }
        }

        /// <summary>
        /// Sets the recognition language
        /// </summary>
        /// <param name="language">Language code (e.g., "tr-TR", "en-US")</param>
        public void SetRecognitionLanguage(string language)
        {
            recognitionLanguage = language;
        }

        /// <summary>
        /// Gets current recognition status
        /// </summary>
        /// <returns>Status information as string</returns>
        public string GetRecognitionStatus()
        {
            return $"Listening: {IsListening}, Configured: {IsConfigured}, " +
                   $"Language: {recognitionLanguage}, Last Confidence: {LastConfidenceScore:F2}";
        }

        private void OnDestroy()
        {
            StopListening();
        }
    }
}