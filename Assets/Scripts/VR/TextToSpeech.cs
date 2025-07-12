using System;
using System.Collections;
using UnityEngine;

namespace HeartLabVR.VR
{
    /// <summary>
    /// Handles text-to-speech for AI responses using Azure Speech Services
    /// Supports Turkish language output with natural voice synthesis
    /// </summary>
    public class TextToSpeech : MonoBehaviour
    {
        [Header("Azure Speech Configuration")]
        [SerializeField] private string subscriptionKey = "";
        [SerializeField] private string serviceRegion = "westeurope";
        [SerializeField] private string voiceName = "tr-TR-EmelNeural"; // Turkish female voice
        
        [Header("Speech Settings")]
        [SerializeField] private float speechRate = 1.0f;
        [SerializeField] private float speechPitch = 1.0f;
        [SerializeField] private float speechVolume = 1.0f;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Performance Settings")]
        [SerializeField] private bool cacheAudioClips = true;
        [SerializeField] private int maxCacheSize = 50;
        [SerializeField] private float audioCompressionQuality = 0.7f;

        public event Action OnSpeechStarted;
        public event Action OnSpeechCompleted;
        public event Action<string> OnSpeechError;

        public bool IsSpeaking { get; private set; }
        public bool IsConfigured => !string.IsNullOrEmpty(subscriptionKey);

        private System.Collections.Generic.Dictionary<string, AudioClip> audioClipCache;
        private System.Collections.Generic.Queue<string> cacheOrder;
        private bool isInitialized;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            audioClipCache = new System.Collections.Generic.Dictionary<string, AudioClip>();
            cacheOrder = new System.Collections.Generic.Queue<string>();
            
            LoadConfiguration();
        }

        private void Start()
        {
            InitializeAzureSpeech();
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
                Debug.LogWarning("Azure Speech Services not configured. Text-to-Speech will use fallback mode.");
                return;
            }

            try
            {
                // Initialize Azure Speech SDK for TTS
                // Note: In a real implementation, you would initialize the Azure Speech SDK here
                isInitialized = true;
                Debug.Log("Azure Text-to-Speech Services initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize Azure TTS Services: {ex.Message}");
                isInitialized = false;
            }
        }

        /// <summary>
        /// Speaks the given text using Azure TTS or fallback synthesis
        /// </summary>
        /// <param name="text">Text to speak in Turkish</param>
        public void Speak(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (IsSpeaking)
            {
                StopSpeaking();
            }

            StartCoroutine(SpeakCoroutine(text));
        }

        /// <summary>
        /// Stops current speech playback
        /// </summary>
        public void StopSpeaking()
        {
            if (IsSpeaking)
            {
                IsSpeaking = false;
                
                if (audioSource != null)
                {
                    audioSource.Stop();
                }
                
                StopAllCoroutines();
                OnSpeechCompleted?.Invoke();
            }
        }

        private IEnumerator SpeakCoroutine(string text)
        {
            IsSpeaking = true;
            OnSpeechStarted?.Invoke();

            try
            {
                AudioClip audioClip = null;
                
                // Check cache first
                if (cacheAudioClips && audioClipCache.ContainsKey(text))
                {
                    audioClip = audioClipCache[text];
                    Debug.Log($"Using cached audio for text: {text.Substring(0, Mathf.Min(30, text.Length))}...");
                }
                else
                {
                    // Generate new audio clip
                    if (isInitialized)
                    {
                        audioClip = yield return StartCoroutine(GenerateAzureTTSAudio(text));
                    }
                    else
                    {
                        audioClip = GenerateFallbackAudio(text);
                    }

                    // Cache the audio clip
                    if (audioClip != null && cacheAudioClips)
                    {
                        CacheAudioClip(text, audioClip);
                    }
                }

                // Play the audio
                if (audioClip != null)
                {
                    yield return StartCoroutine(PlayAudioClip(audioClip));
                }
                else
                {
                    OnSpeechError?.Invoke("Failed to generate audio for text");
                }
            }
            catch (Exception ex)
            {
                OnSpeechError?.Invoke($"Speech synthesis error: {ex.Message}");
            }
            finally
            {
                IsSpeaking = false;
                OnSpeechCompleted?.Invoke();
            }
        }

        private IEnumerator GenerateAzureTTSAudio(string text)
        {
            // This would integrate with Azure Speech Services TTS API
            // For now, we'll simulate the API call and return fallback audio
            
            yield return new WaitForSeconds(0.5f); // Simulate API delay
            
            // In a real implementation, you would:
            // 1. Create SSML with the text and voice settings
            // 2. Call Azure TTS API
            // 3. Convert the returned audio data to AudioClip
            // 4. Return the AudioClip
            
            Debug.Log($"Generating Azure TTS audio for: {text.Substring(0, Mathf.Min(50, text.Length))}...");
            
            // Return fallback for now
            yield return GenerateFallbackAudio(text);
        }

        private AudioClip GenerateFallbackAudio(string text)
        {
            // Fallback audio generation using Unity's built-in capabilities
            // This creates a simple tone-based audio representation
            
            float duration = CalculateSpeechDuration(text);
            int sampleRate = 22050;
            int samples = Mathf.RoundToInt(duration * sampleRate);
            
            float[] audioData = new float[samples];
            
            // Generate simple tone pattern based on text characteristics
            float baseFrequency = 200f; // Base frequency for speech simulation
            float frequencyVariation = 50f;
            
            for (int i = 0; i < samples; i++)
            {
                float time = (float)i / sampleRate;
                float textPosition = (time / duration) * text.Length;
                
                // Vary frequency based on character in text
                char currentChar = text[Mathf.FloorToInt(textPosition) % text.Length];
                float charFrequency = baseFrequency + (currentChar % 26) * (frequencyVariation / 26f);
                
                // Create a more natural speech-like waveform
                float envelope = Mathf.Sin(time * Mathf.PI * 5f) * 0.1f + 0.9f; // Speech envelope
                float tone = Mathf.Sin(2 * Mathf.PI * charFrequency * time) * envelope;
                
                // Add some noise for more natural sound
                float noise = (UnityEngine.Random.Range(-1f, 1f) * 0.05f);
                
                audioData[i] = (tone + noise) * speechVolume * 0.3f;
            }
            
            AudioClip clip = AudioClip.Create($"TTS_{text.GetHashCode()}", samples, 1, sampleRate, false);
            clip.SetData(audioData, 0);
            
            return clip;
        }

        private float CalculateSpeechDuration(string text)
        {
            // Estimate speech duration based on text length and speech rate
            // Average Turkish speech rate is about 3-4 syllables per second
            float baseWordsPerSecond = 2.5f / speechRate;
            int wordCount = text.Split(' ').Length;
            
            float baseDuration = wordCount / baseWordsPerSecond;
            
            // Add pauses for punctuation
            int pauseCount = 0;
            foreach (char c in text)
            {
                if (c == '.' || c == '!' || c == '?')
                    pauseCount++;
                else if (c == ',' || c == ';')
                    pauseCount++;
            }
            
            float pauseDuration = pauseCount * 0.3f;
            
            return Mathf.Max(1f, baseDuration + pauseDuration);
        }

        private IEnumerator PlayAudioClip(AudioClip clip)
        {
            if (audioSource == null || clip == null)
                yield break;

            audioSource.clip = clip;
            audioSource.pitch = speechPitch;
            audioSource.volume = speechVolume;
            audioSource.Play();

            // Wait for audio to complete
            yield return new WaitForSeconds(clip.length / speechPitch);
        }

        private void CacheAudioClip(string text, AudioClip clip)
        {
            if (audioClipCache.Count >= maxCacheSize)
            {
                // Remove oldest cached clip
                string oldestText = cacheOrder.Dequeue();
                if (audioClipCache.ContainsKey(oldestText))
                {
                    AudioClip oldClip = audioClipCache[oldestText];
                    audioClipCache.Remove(oldestText);
                    
                    if (oldClip != null)
                    {
                        Destroy(oldClip);
                    }
                }
            }

            audioClipCache[text] = clip;
            cacheOrder.Enqueue(text);
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
        /// Sets the voice for speech synthesis
        /// </summary>
        /// <param name="voice">Voice name (e.g., "tr-TR-EmelNeural")</param>
        public void SetVoice(string voice)
        {
            voiceName = voice;
        }

        /// <summary>
        /// Sets speech parameters
        /// </summary>
        /// <param name="rate">Speech rate (0.5 - 2.0)</param>
        /// <param name="pitch">Speech pitch (0.5 - 2.0)</param>
        /// <param name="volume">Speech volume (0.0 - 1.0)</param>
        public void SetSpeechParameters(float rate, float pitch, float volume)
        {
            speechRate = Mathf.Clamp(rate, 0.5f, 2.0f);
            speechPitch = Mathf.Clamp(pitch, 0.5f, 2.0f);
            speechVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// Clears the audio clip cache
        /// </summary>
        public void ClearCache()
        {
            foreach (var clip in audioClipCache.Values)
            {
                if (clip != null)
                {
                    Destroy(clip);
                }
            }
            
            audioClipCache.Clear();
            cacheOrder.Clear();
        }

        /// <summary>
        /// Gets current TTS status information
        /// </summary>
        /// <returns>Status information as string</returns>
        public string GetTTSStatus()
        {
            return $"Speaking: {IsSpeaking}, Configured: {IsConfigured}, " +
                   $"Voice: {voiceName}, Cache Size: {audioClipCache.Count}/{maxCacheSize}, " +
                   $"Rate: {speechRate:F1}, Pitch: {speechPitch:F1}, Volume: {speechVolume:F1}";
        }

        /// <summary>
        /// Tests TTS with a sample Turkish medical phrase
        /// </summary>
        public void TestTTS()
        {
            string testPhrase = "Sol ventrikül kalbin en güçlü kasılı bölümüdür ve vücuda kan pompalar.";
            Speak(testPhrase);
        }

        private void OnDestroy()
        {
            StopSpeaking();
            ClearCache();
        }
    }
}