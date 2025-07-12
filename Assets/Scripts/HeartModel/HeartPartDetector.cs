using System;
using UnityEngine;

namespace HeartLabVR.HeartModel
{
    /// <summary>
    /// Detects interactions with heart parts (gaze, touch, selection)
    /// Handles part-specific events and visual feedback
    /// </summary>
    public class HeartPartDetector : MonoBehaviour
    {
        [Header("Part Configuration")]
        [SerializeField] private string partName = "";
        [SerializeField] private string displayName = "";
        [SerializeField] private HeartPartType partType = HeartPartType.Other;
        [SerializeField] private bool isInteractable = true;
        
        [Header("Interaction Settings")]
        [SerializeField] private float gazeHoldTime = 1f;
        [SerializeField] private bool enableHapticFeedback = true;
        [SerializeField] private float hapticIntensity = 0.5f;
        
        [Header("Visual Feedback")]
        [SerializeField] private bool showHoverEffect = true;
        [SerializeField] private bool showSelectionEffect = true;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private Color selectionColor = Color.green;

        public event Action OnPartSelected;
        public event Action OnPartHighlighted;
        public event Action OnPartDeselected;

        public string PartName => string.IsNullOrEmpty(partName) ? gameObject.name : partName;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? PartName : displayName;
        public HeartPartType PartType => partType;
        public bool IsInteractable => isInteractable;
        public bool IsCurrentlySelected { get; private set; }
        public bool IsCurrentlyHighlighted { get; private set; }

        private Renderer partRenderer;
        private Material originalMaterial;
        private Material hoverMaterial;
        private Material selectionMaterial;
        private Collider partCollider;
        private bool isGazing;
        private float gazeStartTime;
        private HeartLabVR.VR.VRInputManager vrInputManager;

        private void Awake()
        {
            InitializeComponent();
        }

        private void Start()
        {
            vrInputManager = HeartLabVR.VR.VRInputManager.Instance;
            SetupMaterials();
        }

        private void InitializeComponent()
        {
            partRenderer = GetComponent<Renderer>();
            if (partRenderer == null)
            {
                Debug.LogWarning($"HeartPartDetector on {gameObject.name} requires a Renderer component!");
            }

            partCollider = GetComponent<Collider>();
            if (partCollider == null)
            {
                // Add a collider if none exists
                partCollider = gameObject.AddComponent<MeshCollider>();
                if (partCollider is MeshCollider meshCollider)
                {
                    meshCollider.convex = true;
                }
            }

            // Ensure the part has the correct tag
            if (!gameObject.CompareTag("HeartPart"))
            {
                gameObject.tag = "HeartPart";
            }

            // Auto-detect part name and type if not set
            if (string.IsNullOrEmpty(partName))
            {
                AutoDetectPartInfo();
            }
        }

        private void AutoDetectPartInfo()
        {
            string objectName = gameObject.name.ToLowerTurkish();
            
            // Auto-detect part type based on name
            if (objectName.Contains("ventrikul") || objectName.Contains("ventricle") || 
                objectName.Contains("atrium") || objectName.Contains("kulakçık"))
            {
                partType = HeartPartType.Chamber;
            }
            else if (objectName.Contains("kapak") || objectName.Contains("valve"))
            {
                partType = HeartPartType.Valve;
            }
            else if (objectName.Contains("arter") || objectName.Contains("artery") || 
                     objectName.Contains("ven") || objectName.Contains("vein") || 
                     objectName.Contains("aorta"))
            {
                partType = HeartPartType.Vessel;
            }
            else if (objectName.Contains("kas") || objectName.Contains("muscle") || 
                     objectName.Contains("miyokard") || objectName.Contains("myocardium"))
            {
                partType = HeartPartType.Muscle;
            }

            partName = objectName.Replace(" ", "_").ToLowerTurkish();
            displayName = FormatDisplayName(gameObject.name);
        }

        private string FormatDisplayName(string name)
        {
            // Clean up the name for display
            return name.Replace("_", " ").Replace("(Clone)", "").Trim();
        }

        private void SetupMaterials()
        {
            if (partRenderer != null)
            {
                originalMaterial = partRenderer.material;
                
                // Create hover material
                if (showHoverEffect)
                {
                    hoverMaterial = new Material(originalMaterial);
                    hoverMaterial.color = hoverColor;
                    hoverMaterial.SetFloat("_Metallic", 0.3f);
                    hoverMaterial.SetFloat("_Smoothness", 0.7f);
                }

                // Create selection material
                if (showSelectionEffect)
                {
                    selectionMaterial = new Material(originalMaterial);
                    selectionMaterial.color = selectionColor;
                    selectionMaterial.SetFloat("_Metallic", 0.5f);
                    selectionMaterial.SetFloat("_Smoothness", 0.8f);
                    selectionMaterial.EnableKeyword("_EMISSION");
                    selectionMaterial.SetColor("_EmissionColor", selectionColor * 0.3f);
                }
            }
        }

        private void Update()
        {
            // Handle gaze selection with dwell time
            if (isGazing && gazeHoldTime > 0)
            {
                if (Time.time - gazeStartTime >= gazeHoldTime)
                {
                    SelectPart();
                    isGazing = false; // Prevent multiple selections
                }
            }
        }

        /// <summary>
        /// Called when gaze enters this part
        /// </summary>
        public void OnGazeEnter()
        {
            if (!isInteractable)
                return;

            isGazing = true;
            gazeStartTime = Time.time;
            
            HighlightPart();
        }

        /// <summary>
        /// Called when gaze exits this part
        /// </summary>
        public void OnGazeExit()
        {
            isGazing = false;
            
            if (IsCurrentlyHighlighted && !IsCurrentlySelected)
            {
                UnhighlightPart();
            }
        }

        /// <summary>
        /// Called when part is clicked/touched
        /// </summary>
        public void OnClick()
        {
            if (!isInteractable)
                return;

            SelectPart();
        }

        /// <summary>
        /// Called when hand tracking detects pointing at this part
        /// </summary>
        public void OnHandPointing()
        {
            if (!isInteractable)
                return;

            HighlightPart();
        }

        /// <summary>
        /// Called when hand tracking stops pointing at this part
        /// </summary>
        public void OnHandStopPointing()
        {
            if (IsCurrentlyHighlighted && !IsCurrentlySelected)
            {
                UnhighlightPart();
            }
        }

        /// <summary>
        /// Highlights this heart part
        /// </summary>
        public void HighlightPart()
        {
            if (IsCurrentlyHighlighted)
                return;

            IsCurrentlyHighlighted = true;
            
            if (showHoverEffect && hoverMaterial != null && partRenderer != null)
            {
                partRenderer.material = hoverMaterial;
            }

            OnPartHighlighted?.Invoke();
            
            Debug.Log($"Highlighted heart part: {DisplayName}");
        }

        /// <summary>
        /// Removes highlight from this heart part
        /// </summary>
        public void UnhighlightPart()
        {
            if (!IsCurrentlyHighlighted)
                return;

            IsCurrentlyHighlighted = false;
            
            if (partRenderer != null)
            {
                partRenderer.material = IsCurrentlySelected ? selectionMaterial : originalMaterial;
            }
        }

        /// <summary>
        /// Selects this heart part
        /// </summary>
        public void SelectPart()
        {
            if (IsCurrentlySelected)
                return;

            IsCurrentlySelected = true;
            
            if (showSelectionEffect && selectionMaterial != null && partRenderer != null)
            {
                partRenderer.material = selectionMaterial;
            }

            // Trigger haptic feedback
            if (enableHapticFeedback && vrInputManager != null)
            {
                vrInputManager.TriggerHapticFeedback(hapticIntensity, 0.1f);
            }

            OnPartSelected?.Invoke();
            
            Debug.Log($"Selected heart part: {DisplayName}");
        }

        /// <summary>
        /// Deselects this heart part
        /// </summary>
        public void DeselectPart()
        {
            if (!IsCurrentlySelected)
                return;

            IsCurrentlySelected = false;
            
            if (partRenderer != null)
            {
                partRenderer.material = IsCurrentlyHighlighted ? hoverMaterial : originalMaterial;
            }

            OnPartDeselected?.Invoke();
            
            Debug.Log($"Deselected heart part: {DisplayName}");
        }

        /// <summary>
        /// Resets the part to default state
        /// </summary>
        public void ResetPart()
        {
            isGazing = false;
            IsCurrentlyHighlighted = false;
            IsCurrentlySelected = false;
            
            if (partRenderer != null)
            {
                partRenderer.material = originalMaterial;
            }
        }

        /// <summary>
        /// Sets the interactable state of this part
        /// </summary>
        /// <param name="interactable">Whether the part should be interactable</param>
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            
            if (partCollider != null)
            {
                partCollider.enabled = interactable;
            }

            if (!interactable)
            {
                ResetPart();
            }
        }

        /// <summary>
        /// Gets detailed information about this part
        /// </summary>
        /// <returns>Part information as string</returns>
        public string GetPartInfo()
        {
            var anatomicalMapping = FindObjectOfType<AnatomicalMapping>();
            string description = "";
            
            if (anatomicalMapping != null)
            {
                description = anatomicalMapping.GetDetailedDescription(PartName);
            }

            return $"Name: {DisplayName} ({PartName})\n" +
                   $"Type: {PartType}\n" +
                   $"Interactable: {IsInteractable}\n" +
                   $"Selected: {IsCurrentlySelected}\n" +
                   $"Highlighted: {IsCurrentlyHighlighted}\n" +
                   $"Description: {description}";
        }

        // Unity Event Methods for Inspector integration
        private void OnMouseDown()
        {
            OnClick();
        }

        private void OnMouseEnter()
        {
            if (!isGazing) // Avoid duplicate calls from gaze tracking
            {
                HighlightPart();
            }
        }

        private void OnMouseExit()
        {
            if (!isGazing) // Avoid conflicts with gaze tracking
            {
                if (IsCurrentlyHighlighted && !IsCurrentlySelected)
                {
                    UnhighlightPart();
                }
            }
        }

        // Gizmos for editor visualization
        private void OnDrawGizmosSelected()
        {
            if (partCollider != null)
            {
                Gizmos.color = IsCurrentlySelected ? Color.green : 
                              IsCurrentlyHighlighted ? Color.yellow : Color.white;
                
                if (partCollider is BoxCollider boxCollider)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
                }
                else if (partCollider is SphereCollider sphereCollider)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
                }
                else if (partCollider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireMesh(meshCollider.sharedMesh, transform.position, transform.rotation, transform.localScale);
                }
            }

            // Draw part type indicator
            Gizmos.color = GetTypeColor();
            Gizmos.DrawSphere(transform.position + Vector3.up * 0.1f, 0.02f);
        }

        private Color GetTypeColor()
        {
            switch (partType)
            {
                case HeartPartType.Chamber: return Color.red;
                case HeartPartType.Valve: return Color.blue;
                case HeartPartType.Vessel: return Color.green;
                case HeartPartType.Muscle: return Color.magenta;
                default: return Color.gray;
            }
        }

        private void OnDestroy()
        {
            // Clean up created materials
            if (hoverMaterial != null && hoverMaterial != originalMaterial)
            {
                DestroyImmediate(hoverMaterial);
            }
            
            if (selectionMaterial != null && selectionMaterial != originalMaterial)
            {
                DestroyImmediate(selectionMaterial);
            }
        }
    }
}