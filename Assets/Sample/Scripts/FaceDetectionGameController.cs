using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class FaceDetectionGameController : MonoBehaviour
{
    [FormerlySerializedAs("cameraDisplay")] [Header("UI Elements")]
    public RawImage CameraDisplay;
    [FormerlySerializedAs("debugText")] public TextMeshProUGUI DebugText;
    [FormerlySerializedAs("faceCountText")] public TextMeshProUGUI FaceCountText;
    
    [FormerlySerializedAs("detectionInterval")] [Header("Detection Settings")]
    public float DetectionInterval = 0.2f;
    
    private MLKitManager mlkitManager;
    private bool isDetecting = false;
    private List<GameObject> faceIndicators = new List<GameObject>();
    
    void Start()
    {
        mlkitManager = MLKitManager.Instance;
        if (mlkitManager == null)
        {
            GameObject mlkitGO = new GameObject("MLKitManager");
            mlkitManager = mlkitGO.AddComponent<MLKitManager>();
        }
        
        mlkitManager.OnFaceDetectionComplete += HandleFaceDetectionResult;
        StartCamera();
    }
    
    void StartCamera()
    {
        mlkitManager.StartCamera();
        
        if (CameraDisplay != null)
        {
            CameraDisplay.texture = mlkitManager.WebCamTexture;
            
            if (mlkitManager.WebCamTexture != null)
            {
                StartCoroutine(AdjustCameraDisplay());
            }
        }
        
        StartCoroutine(DetectFacesPeriodically());
    }
    
    IEnumerator AdjustCameraDisplay()
    {
        while (mlkitManager.WebCamTexture.width == 16)
        {
            yield return null;
        }
        
        float cameraAspect = (float)mlkitManager.WebCamTexture.width / (float)mlkitManager.WebCamTexture.height;
        float screenAspect = (float)Screen.width / (float)Screen.height;
        
        RectTransform rectTransform = CameraDisplay.GetComponent<RectTransform>();
        
        if (cameraAspect > screenAspect)
        {
            rectTransform.sizeDelta = new Vector2(Screen.width, Screen.width / cameraAspect);
        }
        else
        {
            rectTransform.sizeDelta = new Vector2(Screen.height * cameraAspect, Screen.height);
        }
        
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    void Update()
    {
        if (mlkitManager.WebCamTexture != null && mlkitManager.WebCamTexture.isPlaying)
        {
            int rotation = mlkitManager.WebCamTexture.videoRotationAngle;
            CameraDisplay.transform.localEulerAngles = new Vector3(0, 0, -rotation);
        }
    }
    
    IEnumerator DetectFacesPeriodically()
    {
        while (true)
        {
            if (!isDetecting)
            {
                isDetecting = true;
                mlkitManager.DetectFaces();
            }
            
            yield return new WaitForSeconds(DetectionInterval);
            isDetecting = false;
        }
    }
    
    void HandleFaceDetectionResult(string result)
    {
        FaceDetectionData data = FaceDetectionData.ParseResult(result);
        
        if (DebugText != null)
        {
            DebugText.text = $"Face Detection: {result}";
        }
        
        if (FaceCountText != null)
        {
            FaceCountText.text = $"Faces detected: {data.faces.Count}";
        }
        
        DrawFaceRectangles(data);
        
        foreach (var face in data.faces)
        {
            if (face.smileProbability > 0.7f)
            {
                Debug.Log("Person is smiling!");
            }
        }
    }
    
    private void DrawFaceRectangles(FaceDetectionData data)
    {
        // Clear existing indicators
        foreach (GameObject indicator in faceIndicators)
        {
            Destroy(indicator);
        }
        faceIndicators.Clear();
        
        if (mlkitManager.WebCamTexture == null || !mlkitManager.WebCamTexture.isPlaying)
            return;
        
        // Get camera dimensions
        float camWidth = mlkitManager.WebCamTexture.width;
        float camHeight = mlkitManager.WebCamTexture.height;
        
        // Get display dimensions
        RectTransform displayRect = CameraDisplay.GetComponent<RectTransform>();
        float displayWidth = displayRect.rect.width;
        float displayHeight = displayRect.rect.height;
        
        // Calculate scale factors
        float scaleX = displayWidth / camWidth;
        float scaleY = displayHeight / camHeight;
        
        foreach (var face in data.faces)
        {
            // Create rectangle GameObject
            GameObject rectangle = new GameObject("FaceIndicator");
            rectangle.transform.SetParent(CameraDisplay.transform);
            
            // Add Image component for the rectangle
            UnityEngine.UI.Image image = rectangle.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 1, 0, 0.3f); // Semi-transparent green
            
            // Set rectangle position and size
            RectTransform rectTransform = rectangle.GetComponent<RectTransform>();
            
            // Convert face bounds to screen coordinates
            float x = face.bounds.x * scaleX;
            float y = face.bounds.y * scaleY;
            float width = face.bounds.width * scaleX;
            float height = face.bounds.height * scaleY;
            
            // Apply to rectangle
            rectTransform.anchorMin = new Vector2(0, 1); // Top-left anchor
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            
            // Position correctly (convert from ML Kit coordinates)
            rectTransform.anchoredPosition = new Vector2(x, -y); // Negative Y for UI coordinates
            rectTransform.sizeDelta = new Vector2(width, height);
            
            // Add visual feedback for smile detection
            if (face.smileProbability > 0.7f)
            {
                GameObject smileIndicator = new GameObject("SmileIndicator");
                smileIndicator.transform.SetParent(rectangle.transform);
                
                TextMeshProUGUI smileText = smileIndicator.AddComponent<TextMeshProUGUI>();
                smileText.text = "😊"; // Or use "SMILE!"
                smileText.fontSize = 24;
                smileText.color = Color.green;
                smileText.alignment = TextAlignmentOptions.Center;
                
                RectTransform smileTransform = smileIndicator.GetComponent<RectTransform>();
                smileTransform.anchorMin = Vector2.zero;
                smileTransform.anchorMax = Vector2.one;
                smileTransform.offsetMin = Vector2.zero;
                smileTransform.offsetMax = Vector2.zero;
            }
            
            faceIndicators.Add(rectangle);
        }
    }
    
    void OnDestroy()
    {
        if (mlkitManager != null)
        {
            mlkitManager.OnFaceDetectionComplete -= HandleFaceDetectionResult;
        }
    }
}