using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages face detection logic and state without direct UI dependencies
/// </summary>
public class FaceDetectionGameController : MonoBehaviour
{
    [Header("Detection Settings")]
    public float DetectionInterval = 0.2f;
    
    [Header("Logging")]
    public bool EnableStateLogging = true;
    public float LoggingInterval = 1.0f;
    
    // Events for UI to subscribe to
    public delegate void FaceDetectionResultEvent(FaceDetectionData data, string rawResult);
    public event FaceDetectionResultEvent OnFaceDetectionResult;
    
    public delegate void CameraInitializedEvent(bool success, string message);
    public event CameraInitializedEvent OnCameraInitialized;
    
    public delegate void StateLogUpdatedEvent(DeviceState state);
    public event StateLogUpdatedEvent OnStateLogUpdated;
    
    // Public properties
    public bool IsCameraInitialized => isCameraInitialized;
    public string CurrentCameraType => currentCameraType;
    public FaceDetectionData LastDetectionData => lastDetectionData;
    public DeviceState CurrentState => deviceState;
    public Texture CameraTexture => mlkitManager?.DisplayTexture;
    
    private MLKitManager mlkitManager;
    private bool isCameraInitialized = false;
    private string currentCameraType = "Front";
    private FaceDetectionData lastDetectionData;
    
    // Device state tracking
    private DeviceState deviceState = new DeviceState();
    
    void Start()
    {
        // Get or create MLKitManager instance
        mlkitManager = MLKitManager.Instance;
        if (mlkitManager == null)
        {
            GameObject mlkitGO = new GameObject("MLKitManager");
            mlkitManager = mlkitGO.AddComponent<MLKitManager>();
        }
        
        // Subscribe to MLKit events
        mlkitManager.OnFaceDetectionComplete += HandleFaceDetectionResult;
        mlkitManager.OnCameraInitializedComplete += HandleCameraInitialized;
        
        // Start camera
        StartCamera();
        
        // Start logging if enabled
        if (EnableStateLogging)
        {
            StartCoroutine(LogStateRoutine());
        }
    }
    
    void StartCamera()
    {
        mlkitManager.StartCamera();
        
        // Set detection interval (frames to skip)
        // Higher values = better performance but less smooth detection
        int interval = Mathf.RoundToInt(DetectionInterval * 30); // Assuming 30fps
        mlkitManager.SetDetectionInterval(Mathf.Max(1, interval));
    }
    
    void HandleCameraInitialized(string result)
    {
        isCameraInitialized = result.Contains("SUCCESS");
        
        if (isCameraInitialized)
        {
            Debug.Log("Camera initialized successfully");
            deviceState.cameraStatus = "Initialized";
        }
        else
        {
            Debug.LogError("Failed to initialize camera: " + result);
            deviceState.cameraStatus = "Failed: " + result;
        }
        
        // Notify UI through event
        OnCameraInitialized?.Invoke(isCameraInitialized, result);
    }
    
    public void SwitchCamera()
    {
        if (isCameraInitialized)
        {
            mlkitManager.SwitchCamera();
            
            // Toggle camera type for logging
            currentCameraType = (currentCameraType == "Front") ? "Back" : "Front";
            deviceState.cameraType = currentCameraType;
            
            Debug.Log("Switched to " + currentCameraType + " camera");
        }
        else
        {
            Debug.LogWarning("Cannot switch camera - not initialized yet");
        }
    }
    
    void HandleFaceDetectionResult(string result)
    {
        lastDetectionData = FaceDetectionData.ParseResult(result);
    
        // Update device state for logging
        deviceState.faceCount = lastDetectionData.faces.Count;
        if (lastDetectionData.faces.Count > 0)
        {
            deviceState.lastFaceDetectionTime = Time.time;
            deviceState.hasSmile = lastDetectionData.faces.Exists(face => face.smileProbability > 0.7f);
        
            // Update upsetness
            var mostUpsetFace = lastDetectionData.faces.OrderByDescending(face => face.upsetnessProbability).FirstOrDefault();
            if (mostUpsetFace != null)
            {
                deviceState.upsetLevel = mostUpsetFace.upsetnessProbability;
                deviceState.isUpset = mostUpsetFace.upsetnessProbability > 0.6f; // Threshold for "upset"
            }
        }
    
        // Notify UI through event
        OnFaceDetectionResult?.Invoke(lastDetectionData, result);
    }
    
    // State logging coroutine - runs every LoggingInterval seconds
    private IEnumerator LogStateRoutine()
    {
        while (true)
        {
            // Wait for the specified interval
            yield return new WaitForSeconds(LoggingInterval);
            
            // Update additional state information
            deviceState.fps = (int)(1.0f / Time.deltaTime);
            deviceState.memoryUsage = System.GC.GetTotalMemory(false) / (1024 * 1024); // MB
            deviceState.timeSinceLastFace = Time.time - deviceState.lastFaceDetectionTime;
            deviceState.timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            
            // Log state to console
            string stateLog = $"[STATE LOG] {deviceState.timestamp}\n" +
                              $"Camera: {deviceState.cameraType} ({deviceState.cameraStatus})\n" +
                              $"FPS: {deviceState.fps}\n" +
                              $"Memory: {deviceState.memoryUsage}MB\n" +
                              $"Faces: {deviceState.faceCount}\n" +
                              $"Smile Detected: {deviceState.hasSmile}\n" +
                              $"Time Since Face: {deviceState.timeSinceLastFace:F1}s";
            
            Debug.Log(stateLog);
            
            // Notify UI through event
            OnStateLogUpdated?.Invoke(deviceState);
        }
    }
    
    void OnDestroy()
    {
        if (mlkitManager != null)
        {
            mlkitManager.OnFaceDetectionComplete -= HandleFaceDetectionResult;
            mlkitManager.OnCameraInitializedComplete -= HandleCameraInitialized;
        }
    }
    
    // Class to track device and detection state
    [System.Serializable]
    public class DeviceState
    {
        public string timestamp = "";
        public string cameraType = "Front";
        public string cameraStatus = "Initializing";
        public int faceCount = 0;
        public bool hasSmile = false;
        public bool isUpset = false;  // New property
        public float upsetLevel = 0f; // New property
        public float lastFaceDetectionTime = 0f;
        public float timeSinceLastFace = 0f;
        public int fps = 0;
        public long memoryUsage = 0; // in MB
    }
}