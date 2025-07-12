using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace HeartLabVR.AI
{
    /// <summary>
    /// Manages connections to Hugging Face API for medical AI responses
    /// Supports PubMedBERT and ClinicalBERT models with Turkish language support
    /// </summary>
    public class HuggingFaceAPI : MonoBehaviour
    {
        [Header("API Configuration")]
        [SerializeField] private string apiKey = "";
        [SerializeField] private string baseUrl = "https://api-inference.huggingface.co/models/";
        [SerializeField] private string medicalModel = "microsoft/BiomedNLP-PubMedBERT-base-uncased-abstract-fulltext";
        [SerializeField] private float requestTimeout = 10f;

        [Header("Performance Settings")]
        [SerializeField] private int maxResponseLength = 512;
        [SerializeField] private float temperature = 0.7f;
        [SerializeField] private bool useCache = true;

        public static HuggingFaceAPI Instance { get; private set; }

        private ResponseCache responseCache;
        private MedicalPromptManager promptManager;

        public event Action<string> OnResponseReceived;
        public event Action<string> OnErrorOccurred;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            responseCache = GetComponent<ResponseCache>();
            promptManager = GetComponent<MedicalPromptManager>();
            
            // Load API key from environment or config
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable("HUGGINGFACE_API_KEY");
            }
        }

        /// <summary>
        /// Sends a medical query to the AI model with anatomical context
        /// </summary>
        /// <param name="query">User's medical question in Turkish</param>
        /// <param name="anatomicalContext">Currently selected heart part</param>
        public void SendMedicalQuery(string query, string anatomicalContext = "")
        {
            StartCoroutine(ProcessMedicalQuery(query, anatomicalContext));
        }

        private IEnumerator ProcessMedicalQuery(string query, string anatomicalContext)
        {
            try
            {
                // Check cache first for performance
                if (useCache && responseCache != null)
                {
                    string cachedResponse = responseCache.GetCachedResponse(query, anatomicalContext);
                    if (!string.IsNullOrEmpty(cachedResponse))
                    {
                        OnResponseReceived?.Invoke(cachedResponse);
                        yield break;
                    }
                }

                // Prepare the medical prompt with context
                string enhancedPrompt = promptManager.CreateMedicalPrompt(query, anatomicalContext);
                
                // Create the API request
                var requestData = new
                {
                    inputs = enhancedPrompt,
                    parameters = new
                    {
                        max_new_tokens = maxResponseLength,
                        temperature = temperature,
                        return_full_text = false
                    }
                };

                string jsonData = JsonConvert.SerializeObject(requestData);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

                using (UnityWebRequest request = new UnityWebRequest(baseUrl + medicalModel, "POST"))
                {
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                    request.timeout = (int)requestTimeout;

                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string response = ProcessAPIResponse(request.downloadHandler.text);
                        
                        // Cache the response for future use
                        if (useCache && responseCache != null)
                        {
                            responseCache.CacheResponse(query, anatomicalContext, response);
                        }

                        OnResponseReceived?.Invoke(response);
                    }
                    else
                    {
                        string fallbackResponse = GetFallbackResponse(query, anatomicalContext);
                        OnResponseReceived?.Invoke(fallbackResponse);
                        OnErrorOccurred?.Invoke($"API Error: {request.error}");
                    }
                }
            }
            catch (Exception ex)
            {
                string fallbackResponse = GetFallbackResponse(query, anatomicalContext);
                OnResponseReceived?.Invoke(fallbackResponse);
                OnErrorOccurred?.Invoke($"Exception: {ex.Message}");
            }
        }

        private string ProcessAPIResponse(string rawResponse)
        {
            try
            {
                var responseObj = JsonConvert.DeserializeObject<dynamic>(rawResponse);
                if (responseObj is Newtonsoft.Json.Linq.JArray responseArray && responseArray.Count > 0)
                {
                    return responseArray[0]["generated_text"]?.ToString() ?? GetDefaultResponse();
                }
                return GetDefaultResponse();
            }
            catch
            {
                return GetDefaultResponse();
            }
        }

        private string GetFallbackResponse(string query, string anatomicalContext)
        {
            // Return cached educational content or basic anatomical information
            return promptManager.GetOfflineFallback(query, anatomicalContext);
        }

        private string GetDefaultResponse()
        {
            return "Kalp anatomi bilgilerini öğrenmek için gözünüzü kalbin farklı bölümlerine çevirin ve sesli sorular sorun.";
        }

        public void SetAPIKey(string newApiKey)
        {
            apiKey = newApiKey;
        }

        public bool IsConfigured()
        {
            return !string.IsNullOrEmpty(apiKey);
        }
    }
}