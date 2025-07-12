using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HeartLabVR.UI
{
    /// <summary>
    /// Manages VR user interface elements and interactions
    /// Handles part labels, educational panels, and VR-optimized UI
    /// </summary>
    public class VRUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private EducationalPanel educationalPanel;
        [SerializeField] private GameObject partLabelPrefab;
        [SerializeField] private Transform labelContainer;
        
        [Header("VR UI Settings")]
        [SerializeField] private float uiDistance = 2f;
        [SerializeField] private bool followUser = true;
        [SerializeField] private float followSpeed = 2f;
        [SerializeField] private bool autoHideUI = true;
        [SerializeField] private float autoHideDelay = 5f;

        [Header("Label Settings")]
        [SerializeField] private Vector3 labelOffset = new Vector3(0, 0.2f, 0);
        [SerializeField] private float labelFadeSpeed = 2f;
        [SerializeField] private bool worldSpaceLabels = true;

        public static VRUIManager Instance { get; private set; }

        public event Action OnUIShown;
        public event Action OnUIHidden;

        private Camera vrCamera;
        private GameObject currentLabel;
        private Coroutine autoHideCoroutine;
        private bool isUIVisible = true;

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
            // Find VR camera
            vrCamera = Camera.main;
            if (vrCamera == null)
            {
                vrCamera = FindObjectOfType<Camera>();
            }

            // Setup main canvas for VR
            if (mainCanvas == null)
            {
                mainCanvas = GetComponent<Canvas>();
            }

            SetupVRCanvas();
            
            // Get educational panel
            if (educationalPanel == null)
            {
                educationalPanel = GetComponentInChildren<EducationalPanel>();
            }

            // Create label container if not assigned
            if (labelContainer == null)
            {
                GameObject labelObj = new GameObject("LabelContainer");
                labelContainer = labelObj.transform;
                labelContainer.SetParent(transform);
            }
        }

        private void SetupVRCanvas()
        {
            if (mainCanvas != null)
            {
                mainCanvas.renderMode = RenderMode.WorldSpace;
                mainCanvas.worldCamera = vrCamera;
                
                // Position canvas in front of user
                if (vrCamera != null)
                {
                    Vector3 forward = vrCamera.transform.forward;
                    forward.y = 0; // Keep canvas at eye level
                    mainCanvas.transform.position = vrCamera.transform.position + forward * uiDistance;
                    mainCanvas.transform.LookAt(vrCamera.transform);
                    mainCanvas.transform.Rotate(0, 180, 0); // Face the user
                }

                // Set appropriate scale for VR
                float scaleFactor = uiDistance * 0.001f; // Adjust based on distance
                mainCanvas.transform.localScale = Vector3.one * scaleFactor;

                // Optimize for VR rendering
                var canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
                if (canvasScaler == null)
                {
                    canvasScaler = mainCanvas.gameObject.AddComponent<CanvasScaler>();
                }
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                canvasScaler.scaleFactor = 1f;

                // Add GraphicRaycaster for VR interaction
                if (mainCanvas.GetComponent<GraphicRaycaster>() == null)
                {
                    mainCanvas.gameObject.AddComponent<GraphicRaycaster>();
                }
            }
        }

        private void Update()
        {
            if (followUser && vrCamera != null && mainCanvas != null)
            {
                UpdateUIPosition();
            }
        }

        private void UpdateUIPosition()
        {
            // Smoothly follow user's gaze direction
            Vector3 targetPosition = vrCamera.transform.position + vrCamera.transform.forward * uiDistance;
            targetPosition.y = vrCamera.transform.position.y; // Keep at eye level

            mainCanvas.transform.position = Vector3.Lerp(
                mainCanvas.transform.position, 
                targetPosition, 
                followSpeed * Time.deltaTime
            );

            // Make canvas face the user
            Vector3 lookDirection = vrCamera.transform.position - mainCanvas.transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-lookDirection);
                mainCanvas.transform.rotation = Quaternion.Lerp(
                    mainCanvas.transform.rotation,
                    targetRotation,
                    followSpeed * Time.deltaTime
                );
            }
        }

        /// <summary>
        /// Shows a label for a heart part at the specified world position
        /// </summary>
        /// <param name="partName">Name of the heart part</param>
        /// <param name="worldPosition">World position for the label</param>
        public void ShowPartLabel(string partName, Vector3 worldPosition)
        {
            // Hide existing label
            HidePartLabel();

            if (partLabelPrefab == null)
            {
                CreateDefaultLabelPrefab();
            }

            // Create new label
            currentLabel = Instantiate(partLabelPrefab, labelContainer);
            
            // Set label text
            var textComponent = currentLabel.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = partName;
            }
            else
            {
                var legacyText = currentLabel.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    legacyText.text = partName;
                }
            }

            // Position label
            if (worldSpaceLabels)
            {
                currentLabel.transform.position = worldPosition + labelOffset;
                
                // Make label face camera
                if (vrCamera != null)
                {
                    currentLabel.transform.LookAt(vrCamera.transform);
                    currentLabel.transform.Rotate(0, 180, 0);
                }
            }
            else
            {
                // Convert world position to screen position for overlay canvas
                if (vrCamera != null)
                {
                    Vector3 screenPos = vrCamera.WorldToScreenPoint(worldPosition + labelOffset);
                    currentLabel.transform.position = screenPos;
                }
            }

            // Animate label appearance
            StartCoroutine(FadeInLabel(currentLabel));

            // Start auto-hide timer
            if (autoHideUI)
            {
                if (autoHideCoroutine != null)
                {
                    StopCoroutine(autoHideCoroutine);
                }
                autoHideCoroutine = StartCoroutine(AutoHideLabel());
            }
        }

        /// <summary>
        /// Hides the current part label
        /// </summary>
        public void HidePartLabel()
        {
            if (currentLabel != null)
            {
                StartCoroutine(FadeOutLabel(currentLabel));
            }

            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }
        }

        /// <summary>
        /// Shows the educational panel with content about a heart part
        /// </summary>
        /// <param name="partName">Name of the heart part</param>
        /// <param name="content">Educational content to display</param>
        public void ShowEducationalContent(string partName, string content)
        {
            if (educationalPanel != null)
            {
                educationalPanel.ShowContent(partName, content);
                ShowUI();
            }
        }

        /// <summary>
        /// Hides the educational panel
        /// </summary>
        public void HideEducationalContent()
        {
            if (educationalPanel != null)
            {
                educationalPanel.HideContent();
            }
        }

        /// <summary>
        /// Shows the main UI
        /// </summary>
        public void ShowUI()
        {
            if (!isUIVisible)
            {
                isUIVisible = true;
                
                if (mainCanvas != null)
                {
                    mainCanvas.gameObject.SetActive(true);
                }

                OnUIShown?.Invoke();
            }
        }

        /// <summary>
        /// Hides the main UI
        /// </summary>
        public void HideUI()
        {
            if (isUIVisible)
            {
                isUIVisible = false;
                
                if (mainCanvas != null)
                {
                    mainCanvas.gameObject.SetActive(false);
                }

                HidePartLabel();
                OnUIHidden?.Invoke();
            }
        }

        /// <summary>
        /// Toggles UI visibility
        /// </summary>
        public void ToggleUI()
        {
            if (isUIVisible)
            {
                HideUI();
            }
            else
            {
                ShowUI();
            }
        }

        /// <summary>
        /// Sets UI distance from user
        /// </summary>
        /// <param name="distance">Distance in meters</param>
        public void SetUIDistance(float distance)
        {
            uiDistance = Mathf.Clamp(distance, 0.5f, 5f);
            
            // Update canvas scale based on distance
            if (mainCanvas != null)
            {
                float scaleFactor = uiDistance * 0.001f;
                mainCanvas.transform.localScale = Vector3.one * scaleFactor;
            }
        }

        /// <summary>
        /// Sets whether UI should follow user's gaze
        /// </summary>
        /// <param name="follow">Enable follow mode</param>
        public void SetFollowUser(bool follow)
        {
            followUser = follow;
        }

        /// <summary>
        /// Shows a notification message
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="duration">Duration in seconds</param>
        public void ShowNotification(string message, float duration = 3f)
        {
            StartCoroutine(ShowNotificationCoroutine(message, duration));
        }

        private void CreateDefaultLabelPrefab()
        {
            // Create a simple label prefab if none is assigned
            GameObject labelGO = new GameObject("PartLabel");
            
            // Add Canvas for world space rendering
            Canvas labelCanvas = labelGO.AddComponent<Canvas>();
            labelCanvas.renderMode = RenderMode.WorldSpace;
            
            // Add CanvasScaler
            CanvasScaler scaler = labelGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantWorldSize;
            scaler.scaleFactor = 0.01f;
            
            // Add GraphicRaycaster
            labelGO.AddComponent<GraphicRaycaster>();
            
            // Create background panel
            GameObject bgPanel = new GameObject("Background");
            bgPanel.transform.SetParent(labelGO.transform);
            
            RectTransform bgRect = bgPanel.AddComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(200, 50);
            
            Image bgImage = bgPanel.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);
            
            // Create text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(bgPanel.transform);
            
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "Part Label";
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            
            partLabelPrefab = labelGO;
        }

        private IEnumerator FadeInLabel(GameObject label)
        {
            if (label == null) yield break;

            CanvasGroup canvasGroup = label.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = label.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;
            
            while (canvasGroup.alpha < 1f)
            {
                canvasGroup.alpha += labelFadeSpeed * Time.deltaTime;
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOutLabel(GameObject label)
        {
            if (label == null) yield break;

            CanvasGroup canvasGroup = label.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = label.AddComponent<CanvasGroup>();
            }

            while (canvasGroup.alpha > 0f)
            {
                canvasGroup.alpha -= labelFadeSpeed * Time.deltaTime;
                yield return null;
            }

            Destroy(label);
            if (currentLabel == label)
            {
                currentLabel = null;
            }
        }

        private IEnumerator AutoHideLabel()
        {
            yield return new WaitForSeconds(autoHideDelay);
            HidePartLabel();
        }

        private IEnumerator ShowNotificationCoroutine(string message, float duration)
        {
            // Simple notification implementation
            Debug.Log($"Notification: {message}");
            
            // In a full implementation, this would show a notification UI element
            yield return new WaitForSeconds(duration);
        }

        /// <summary>
        /// Gets current UI status information
        /// </summary>
        /// <returns>UI status as string</returns>
        public string GetUIStatus()
        {
            return $"UI Visible: {isUIVisible}, Follow User: {followUser}, " +
                   $"UI Distance: {uiDistance:F1}m, Has Label: {currentLabel != null}, " +
                   $"Educational Panel: {(educationalPanel != null ? "Available" : "Missing")}";
        }

        private void OnDestroy()
        {
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
            }
        }
    }
}