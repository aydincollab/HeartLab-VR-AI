using System;
using System.Collections.Generic;
using UnityEngine;
using HeartLabVR.AI;

namespace HeartLabVR.HeartModel
{
    /// <summary>
    /// Manages heart parts and their interactions
    /// Handles highlighting, selection, and educational content display
    /// </summary>
    public class HeartPartManager : MonoBehaviour
    {
        [Header("Heart Model Configuration")]
        [SerializeField] private Transform heartModelRoot;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material selectedMaterial;
        [SerializeField] private Material defaultMaterial;
        
        [Header("Animation Settings")]
        [SerializeField] private float highlightAnimationSpeed = 2f;
        [SerializeField] private bool enablePulseAnimation = true;
        [SerializeField] private float pulseSpeed = 1.2f; // Heartbeat BPM equivalent
        
        [Header("Educational Settings")]
        [SerializeField] private bool showLabelsOnHover = true;
        [SerializeField] private bool autoPlayAudio = true;
        [SerializeField] private float autoPlayDelay = 1f;

        public static HeartPartManager Instance { get; private set; }

        public event Action<HeartPart> OnPartSelected;
        public event Action<HeartPart> OnPartHighlighted;
        public event Action OnPartDeselected;

        public HeartPart CurrentSelectedPart { get; private set; }
        public List<HeartPart> HeartParts { get; private set; }

        private AnatomicalMapping anatomicalMapping;
        private MedicalPromptManager promptManager;
        private Dictionary<string, HeartPart> partLookup;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            HeartParts = new List<HeartPart>();
            partLookup = new Dictionary<string, HeartPart>();
            
            anatomicalMapping = GetComponent<AnatomicalMapping>();
            promptManager = FindObjectOfType<MedicalPromptManager>();
            
            InitializeHeartParts();
            SetupHeartAnimation();
        }

        private void InitializeHeartParts()
        {
            if (heartModelRoot == null)
            {
                Debug.LogError("Heart model root not assigned!");
                return;
            }

            // Find all heart part detectors in the model
            HeartPartDetector[] detectors = heartModelRoot.GetComponentsInChildren<HeartPartDetector>();
            
            foreach (var detector in detectors)
            {
                HeartPart heartPart = new HeartPart(detector);
                HeartParts.Add(heartPart);
                partLookup[heartPart.PartName] = heartPart;
                
                // Subscribe to detector events
                detector.OnPartSelected += () => SelectPart(heartPart);
                detector.OnPartHighlighted += () => HighlightPart(heartPart);
                detector.OnPartDeselected += DeselectCurrentPart;
            }

            Debug.Log($"Initialized {HeartParts.Count} heart parts");
        }

        private void SetupHeartAnimation()
        {
            if (enablePulseAnimation)
            {
                // Add heartbeat animation to the entire heart model
                var heartAnimator = heartModelRoot.gameObject.GetComponent<Animator>();
                if (heartAnimator == null)
                {
                    heartAnimator = heartModelRoot.gameObject.AddComponent<Animator>();
                }
                
                // Create a simple pulse animation
                StartPulseAnimation();
            }
        }

        private void StartPulseAnimation()
        {
            if (heartModelRoot != null)
            {
                StartCoroutine(PulseHeartCoroutine());
            }
        }

        private System.Collections.IEnumerator PulseHeartCoroutine()
        {
            Vector3 originalScale = heartModelRoot.localScale;
            float pulseInterval = 60f / pulseSpeed; // Convert BPM to seconds
            
            while (this != null && heartModelRoot != null)
            {
                // Systole (contraction) - slightly smaller
                float contractionDuration = pulseInterval * 0.3f;
                yield return StartCoroutine(ScaleTo(originalScale * 0.95f, contractionDuration));
                
                // Diastole (relaxation) - back to original
                float relaxationDuration = pulseInterval * 0.7f;
                yield return StartCoroutine(ScaleTo(originalScale, relaxationDuration));
            }
        }

        private System.Collections.IEnumerator ScaleTo(Vector3 targetScale, float duration)
        {
            Vector3 startScale = heartModelRoot.localScale;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                
                // Use a smooth ease-in-out curve for natural heartbeat
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
                heartModelRoot.localScale = Vector3.Lerp(startScale, targetScale, smoothProgress);
                
                yield return null;
            }
            
            heartModelRoot.localScale = targetScale;
        }

        /// <summary>
        /// Selects a heart part and triggers educational content
        /// </summary>
        /// <param name="heartPart">Heart part to select</param>
        public void SelectPart(HeartPart heartPart)
        {
            if (heartPart == null)
                return;

            // Deselect current part first
            if (CurrentSelectedPart != null)
            {
                DeselectCurrentPart();
            }

            CurrentSelectedPart = heartPart;
            heartPart.SetSelected(true);
            
            // Apply selection visual effects
            ApplyMaterial(heartPart, selectedMaterial);
            
            OnPartSelected?.Invoke(heartPart);
            
            // Show educational content
            if (autoPlayAudio && promptManager != null)
            {
                string educationalContent = promptManager.GetAnatomicalEducation(heartPart.PartName);
                var tts = FindObjectOfType<HeartLabVR.VR.TextToSpeech>();
                if (tts != null)
                {
                    tts.Speak(educationalContent);
                }
            }

            Debug.Log($"Selected heart part: {heartPart.PartName}");
        }

        /// <summary>
        /// Highlights a heart part on hover/gaze
        /// </summary>
        /// <param name="heartPart">Heart part to highlight</param>
        public void HighlightPart(HeartPart heartPart)
        {
            if (heartPart == null || heartPart == CurrentSelectedPart)
                return;

            // Remove highlight from other parts
            foreach (var part in HeartParts)
            {
                if (part != heartPart && part != CurrentSelectedPart && part.IsHighlighted)
                {
                    part.SetHighlighted(false);
                    ApplyMaterial(part, defaultMaterial);
                }
            }

            heartPart.SetHighlighted(true);
            ApplyMaterial(heartPart, highlightMaterial);
            
            OnPartHighlighted?.Invoke(heartPart);
            
            // Show label if enabled
            if (showLabelsOnHover)
            {
                ShowPartLabel(heartPart);
            }
        }

        /// <summary>
        /// Deselects the currently selected part
        /// </summary>
        public void DeselectCurrentPart()
        {
            if (CurrentSelectedPart != null)
            {
                CurrentSelectedPart.SetSelected(false);
                ApplyMaterial(CurrentSelectedPart, defaultMaterial);
                
                HidePartLabel(CurrentSelectedPart);
                
                CurrentSelectedPart = null;
                OnPartDeselected?.Invoke();
            }
        }

        /// <summary>
        /// Gets a heart part by name
        /// </summary>
        /// <param name="partName">Name of the part</param>
        /// <returns>Heart part or null if not found</returns>
        public HeartPart GetPartByName(string partName)
        {
            return partLookup.ContainsKey(partName) ? partLookup[partName] : null;
        }

        /// <summary>
        /// Gets all parts of a specific type
        /// </summary>
        /// <param name="partType">Type of heart part</param>
        /// <returns>List of matching parts</returns>
        public List<HeartPart> GetPartsByType(HeartPartType partType)
        {
            var matchingParts = new List<HeartPart>();
            
            foreach (var part in HeartParts)
            {
                if (part.PartType == partType)
                {
                    matchingParts.Add(part);
                }
            }
            
            return matchingParts;
        }

        /// <summary>
        /// Highlights parts by type (e.g., all chambers, all valves)
        /// </summary>
        /// <param name="partType">Type to highlight</param>
        public void HighlightPartsByType(HeartPartType partType)
        {
            var partsToHighlight = GetPartsByType(partType);
            
            foreach (var part in partsToHighlight)
            {
                HighlightPart(part);
            }
        }

        /// <summary>
        /// Resets all heart parts to default state
        /// </summary>
        public void ResetAllParts()
        {
            DeselectCurrentPart();
            
            foreach (var part in HeartParts)
            {
                part.SetHighlighted(false);
                part.SetSelected(false);
                ApplyMaterial(part, defaultMaterial);
                HidePartLabel(part);
            }
        }

        private void ApplyMaterial(HeartPart heartPart, Material material)
        {
            if (heartPart?.Renderer != null && material != null)
            {
                heartPart.Renderer.material = material;
            }
        }

        private void ShowPartLabel(HeartPart heartPart)
        {
            // Show UI label for the part
            var uiManager = FindObjectOfType<HeartLabVR.UI.VRUIManager>();
            if (uiManager != null)
            {
                uiManager.ShowPartLabel(heartPart.DisplayName, heartPart.Transform.position);
            }
        }

        private void HidePartLabel(HeartPart heartPart)
        {
            // Hide UI label
            var uiManager = FindObjectOfType<HeartLabVR.UI.VRUIManager>();
            if (uiManager != null)
            {
                uiManager.HidePartLabel();
            }
        }

        /// <summary>
        /// Sets the pulse animation speed (simulates heart rate)
        /// </summary>
        /// <param name="bpm">Beats per minute</param>
        public void SetHeartRate(float bpm)
        {
            pulseSpeed = Mathf.Clamp(bpm, 30f, 200f);
        }

        /// <summary>
        /// Enables or disables the heartbeat animation
        /// </summary>
        /// <param name="enabled">Enable pulse animation</param>
        public void SetPulseAnimationEnabled(bool enabled)
        {
            enablePulseAnimation = enabled;
            
            if (enabled)
            {
                StartPulseAnimation();
            }
            else
            {
                StopCoroutine(PulseHeartCoroutine());
                if (heartModelRoot != null)
                {
                    heartModelRoot.localScale = Vector3.one;
                }
            }
        }

        /// <summary>
        /// Gets statistics about the heart model
        /// </summary>
        /// <returns>Heart model statistics</returns>
        public string GetHeartModelStats()
        {
            int chambers = GetPartsByType(HeartPartType.Chamber).Count;
            int valves = GetPartsByType(HeartPartType.Valve).Count;
            int vessels = GetPartsByType(HeartPartType.Vessel).Count;
            
            return $"Total Parts: {HeartParts.Count}, Chambers: {chambers}, Valves: {valves}, " +
                   $"Vessels: {vessels}, Selected: {(CurrentSelectedPart?.PartName ?? "None")}, " +
                   $"Heart Rate: {pulseSpeed:F0} BPM";
        }
    }

    /// <summary>
    /// Represents a heart part with its properties and state
    /// </summary>
    [System.Serializable]
    public class HeartPart
    {
        public string PartName { get; private set; }
        public string DisplayName { get; private set; }
        public HeartPartType PartType { get; private set; }
        public Transform Transform { get; private set; }
        public Renderer Renderer { get; private set; }
        public HeartPartDetector Detector { get; private set; }
        
        public bool IsSelected { get; private set; }
        public bool IsHighlighted { get; private set; }

        public HeartPart(HeartPartDetector detector)
        {
            Detector = detector;
            Transform = detector.transform;
            Renderer = detector.GetComponent<Renderer>();
            
            PartName = detector.PartName;
            DisplayName = detector.DisplayName;
            PartType = detector.PartType;
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
        }

        public void SetHighlighted(bool highlighted)
        {
            IsHighlighted = highlighted;
        }
    }

    /// <summary>
    /// Types of heart parts for categorization
    /// </summary>
    public enum HeartPartType
    {
        Chamber,    // Ventricles and Atria
        Valve,      // Heart valves
        Vessel,     // Arteries and veins
        Muscle,     // Heart muscle tissue
        Other       // Other anatomical structures
    }
}