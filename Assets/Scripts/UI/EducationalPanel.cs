using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HeartLabVR.UI
{
    /// <summary>
    /// Manages educational content display in VR
    /// Shows detailed information about heart parts and AI responses
    /// </summary>
    public class EducationalPanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button audioButton;
        [SerializeField] private Slider progressSlider;

        [Header("Animation Settings")]
        [SerializeField] private float animationSpeed = 2f;
        [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Content Settings")]
        [SerializeField] private int maxContentLength = 500;
        [SerializeField] private float typingSpeed = 50f; // Characters per second
        [SerializeField] private bool enableTypingEffect = true;
        [SerializeField] private bool autoScrollText = true;

        [Header("Audio Settings")]
        [SerializeField] private bool enableAudioPlayback = true;
        [SerializeField] private Sprite audioPlayIcon;
        [SerializeField] private Sprite audioStopIcon;

        public event Action<string> OnContentRequested;
        public event Action OnPanelClosed;
        public event Action<string> OnAudioRequested;

        public bool IsVisible { get; private set; }
        public bool IsPlayingAudio { get; private set; }
        public string CurrentContent { get; private set; }

        private CanvasGroup canvasGroup;
        private Coroutine typingCoroutine;
        private Coroutine animationCoroutine;
        private HeartLabVR.VR.TextToSpeech textToSpeech;

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            textToSpeech = FindObjectOfType<HeartLabVR.VR.TextToSpeech>();
            
            if (textToSpeech != null)
            {
                textToSpeech.OnSpeechStarted += OnAudioStarted;
                textToSpeech.OnSpeechCompleted += OnAudioCompleted;
            }
        }

        private void Initialize()
        {
            // Get or create canvas group for animations
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // Setup panel root
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            // Find UI components if not assigned
            if (titleText == null)
            {
                titleText = transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            }

            if (contentText == null)
            {
                contentText = transform.Find("Content")?.GetComponent<TextMeshProUGUI>();
            }

            if (closeButton == null)
            {
                closeButton = GetComponentInChildren<Button>();
            }

            // Setup button events
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HideContent);
            }

            if (audioButton != null)
            {
                audioButton.onClick.AddListener(ToggleAudio);
            }

            // Initially hide the panel
            panelRoot.SetActive(false);
            IsVisible = false;
        }

        /// <summary>
        /// Shows educational content about a heart part
        /// </summary>
        /// <param name="partName">Name of the heart part</param>
        /// <param name="content">Educational content to display</param>
        public void ShowContent(string partName, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                OnContentRequested?.Invoke(partName);
                return;
            }

            CurrentContent = content;
            
            // Set title
            if (titleText != null)
            {
                titleText.text = FormatPartName(partName);
            }

            // Show panel with animation
            if (!IsVisible)
            {
                ShowPanel();
            }

            // Display content with typing effect
            if (enableTypingEffect)
            {
                StartTypingEffect(content);
            }
            else
            {
                if (contentText != null)
                {
                    contentText.text = ProcessContent(content);
                }
            }

            Debug.Log($"Showing educational content for: {partName}");
        }

        /// <summary>
        /// Hides the educational panel
        /// </summary>
        public void HideContent()
        {
            if (IsVisible)
            {
                HidePanel();
                StopTypingEffect();
                StopAudio();
                OnPanelClosed?.Invoke();
            }
        }

        /// <summary>
        /// Updates content without changing visibility
        /// </summary>
        /// <param name="newContent">New content to display</param>
        public void UpdateContent(string newContent)
        {
            if (IsVisible)
            {
                CurrentContent = newContent;
                
                if (enableTypingEffect)
                {
                    StartTypingEffect(newContent);
                }
                else
                {
                    if (contentText != null)
                    {
                        contentText.text = ProcessContent(newContent);
                    }
                }
            }
        }

        /// <summary>
        /// Toggles audio playback of current content
        /// </summary>
        public void ToggleAudio()
        {
            if (!enableAudioPlayback || textToSpeech == null)
                return;

            if (IsPlayingAudio)
            {
                StopAudio();
            }
            else
            {
                PlayAudio();
            }
        }

        /// <summary>
        /// Plays audio for current content
        /// </summary>
        public void PlayAudio()
        {
            if (textToSpeech != null && !string.IsNullOrEmpty(CurrentContent))
            {
                textToSpeech.Speak(CurrentContent);
                OnAudioRequested?.Invoke(CurrentContent);
            }
        }

        /// <summary>
        /// Stops audio playback
        /// </summary>
        public void StopAudio()
        {
            if (textToSpeech != null)
            {
                textToSpeech.StopSpeaking();
            }
        }

        private void ShowPanel()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            panelRoot.SetActive(true);
            IsVisible = true;
            animationCoroutine = StartCoroutine(AnimatePanel(true));
        }

        private void HidePanel()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            IsVisible = false;
            animationCoroutine = StartCoroutine(AnimatePanel(false));
        }

        private IEnumerator AnimatePanel(bool show)
        {
            float startAlpha = canvasGroup.alpha;
            float targetAlpha = show ? 1f : 0f;
            float startScale = panelRoot.transform.localScale.x;
            float targetScale = show ? 1f : 0.8f;
            
            AnimationCurve curve = show ? showCurve : hideCurve;
            float duration = 1f / animationSpeed;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                float curveValue = curve.Evaluate(progress);

                // Animate alpha
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
                
                // Animate scale
                float scale = Mathf.Lerp(startScale, targetScale, curveValue);
                panelRoot.transform.localScale = Vector3.one * scale;

                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            panelRoot.transform.localScale = Vector3.one * targetScale;

            if (!show)
            {
                panelRoot.SetActive(false);
            }

            animationCoroutine = null;
        }

        private void StartTypingEffect(string content)
        {
            StopTypingEffect();
            
            if (contentText != null)
            {
                typingCoroutine = StartCoroutine(TypeText(ProcessContent(content)));
            }
        }

        private void StopTypingEffect()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
        }

        private IEnumerator TypeText(string text)
        {
            if (contentText == null) yield break;

            contentText.text = "";
            float delay = 1f / typingSpeed;

            for (int i = 0; i <= text.Length; i++)
            {
                contentText.text = text.Substring(0, i);
                
                // Auto-scroll if enabled
                if (autoScrollText && contentText.textInfo.lineCount > 5)
                {
                    // Implement auto-scrolling logic here
                }

                yield return new WaitForSeconds(delay);
            }

            typingCoroutine = null;
        }

        private string ProcessContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            // Limit content length
            if (content.Length > maxContentLength)
            {
                content = content.Substring(0, maxContentLength) + "...";
            }

            // Process markdown-like formatting
            content = content.Replace("**", "<b>").Replace("**", "</b>");
            content = content.Replace("*", "<i>").Replace("*", "</i>");

            return content;
        }

        private string FormatPartName(string partName)
        {
            if (string.IsNullOrEmpty(partName))
                return "";

            // Convert from internal name format to display format
            string formatted = partName.Replace("_", " ");
            formatted = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formatted);
            
            // Turkish specific formatting
            formatted = formatted.Replace("Sol ", "Sol ");
            formatted = formatted.Replace("Sag ", "Sağ ");
            formatted = formatted.Replace("Ventrikul", "Ventrikül");
            formatted = formatted.Replace("Kapak", "Kapağı");

            return formatted;
        }

        private void OnAudioStarted()
        {
            IsPlayingAudio = true;
            UpdateAudioButton();
            
            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(true);
                StartCoroutine(UpdateAudioProgress());
            }
        }

        private void OnAudioCompleted()
        {
            IsPlayingAudio = false;
            UpdateAudioButton();
            
            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(false);
                progressSlider.value = 0f;
            }
        }

        private void UpdateAudioButton()
        {
            if (audioButton != null)
            {
                var buttonImage = audioButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = IsPlayingAudio ? audioStopIcon : audioPlayIcon;
                }

                // Update button text if using TextMeshPro
                var buttonText = audioButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = IsPlayingAudio ? "Durdur" : "Dinle";
                }
            }
        }

        private IEnumerator UpdateAudioProgress()
        {
            if (progressSlider == null || textToSpeech == null)
                yield break;

            while (IsPlayingAudio)
            {
                // This would need to be implemented based on the actual TTS progress
                // For now, we'll show a simple animation
                float time = Time.time * 0.5f;
                progressSlider.value = Mathf.PingPong(time, 1f);
                yield return null;
            }

            progressSlider.value = 0f;
        }

        /// <summary>
        /// Gets current panel status
        /// </summary>
        /// <returns>Panel status information</returns>
        public string GetPanelStatus()
        {
            return $"Visible: {IsVisible}, Playing Audio: {IsPlayingAudio}, " +
                   $"Has Content: {!string.IsNullOrEmpty(CurrentContent)}, " +
                   $"Typing Effect: {enableTypingEffect}, Content Length: {CurrentContent?.Length ?? 0}";
        }

        /// <summary>
        /// Sets typing speed for text animation
        /// </summary>
        /// <param name="speed">Characters per second</param>
        public void SetTypingSpeed(float speed)
        {
            typingSpeed = Mathf.Clamp(speed, 10f, 200f);
        }

        /// <summary>
        /// Enables or disables typing effect
        /// </summary>
        /// <param name="enabled">Enable typing effect</param>
        public void SetTypingEffectEnabled(bool enabled)
        {
            enableTypingEffect = enabled;
        }

        /// <summary>
        /// Sets maximum content length
        /// </summary>
        /// <param name="length">Maximum characters</param>
        public void SetMaxContentLength(int length)
        {
            maxContentLength = Mathf.Clamp(length, 100, 2000);
        }

        private void OnDestroy()
        {
            if (textToSpeech != null)
            {
                textToSpeech.OnSpeechStarted -= OnAudioStarted;
                textToSpeech.OnSpeechCompleted -= OnAudioCompleted;
            }

            StopTypingEffect();
            
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
        }
    }
}