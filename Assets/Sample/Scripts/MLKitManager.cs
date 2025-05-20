using UnityEngine;

public class MLKitManager : MonoBehaviour
{
    public static MLKitManager Instance { get; private set; }
    
    public delegate void FaceDetectionResult(string result);
    public event FaceDetectionResult OnFaceDetectionComplete;
    
    public delegate void CameraInitializedResult(string result);
    public event CameraInitializedResult OnCameraInitializedComplete;
    
    // Reference texture to display the camera in Unity
    private Texture2D displayTexture;
    public Texture2D DisplayTexture => displayTexture;
    
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
#endif
    }
    
    public void StartCamera()
    {
        Debug.Log("MLKitManager.StartCamera called");
    
#if UNITY_ANDROID && !UNITY_EDITOR
        try {
            Debug.Log("Attempting to call Android startCamera");
            AndroidJavaClass bridgeClass = new AndroidJavaClass("com.medrick.mlkit.UnityMLKitBridge");
            Debug.Log("Got UnityMLKitBridge class, calling startCamera");
            bridgeClass.CallStatic("startCamera");
            Debug.Log("Android startCamera method called");
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
        Debug.Log("Face Detection Result: " + result);
        OnFaceDetectionComplete?.Invoke(result);
    }
    
    void OnDestroy()
    {
        StopCamera();
    }
}