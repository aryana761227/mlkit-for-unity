using UnityEngine;

public class MLKitManager : MonoBehaviour
{
    public static MLKitManager Instance { get; private set; }
    
    public delegate void FaceDetectionResult(string result);
    public event FaceDetectionResult OnFaceDetectionComplete;
    
    private WebCamTexture webCamTexture;
    private Color32[] pixels;
    private byte[] imageBytes;
    
    public WebCamTexture WebCamTexture => webCamTexture;
    
    void Awake()
    {
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
        InitializeMLKit();
    }
    
    void InitializeMLKit()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
            AndroidJavaClass bridgeClass = new AndroidJavaClass("com.medrick.mlkit.UnityMLKitBridge");
            bridgeClass.CallStatic("initialize");
        #else
            Debug.Log("ML Kit only works on Android devices");
        #endif
    }
    
    public void StartCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        
        if (devices.Length > 0)
        {
            string deviceName = devices[0].name; // Default to first camera
            WebCamDevice selectedDevice = devices[0];
        
            // Find front-facing camera
            for (int i = 0; i < devices.Length; i++)
            {
                var webCamDevice = devices[i];
                if (webCamDevice.isFrontFacing)
                {
                    selectedDevice = webCamDevice;
                    deviceName = webCamDevice.name;
                    Debug.Log("Found front camera: " + deviceName);
                    break;
                }
            }

            var selectedDeviceAvailableResolution = selectedDevice.availableResolutions[0];
            int requestedWidth = selectedDeviceAvailableResolution.width;
            int requestedHeight = selectedDeviceAvailableResolution.height;
            
            webCamTexture = new WebCamTexture(deviceName, requestedWidth, requestedHeight);
            webCamTexture.Play();
        }
        else
        {
            Debug.LogError("No camera devices found!");
        }
    }
    
    public void DetectFaces()
    {
        if (webCamTexture == null || !webCamTexture.isPlaying)
        {
            Debug.LogWarning("Camera not initialized");
            return;
        }
        
        #if UNITY_ANDROID && !UNITY_EDITOR
            pixels = webCamTexture.GetPixels32();
            imageBytes = EncodeToYUV420(pixels, webCamTexture.width, webCamTexture.height);
            
            AndroidJavaClass bridgeClass = new AndroidJavaClass("com.medrick.mlkit.UnityMLKitBridge");
            bridgeClass.CallStatic("detectFaces", imageBytes, webCamTexture.width, webCamTexture.height);
        #else
            Debug.Log("Face detection only works on Android devices");
        #endif
    }
    
    public void OnInitialized(string result)
    {
        Debug.Log("ML Kit Initialized: " + result);
    }
    
    public void OnFaceDetectionResult(string result)
    {
        Debug.Log("Face Detection Result: " + result);
        OnFaceDetectionComplete?.Invoke(result);
    }
    
    private byte[] EncodeToYUV420(Color32[] colors, int width, int height)
    {
        byte[] yuv = new byte[width * height * 3 / 2];
        int frameSize = width * height;
        
        for (int i = 0; i < colors.Length; i++)
        {
            int r = colors[i].r;
            int g = colors[i].g;
            int b = colors[i].b;
            
            int y = ((66 * r + 129 * g + 25 * b + 128) >> 8) + 16;
            int u = ((-38 * r - 74 * g + 112 * b + 128) >> 8) + 128;
            int v = ((112 * r - 94 * g - 18 * b + 128) >> 8) + 128;
            
            yuv[i] = (byte)Mathf.Clamp(y, 0, 255);
            
            if (i % 2 == 0)
            {
                int uvIndex = frameSize + (i / 2);
                if (uvIndex < yuv.Length - 1)
                {
                    yuv[uvIndex] = (byte)Mathf.Clamp(u, 0, 255);
                    yuv[uvIndex + 1] = (byte)Mathf.Clamp(v, 0, 255);
                }
            }
        }
        
        return yuv;
    }
}