using System;
using UnityEngine;

namespace HeartLabVR.VR
{
    /// <summary>
    /// Handles gaze tracking for anatomical part selection
    /// Optimized for 120Hz tracking with smooth gaze detection
    /// </summary>
    public class GazeTracker : MonoBehaviour
    {
        [Header("Gaze Configuration")]
        [SerializeField] private Camera gazeCamera;
        [SerializeField] private float gazeDistance = 10f;
        [SerializeField] private LayerMask gazeLayers = -1;
        [SerializeField] private float dwellTime = 1f; // Time to dwell before selection
        
        [Header("Performance Settings")]
        [SerializeField] private float trackingRate = 120f;
        [SerializeField] private bool smoothGazeTracking = true;
        [SerializeField] private float smoothingFactor = 0.1f;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject gazePointer;
        [SerializeField] private Material gazeHitMaterial;
        [SerializeField] private Material gazeNormalMaterial;

        public event Action<Transform, Vector3> OnGazeHit;
        public event Action OnGazeLost;
        public event Action<Transform> OnGazeSelect; // After dwell time
        
        public Transform CurrentGazeTarget { get; private set; }
        public Vector3 CurrentGazePoint { get; private set; }
        public bool IsGazing => CurrentGazeTarget != null;
        
        private Transform lastGazeTarget;
        private float gazeStartTime;
        private float lastTrackingTime;
        private Vector3 smoothedGazeDirection;
        private bool isDwelling;

        private void Awake()
        {
            if (gazeCamera == null)
            {
                gazeCamera = Camera.main;
                if (gazeCamera == null)
                {
                    gazeCamera = FindObjectOfType<Camera>();
                }
            }

            if (gazePointer != null)
            {
                gazePointer.SetActive(false);
            }

            smoothedGazeDirection = Vector3.forward;
        }

        private void Update()
        {
            // Limit tracking rate for performance
            if (Time.time - lastTrackingTime < 1f / trackingRate)
                return;

            lastTrackingTime = Time.time;
            
            PerformGazeTracking();
            UpdateDwellSelection();
            UpdateVisualFeedback();
        }

        private void PerformGazeTracking()
        {
            if (gazeCamera == null)
                return;

            Vector3 gazeDirection = gazeCamera.transform.forward;
            
            // Apply smoothing for more stable gaze tracking
            if (smoothGazeTracking)
            {
                smoothedGazeDirection = Vector3.Lerp(smoothedGazeDirection, gazeDirection, smoothingFactor);
                gazeDirection = smoothedGazeDirection.normalized;
            }

            Vector3 gazeOrigin = gazeCamera.transform.position;
            
            if (Physics.Raycast(gazeOrigin, gazeDirection, out RaycastHit hit, gazeDistance, gazeLayers))
            {
                Transform hitTransform = hit.transform;
                Vector3 hitPoint = hit.point;
                
                // Check if this is a new gaze target
                if (CurrentGazeTarget != hitTransform)
                {
                    // Exit previous target
                    if (CurrentGazeTarget != null)
                    {
                        OnGazeLost?.Invoke();
                        isDwelling = false;
                    }
                    
                    // Enter new target
                    CurrentGazeTarget = hitTransform;
                    CurrentGazePoint = hitPoint;
                    gazeStartTime = Time.time;
                    isDwelling = false;
                    
                    OnGazeHit?.Invoke(hitTransform, hitPoint);
                }
                else
                {
                    // Update gaze point for current target
                    CurrentGazePoint = hitPoint;
                }
            }
            else
            {
                // No gaze hit
                if (CurrentGazeTarget != null)
                {
                    CurrentGazeTarget = null;
                    isDwelling = false;
                    OnGazeLost?.Invoke();
                }
            }
        }

        private void UpdateDwellSelection()
        {
            if (CurrentGazeTarget != null && dwellTime > 0)
            {
                float dwellProgress = (Time.time - gazeStartTime) / dwellTime;
                
                if (dwellProgress >= 1f && !isDwelling)
                {
                    isDwelling = true;
                    OnGazeSelect?.Invoke(CurrentGazeTarget);
                }
            }
        }

        private void UpdateVisualFeedback()
        {
            if (gazePointer == null)
                return;

            if (CurrentGazeTarget != null)
            {
                gazePointer.SetActive(true);
                gazePointer.transform.position = CurrentGazePoint;
                gazePointer.transform.LookAt(gazeCamera.transform);
                
                // Update material based on dwell progress
                if (dwellTime > 0)
                {
                    float dwellProgress = Mathf.Clamp01((Time.time - gazeStartTime) / dwellTime);
                    UpdatePointerMaterial(dwellProgress);
                }
            }
            else
            {
                gazePointer.SetActive(false);
            }
        }

        private void UpdatePointerMaterial(float dwellProgress)
        {
            if (gazePointer == null || gazeHitMaterial == null || gazeNormalMaterial == null)
                return;

            var renderer = gazePointer.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (dwellProgress >= 1f)
                {
                    renderer.material = gazeHitMaterial;
                }
                else
                {
                    renderer.material = gazeNormalMaterial;
                    
                    // Optionally animate the material based on dwell progress
                    if (renderer.material.HasProperty("_FillAmount"))
                    {
                        renderer.material.SetFloat("_FillAmount", dwellProgress);
                    }
                }
            }
        }

        /// <summary>
        /// Forces a gaze selection on the current target
        /// </summary>
        public void ForceGazeSelect()
        {
            if (CurrentGazeTarget != null)
            {
                OnGazeSelect?.Invoke(CurrentGazeTarget);
            }
        }

        /// <summary>
        /// Gets the gaze ray for external use
        /// </summary>
        /// <returns>Ray representing current gaze direction</returns>
        public Ray GetGazeRay()
        {
            if (gazeCamera != null)
            {
                Vector3 direction = smoothGazeTracking ? smoothedGazeDirection : gazeCamera.transform.forward;
                return new Ray(gazeCamera.transform.position, direction);
            }
            return new Ray(Vector3.zero, Vector3.forward);
        }

        /// <summary>
        /// Gets dwell progress for current target (0-1)
        /// </summary>
        /// <returns>Dwell progress percentage</returns>
        public float GetDwellProgress()
        {
            if (CurrentGazeTarget != null && dwellTime > 0)
            {
                return Mathf.Clamp01((Time.time - gazeStartTime) / dwellTime);
            }
            return 0f;
        }

        /// <summary>
        /// Sets the gaze tracking distance
        /// </summary>
        /// <param name="distance">New gaze distance</param>
        public void SetGazeDistance(float distance)
        {
            gazeDistance = Mathf.Max(0.1f, distance);
        }

        /// <summary>
        /// Sets the dwell time for selection
        /// </summary>
        /// <param name="time">New dwell time in seconds</param>
        public void SetDwellTime(float time)
        {
            dwellTime = Mathf.Max(0f, time);
        }

        /// <summary>
        /// Enables or disables smooth gaze tracking
        /// </summary>
        /// <param name="enabled">Enable smooth tracking</param>
        public void SetSmoothTracking(bool enabled)
        {
            smoothGazeTracking = enabled;
        }

        /// <summary>
        /// Sets the smoothing factor for gaze tracking
        /// </summary>
        /// <param name="factor">Smoothing factor (0-1)</param>
        public void SetSmoothingFactor(float factor)
        {
            smoothingFactor = Mathf.Clamp01(factor);
        }

        /// <summary>
        /// Gets current gaze tracking statistics
        /// </summary>
        /// <returns>Gaze tracking info as string</returns>
        public string GetGazeInfo()
        {
            string targetName = CurrentGazeTarget != null ? CurrentGazeTarget.name : "None";
            float dwellProgress = GetDwellProgress();
            
            return $"Target: {targetName}, Dwell: {dwellProgress:F2}, " +
                   $"Tracking Rate: {trackingRate}Hz, Smooth: {smoothGazeTracking}";
        }

        /// <summary>
        /// Resets gaze tracking state
        /// </summary>
        public void ResetGazeState()
        {
            if (CurrentGazeTarget != null)
            {
                OnGazeLost?.Invoke();
            }
            
            CurrentGazeTarget = null;
            isDwelling = false;
            gazeStartTime = 0f;
            smoothedGazeDirection = gazeCamera != null ? gazeCamera.transform.forward : Vector3.forward;
            
            if (gazePointer != null)
            {
                gazePointer.SetActive(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (gazeCamera != null)
            {
                Gizmos.color = Color.green;
                Vector3 origin = gazeCamera.transform.position;
                Vector3 direction = smoothGazeTracking ? smoothedGazeDirection : gazeCamera.transform.forward;
                Gizmos.DrawRay(origin, direction * gazeDistance);
                
                if (CurrentGazeTarget != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(CurrentGazePoint, 0.05f);
                }
            }
        }
    }
}