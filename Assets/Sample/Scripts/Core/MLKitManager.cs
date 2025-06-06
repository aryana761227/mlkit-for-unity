using UnityEngine;

public class MLKitManager : MonoBehaviour
{
    public static MLKitManager Instance { get; private set; }
    
    public delegate void FaceDetectionResult(string result);
    public event FaceDetectionResult OnFaceDetectionComplete;
    public delegate void FaceMeshPointsDetectionResult(string result);
    public event FaceMeshPointsDetectionResult OnFaceMeshPointsDetectionComplete;
    public delegate void FaceMeshTrianglesDetectionResult(string result);
    public event FaceMeshTrianglesDetectionResult OnFaceMeshTrianglesDetectionComplete;
    
    public delegate void CameraInitializedResult(string result);
    public event CameraInitializedResult OnCameraInitializedComplete;
    
    // Reference texture to display the camera in Unity
    private Texture2D displayTexture;
    public Texture2D DisplayTexture => displayTexture;
    
    // Face detection configuration
    [Header("Detection Settings")]
    [Tooltip("Whether to detect facial landmarks (eyes, nose, ears, etc.)")]
    public bool enableLandmarks = true;
    
    [Tooltip("Whether to detect facial contours (face outline, lips, eyebrows, etc.)")]
    public bool enableContours = true;
    
    // Permission handling
    private bool isInitialized = false;
    private bool pendingCameraStart = false;
    private bool faceMeshTrianglesDetector = false;
    private bool lightWeightFaceDetector;
    private bool faceMeshPointsDetector;

    void Awake()
    {
        gameObject.name = "MLKitManager";
        Application.targetFrameRate = 60;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        Debug.Log("MLKitManager Start called");
        InitializeMLKit();
    }

    void InitializeMLKit()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try {
            Debug.Log("Attempting to initialize ML Kit on Android");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
            Debug.Log("Looking for UnityMLKitBridge class");
            AndroidJavaClass bridgeClass = new AndroidJavaClass("com.medrick.mlkit.UnityMLKitBridge");
            Debug.Log("Found UnityMLKitBridge class, calling initialize");
            bridgeClass.CallStatic("initialize");
            Debug.Log("ML Kit initialization called");
        }
        catch (System.Exception e) {
            Debug.LogError("Failed to initialize ML Kit: " + e.Message + "\n" + e.StackTrace);
        }
#else
        Debug.Log("ML Kit only works on Android devices");
        // For testing in editor
        OnInitialized("SUCCESS");
#endif
    }
    
    public void StartCamera()
    {
        Debug.Log("MLKitManager.StartCamera called");
        if (!isInitialized)
        {
            Debug.Log("ML Kit not initialized yet, marking camera start as pending");
            pendingCameraStart = true;
            return;
        }
    
#if UNITY_ANDROID && !UNITY_EDITOR
        try {
            Debug.Log("Attempting to call Android startCamera");
            AndroidJavaClass bridgeClass = new AndroidJavaClass("com.medrick.mlkit.UnityMLKitBridge");
            Debug.Log("Got UnityMLKitBridge class, calling startCamera");
            bridgeClass.CallStatic("startCamera");
            Debug.Log("Android startCamera method called");
            // Configure detection features
            ConfigureFaceDetection(enableLandmarks, enableContours);
        }
        catch (System.Exception e) {
            Debug.LogError("Failed to call startCamera: " + e.Message + "\n" + e.StackTrace);
        }
#else
        Debug.Log("Camera only starts on Android devices");
        // For testing in editor
        OnCameraInitialized("SUCCESS");
#endif
    }

    public void EnableLightWeightFaceDetector(bool enable)
    {
        this.lightWeightFaceDetector = enable;
        ApplyFaceDetectionLevelsToAndroid();
    }
    public void EnableFaceMeshPointsDetector(bool enable)
    {
        this.faceMeshPointsDetector = enable;
        ApplyFaceDetectionLevelsToAndroid();
    }
    public void EnableFaceMeshTrianglesDetector(bool enable)
    {
        this.faceMeshTrianglesDetector = enable;
        ApplyFaceDetectionLevelsToAndroid();
    }

    private void ApplyFaceDetectionLevelsToAndroid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try {
            AndroidJavaClass bridgeClass = new AndroidJavaClass("com.medrick.mlkit.UnityMLKitBridge");
            bridgeClass.CallStatic("setFaceActivations", lightWeightFaceDetector, faceMeshPointsDetector, faceMeshTrianglesDetector);
            Debug.Log($"Configured face detection - lightWeightFaceDetector: {lightWeightFaceDetector}, faceMeshPointsDetector: {faceMeshPointsDetector}, faceMeshTrianglesDetector : {faceMeshTrianglesDetector}");
        }
        catch (System.Exception e) {
            Debug.LogError("Failed to face detection levels: " + e.Message + "\n" + e.StackTrace);
        }
#else
        Debug.Log($"Face detection level - lightWeightFaceDetector: {lightWeightFaceDetector}, faceMeshPointsDetector: {faceMeshPointsDetector}, faceMeshTrianglesDetector : {faceMeshTrianglesDetector} only works on Android devices");
#endif
    }

    // New method to configure face detection options
    public void ConfigureFaceDetection(bool enableLandmarks, bool enableContours)
    {
        this.enableLandmarks = enableLandmarks;
        this.enableContours = enableContours;
        
#if UNITY_ANDROID && !UNITY_EDITOR
        try {
            AndroidJavaClass bridgeClass = new AndroidJavaClass("com.medrick.mlkit.UnityMLKitBridge");
            bridgeClass.CallStatic("configureFaceDetection", enableLandmarks, enableContours);
            Debug.Log($"Configured face detection - landmarks: {enableLandmarks}, contours: {enableContours}");
        }
        catch (System.Exception e) {
            Debug.LogError("Failed to configure face detection: " + e.Message + "\n" + e.StackTrace);
        }
#else
        Debug.Log($"Face detection configuration (landmarks: {enableLandmarks}, contours: {enableContours}) only works on Android devices");
#endif
    }
    
    public void SwitchCamera()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass bridgeClass = new AndroidJavaClass("com.medrick.mlkit.UnityMLKitBridge");
            bridgeClass.CallStatic("switchCamera");
        #else
            Debug.Log("Camera switching only works on Android devices");
        #endif
    }
    
    public void SetDetectionInterval(int interval)
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass bridgeClass = new AndroidJavaClass("com.medrick.mlkit.UnityMLKitBridge");
            bridgeClass.CallStatic("setDetectionInterval", interval);
        #else
            Debug.Log("Setting detection interval only works on Android devices");
        #endif
    }
    
    public void StopCamera()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass bridgeClass = new AndroidJavaClass("com.medrick.mlkit.UnityMLKitBridge");
            bridgeClass.CallStatic("stopCamera");
        #else
            Debug.Log("Camera stopping only works on Android devices");
        #endif
    }
    
    // Called from Android when initialization is complete
    public void OnInitialized(string result)
    {
        Debug.Log("ML Kit Initialized: " + result);
        isInitialized = result.Contains("SUCCESS");
        
        // If we had a pending camera start, do it now
        if (isInitialized && pendingCameraStart)
        {
            pendingCameraStart = false;
            Debug.Log("Starting camera after initialization completed");
            StartCamera();
        }
    }
    
    // Called from Android when camera is initialized
    public void OnCameraInitialized(string result)
    {
        Debug.Log("Camera Initialized: " + result);
        OnCameraInitializedComplete?.Invoke(result);
    }
    
    // Called from Android when face detection is complete
    public void OnFaceDetectionResult(string result)
    {
        Debug.Log("LightWeight Face Detection Result: " + result);
        OnFaceDetectionComplete?.Invoke(result);
    }
    public void OnFaceMeshPointsDetectionResult(string result)
    {
        Debug.Log("Face Mesh Points Detection Result: " + result);
        OnFaceMeshPointsDetectionComplete?.Invoke(result);
    }
    public void OnFaceMeshTrianglesDetectionResult(string result)
    {
        Debug.Log("Face Triangles Points Detection Result: " + result);
        OnFaceMeshTrianglesDetectionComplete?.Invoke(result);
    }
    // Handle permission results from Android
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Debug.Log("Application gained focus, checking if camera should be restarted");
            // You might want to restart camera here if needed
        }
    }
    
    void OnDestroy()
    {
        StopCamera();
    }
}