using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Handles all UI operations for face detection, separating UI concerns from game logic
/// </summary>
public class FaceDetectionUIController : MonoBehaviour
{
    [Header("References")] public FaceDetectionGameController gameController;

    [Header("UI Elements")] public RawImage cameraDisplay;
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI faceCountText;
    public TextMeshProUGUI stateLogText;
    public Button switchCameraButton;

    [Header("Face Indicator Settings")] public Color faceRectangleColor = new Color(0, 1, 0, 0.3f);
    public Color smileIndicatorColor = Color.green;
    public float smileIndicatorFontSize = 24f;

    private List<GameObject> faceIndicators = new List<GameObject>();

    void Start()
    {
        if (gameController == null)
        {
            gameController = FindObjectOfType<FaceDetectionGameController>();
            if (gameController == null)
            {
                Debug.LogError("No FaceDetectionGameController found! Please assign one in the inspector.");
                enabled = false;
                return;
            }
        }

        // Subscribe to game controller events
        gameController.OnFaceDetectionResult += HandleFaceDetectionResult;
        gameController.OnCameraInitialized += HandleCameraInitialized;
        gameController.OnStateLogUpdated += HandleStateLogUpdated;

        // Set up button click event
        if (switchCameraButton != null)
        {
            switchCameraButton.onClick.AddListener(gameController.SwitchCamera);
        }

        // Set initial camera display if available
        UpdateCameraDisplay();
    }

    void Update()
    {
        // Continuously update camera display (if needed)
        UpdateCameraDisplay();
    }

    private void UpdateCameraDisplay()
    {
        if (cameraDisplay != null && gameController?.CameraTexture != null)
        {
            cameraDisplay.texture = gameController.CameraTexture;
        }
    }

    private void HandleCameraInitialized(bool success, string message)
    {
        if (debugText != null)
        {
            debugText.text = success ? "Camera initialized successfully" : $"Camera initialization failed: {message}";
        }

        // Update camera display
        UpdateCameraDisplay();
    }

    private void HandleFaceDetectionResult(FaceDetectionData data, string rawResult)
    {
        // Update debug text
        if (debugText != null)
        {
            debugText.text = $"Face Detection: {rawResult}";
        }

        // Update face count text
        if (faceCountText != null)
        {
            faceCountText.text = $"Faces detected: {data.faces.Count}";
        }

        // Draw face rectangles
        DrawFaceRectangles(data);
    }

    private void HandleStateLogUpdated(FaceDetectionGameController.DeviceState state)
    {
        if (stateLogText != null)
        {
            stateLogText.text = $"[STATE LOG] {state.timestamp}\n" +
                                $"Camera: {state.cameraType} ({state.cameraStatus})\n" +
                                $"FPS: {state.fps}\n" +
                                $"Memory: {state.memoryUsage}MB\n" +
                                $"Faces: {state.faceCount}\n" +
                                $"Smile Detected: {state.hasSmile}\n" +
                                $"Upset Level: {state.upsetLevel:F2}\n" +
                                $"Is Upset: {state.isUpset}\n" +
                                $"Time Since Face: {state.timeSinceLastFace:F1}s";
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

        // Get display dimensions
        if (cameraDisplay == null) return;

        RectTransform displayRect = cameraDisplay.GetComponent<RectTransform>();
        float displayWidth = displayRect.rect.width;
        float displayHeight = displayRect.rect.height;

        foreach (var face in data.faces)
        {
            // Create rectangle GameObject
            GameObject rectangle = new GameObject("FaceIndicator");
            rectangle.transform.SetParent(cameraDisplay.transform);

            // Add Image component for the rectangle
            UnityEngine.UI.Image image = rectangle.AddComponent<UnityEngine.UI.Image>();

            // Change color based on upsetness level
            if (face.upsetnessProbability > 0.6f)
            {
                // Red for upset
                image.color = new Color(1, 0, 0, 0.3f);
            }
            else if (face.smileProbability > 0.7f)
            {
                // Green for smile
                image.color = new Color(0, 1, 0, 0.3f);
            }
            else
            {
                // Default color
                image.color = new Color(0, 0.7f, 1, 0.3f);
            }

            // Set rectangle position and size
            RectTransform rectTransform = rectangle.GetComponent<RectTransform>();

            // Get normalized face bounds
            float x = face.bounds.x;
            float y = face.bounds.y;
            float width = face.bounds.width;
            float height = face.bounds.height;

            // Apply to rectangle
            rectTransform.anchorMin = new Vector2(0, 1); // Top-left anchor
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);

            // Position correctly
            rectTransform.anchoredPosition = new Vector2(x * displayWidth, -y * displayHeight);
            rectTransform.sizeDelta = new Vector2(width * displayWidth, height * displayHeight);

            // Add visual feedback based on face state
            if (face.upsetnessProbability > 0.6f)
            {
                // Add upset indicator
                GameObject upsetIndicator = new GameObject("UpsetIndicator");
                upsetIndicator.transform.SetParent(rectangle.transform);

                TextMeshProUGUI upsetText = upsetIndicator.AddComponent<TextMeshProUGUI>();
                upsetText.text = "😠"; // Upset emoji
                upsetText.fontSize = 24;
                upsetText.color = Color.red;
                upsetText.alignment = TextAlignmentOptions.Center;

                RectTransform upsetTransform = upsetIndicator.GetComponent<RectTransform>();
                upsetTransform.anchorMin = Vector2.zero;
                upsetTransform.anchorMax = Vector2.one;
                upsetTransform.offsetMin = Vector2.zero;
                upsetTransform.offsetMax = Vector2.zero;

                faceIndicators.Add(upsetIndicator);
            }
            else if (face.smileProbability > 0.7f)
            {
                // Add smile indicator
                GameObject smileIndicator = new GameObject("SmileIndicator");
                smileIndicator.transform.SetParent(rectangle.transform);

                TextMeshProUGUI smileText = smileIndicator.AddComponent<TextMeshProUGUI>();
                smileText.text = "😊"; // Smile emoji
                smileText.fontSize = 24;
                smileText.color = Color.green;
                smileText.alignment = TextAlignmentOptions.Center;

                RectTransform smileTransform = smileIndicator.GetComponent<RectTransform>();
                smileTransform.anchorMin = Vector2.zero;
                smileTransform.anchorMax = Vector2.one;
                smileTransform.offsetMin = Vector2.zero;
                smileTransform.offsetMax = Vector2.zero;

                faceIndicators.Add(smileIndicator);
            }

            faceIndicators.Add(rectangle);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (gameController != null)
        {
            gameController.OnFaceDetectionResult -= HandleFaceDetectionResult;
            gameController.OnCameraInitialized -= HandleCameraInitialized;
            gameController.OnStateLogUpdated -= HandleStateLogUpdated;
        }
    }
}