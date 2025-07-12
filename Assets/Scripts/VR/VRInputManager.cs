using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace HeartLabVR.VR
{
    /// <summary>
    /// Manages VR input interactions for Meta Quest optimization
    /// Handles hand tracking, gaze input, and voice commands
    /// </summary>
    public class VRInputManager : MonoBehaviour
    {
        [Header("Input Configuration")]
        [SerializeField] private bool enableHandTracking = true;
        [SerializeField] private bool enableGazeInput = true;
        [SerializeField] private bool enableVoiceInput = true;
        [SerializeField] private float interactionDistance = 2f;

        [Header("Performance Settings")]
        [SerializeField] private int targetFrameRate = 90;
        [SerializeField] private bool optimizeForMobileVR = true;
        [SerializeField] private float inputSampleRate = 120f;

        [Header("Hand Tracking")]
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private Transform rightHandTransform;
        [SerializeField] private LayerMask interactableLayers = -1;

        public static VRInputManager Instance { get; private set; }

        // Input Events
        public event Action<Vector3> OnHandPointingStarted;
        public event Action OnHandPointingEnded;
        public event Action<string> OnVoiceCommandDetected;
        public event Action<Transform> OnHandGrabStarted;
        public event Action OnHandGrabEnded;

        // Input State
        public bool IsHandTrackingActive { get; private set; }
        public bool IsGazeTrackingActive { get; private set; }
        public Transform CurrentInteractedObject { get; private set; }

        private InputActionAsset inputActions;
        private InputAction handTrackingAction;
        private InputAction grabAction;
        private GazeTracker gazeTracker;
        private VoiceRecognition voiceRecognition;

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
            // Set target frame rate for VR
            Application.targetFrameRate = targetFrameRate;
            
            if (optimizeForMobileVR)
            {
                QualitySettings.vSyncCount = 0;
                Time.fixedDeltaTime = 1f / inputSampleRate;
            }

            // Get components
            gazeTracker = GetComponent<GazeTracker>();
            voiceRecognition = GetComponent<VoiceRecognition>();

            // Setup input actions
            SetupInputActions();

            // Enable input systems
            if (enableGazeInput && gazeTracker != null)
            {
                gazeTracker.OnGazeHit += HandleGazeHit;
                gazeTracker.OnGazeLost += HandleGazeLost;
                IsGazeTrackingActive = true;
            }

            if (enableVoiceInput && voiceRecognition != null)
            {
                voiceRecognition.OnSpeechRecognized += HandleVoiceCommand;
            }
        }

        private void SetupInputActions()
        {
            try
            {
                // Create input actions for VR controllers
                inputActions = ScriptableObject.CreateInstance<InputActionAsset>();
                
                var actionMap = inputActions.AddActionMap("VR");
                
                handTrackingAction = actionMap.AddAction("HandTracking", InputActionType.Value);
                handTrackingAction.AddBinding("<XRController>/position");
                
                grabAction = actionMap.AddAction("Grab", InputActionType.Button);
                grabAction.AddBinding("<XRController>/trigger");
                
                // Enable actions
                inputActions.Enable();
                
                // Subscribe to events
                grabAction.performed += OnGrabPerformed;
                grabAction.canceled += OnGrabCanceled;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to setup input actions: {ex.Message}");
            }
        }

        private void Update()
        {
            if (enableHandTracking)
            {
                UpdateHandTracking();
            }
        }

        private void UpdateHandTracking()
        {
            // Update hand tracking state
            bool handTrackingDetected = IsHandTrackingDetected();
            
            if (handTrackingDetected != IsHandTrackingActive)
            {
                IsHandTrackingActive = handTrackingDetected;
                
                if (IsHandTrackingActive)
                {
                    Debug.Log("Hand tracking activated");
                }
                else
                {
                    Debug.Log("Hand tracking deactivated");
                    OnHandPointingEnded?.Invoke();
                }
            }

            if (IsHandTrackingActive)
            {
                ProcessHandInteractions();
            }
        }

        private bool IsHandTrackingDetected()
        {
            // Check if hand tracking is available and hands are detected
            if (leftHandTransform != null && rightHandTransform != null)
            {
                return leftHandTransform.gameObject.activeInHierarchy || 
                       rightHandTransform.gameObject.activeInHierarchy;
            }
            return false;
        }

        private void ProcessHandInteractions()
        {
            // Process hand pointing and interaction
            Transform activeHand = GetActiveHand();
            
            if (activeHand != null)
            {
                Vector3 handPosition = activeHand.position;
                Vector3 handForward = activeHand.forward;
                
                // Raycast from hand position
                if (Physics.Raycast(handPosition, handForward, out RaycastHit hit, interactionDistance, interactableLayers))
                {
                    Transform hitObject = hit.transform;
                    
                    if (CurrentInteractedObject != hitObject)
                    {
                        CurrentInteractedObject = hitObject;
                        OnHandPointingStarted?.Invoke(hit.point);
                    }
                }
                else if (CurrentInteractedObject != null)
                {
                    CurrentInteractedObject = null;
                    OnHandPointingEnded?.Invoke();
                }
            }
        }

        private Transform GetActiveHand()
        {
            // Return the hand that is actively pointing or interacting
            if (leftHandTransform != null && leftHandTransform.gameObject.activeInHierarchy)
            {
                return leftHandTransform;
            }
            else if (rightHandTransform != null && rightHandTransform.gameObject.activeInHierarchy)
            {
                return rightHandTransform;
            }
            return null;
        }

        private void OnGrabPerformed(InputAction.CallbackContext context)
        {
            if (CurrentInteractedObject != null)
            {
                OnHandGrabStarted?.Invoke(CurrentInteractedObject);
            }
        }

        private void OnGrabCanceled(InputAction.CallbackContext context)
        {
            OnHandGrabEnded?.Invoke();
        }

        private void HandleGazeHit(Transform hitObject, Vector3 hitPoint)
        {
            // Handle gaze-based selection
            if (hitObject.CompareTag("HeartPart"))
            {
                var heartPart = hitObject.GetComponent<HeartLabVR.HeartModel.HeartPartDetector>();
                if (heartPart != null)
                {
                    heartPart.OnGazeEnter();
                }
            }
        }

        private void HandleGazeLost()
        {
            // Handle gaze loss
            var heartParts = FindObjectsOfType<HeartLabVR.HeartModel.HeartPartDetector>();
            foreach (var part in heartParts)
            {
                part.OnGazeExit();
            }
        }

        private void HandleVoiceCommand(string command)
        {
            OnVoiceCommandDetected?.Invoke(command);
        }

        /// <summary>
        /// Enables or disables hand tracking
        /// </summary>
        /// <param name="enabled">Enable state</param>
        public void SetHandTrackingEnabled(bool enabled)
        {
            enableHandTracking = enabled;
            
            if (!enabled)
            {
                IsHandTrackingActive = false;
                CurrentInteractedObject = null;
                OnHandPointingEnded?.Invoke();
            }
        }

        /// <summary>
        /// Enables or disables gaze input
        /// </summary>
        /// <param name="enabled">Enable state</param>
        public void SetGazeInputEnabled(bool enabled)
        {
            enableGazeInput = enabled;
            
            if (gazeTracker != null)
            {
                gazeTracker.enabled = enabled;
            }
            
            IsGazeTrackingActive = enabled;
        }

        /// <summary>
        /// Enables or disables voice input
        /// </summary>
        /// <param name="enabled">Enable state</param>
        public void SetVoiceInputEnabled(bool enabled)
        {
            enableVoiceInput = enabled;
            
            if (voiceRecognition != null)
            {
                voiceRecognition.enabled = enabled;
            }
        }

        /// <summary>
        /// Gets current input state information
        /// </summary>
        /// <returns>Input state as string</returns>
        public string GetInputState()
        {
            return $"Hand Tracking: {IsHandTrackingActive}, Gaze: {IsGazeTrackingActive}, " +
                   $"Interacted Object: {(CurrentInteractedObject != null ? CurrentInteractedObject.name : "None")}, " +
                   $"FPS: {(1f / Time.unscaledDeltaTime):F1}";
        }

        /// <summary>
        /// Triggers haptic feedback on controllers
        /// </summary>
        /// <param name="intensity">Haptic intensity (0-1)</param>
        /// <param name="duration">Duration in seconds</param>
        public void TriggerHapticFeedback(float intensity = 0.5f, float duration = 0.1f)
        {
            // Trigger haptic feedback on VR controllers
            var controllers = FindObjectsOfType<XRBaseController>();
            foreach (var controller in controllers)
            {
                controller.SendHapticImpulse(intensity, duration);
            }
        }

        private void OnDestroy()
        {
            if (inputActions != null)
            {
                inputActions.Disable();
                inputActions = null;
            }

            if (gazeTracker != null)
            {
                gazeTracker.OnGazeHit -= HandleGazeHit;
                gazeTracker.OnGazeLost -= HandleGazeLost;
            }

            if (voiceRecognition != null)
            {
                voiceRecognition.OnSpeechRecognized -= HandleVoiceCommand;
            }
        }

        private void OnEnable()
        {
            inputActions?.Enable();
        }

        private void OnDisable()
        {
            inputActions?.Disable();
        }
    }
}