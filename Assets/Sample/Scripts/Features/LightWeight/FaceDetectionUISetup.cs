using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates and sets up all UI elements for face detection
/// </summary>
public class FaceDetectionUISetup : MonoBehaviour
{
    // References to controllers
    public FaceDetectionGameController gameController;
    public FaceDetectionUIController uiController;
    
    // UI prefabs (can be left null to use default creation)
    public RawImage cameraPrefab;
    public Button switchCameraPrefab;
    public TMPro.TextMeshProUGUI debugTextPrefab;
    public TMPro.TextMeshProUGUI faceCountPrefab;
    public TMPro.TextMeshProUGUI stateLogPrefab;
    
    // UI Colors
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f);
    public Color buttonColor = new Color(0.2f, 0.6f, 0.8f);
    public Color textColor = Color.white;
    
    private Canvas canvas;
    private RectTransform canvasRect;
    
    void Awake()
    {
        // Find controllers if not assigned
        if (gameController == null)
        {
            gameController = FindObjectOfType<FaceDetectionGameController>();
            if (gameController == null)
            {
                Debug.LogError("No FaceDetectionGameController found! Please assign one in the inspector.");
            }
        }
        
        if (uiController == null)
        {
            uiController = FindObjectOfType<FaceDetectionUIController>();
            if (uiController == null)
            {
                // Create the UI controller if it doesn't exist
                GameObject uiControllerObj = new GameObject("UI Controller");
                uiController = uiControllerObj.AddComponent<FaceDetectionUIController>();
                uiController.gameController = gameController;
            }
        }
        
        // Find or create the canvas
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("UI Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        canvasRect = canvas.GetComponent<RectTransform>();
        
        // Create the UI
        SetupUI();
    }
    
    void SetupUI()
    {
        return;
        // Create a background panel
        GameObject panelGO = CreateUIElement("Background Panel", canvas.transform);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = backgroundColor;
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Camera Display
        GameObject cameraGO = CreateUIElement("Camera Display", panelRect);
        RawImage cameraDisplay = cameraPrefab != null ? 
            Instantiate(cameraPrefab, cameraGO.transform) : 
            cameraGO.AddComponent<RawImage>();
        cameraDisplay.color = Color.white;
        
        RectTransform cameraRect = cameraGO.GetComponent<RectTransform>();
        cameraRect.anchorMin = new Vector2(0f, 0.1f);
        cameraRect.anchorMax = new Vector2(1f, 0.9f);
        cameraRect.offsetMin = new Vector2(10f, 10f);
        cameraRect.offsetMax = new Vector2(-10f, -10f);
        
        // Switch Camera Button
        GameObject buttonGO = CreateUIElement("Switch Camera Button", panelRect);
        Button switchButton = switchCameraPrefab != null ? 
            Instantiate(switchCameraPrefab, buttonGO.transform) : 
            buttonGO.AddComponent<Button>();
        
        // Add image to button
        Image buttonImage = buttonGO.GetComponent<Image>();
        if (buttonImage == null)
        {
            buttonImage = buttonGO.AddComponent<Image>();
        }
        buttonImage.color = buttonColor;
        
        // Add text to button
        GameObject buttonTextGO = CreateUIElement("Button Text", buttonGO.transform);
        TMPro.TextMeshProUGUI buttonText = buttonTextGO.AddComponent<TMPro.TextMeshProUGUI>();
        buttonText.text = "Switch Camera";
        buttonText.color = textColor;
        buttonText.alignment = TMPro.TextAlignmentOptions.Center;
        buttonText.fontSize = 24;
        
        RectTransform buttonTextRect = buttonTextGO.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.02f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.08f);
        buttonRect.sizeDelta = new Vector2(200f, 50f);
        buttonRect.anchoredPosition = Vector2.zero;
        
        // Debug Text
        GameObject debugGO = CreateUIElement("Debug Text", panelRect);
        TMPro.TextMeshProUGUI debugText = debugTextPrefab != null ? 
            Instantiate(debugTextPrefab, debugGO.transform) : 
            debugGO.AddComponent<TMPro.TextMeshProUGUI>();
        debugText.text = "Debug Info";
        debugText.color = textColor;
        debugText.fontSize = 14;
        debugText.alignment = TMPro.TextAlignmentOptions.Left;
        
        RectTransform debugRect = debugGO.GetComponent<RectTransform>();
        debugRect.anchorMin = new Vector2(0f, 0.92f);
        debugRect.anchorMax = new Vector2(1f, 0.98f);
        debugRect.offsetMin = new Vector2(10f, 0f);
        debugRect.offsetMax = new Vector2(-10f, 0f);
        
        // Face Count Text
        GameObject countGO = CreateUIElement("Face Count", panelRect);
        TMPro.TextMeshProUGUI countText = faceCountPrefab != null ? 
            Instantiate(faceCountPrefab, countGO.transform) : 
            countGO.AddComponent<TMPro.TextMeshProUGUI>();
        countText.text = "Faces detected: 0";
        countText.color = textColor;
        countText.fontSize = 24;
        countText.alignment = TMPro.TextAlignmentOptions.Left;
        
        RectTransform countRect = countGO.GetComponent<RectTransform>();
        countRect.anchorMin = new Vector2(0.02f, 0.92f);
        countRect.anchorMax = new Vector2(0.3f, 0.98f);
        countRect.offsetMin = Vector2.zero;
        countRect.offsetMax = Vector2.zero;
        
        // State Log Text
        GameObject stateGO = CreateUIElement("State Log", panelRect);
        TMPro.TextMeshProUGUI stateText = stateLogPrefab != null ? 
            Instantiate(stateLogPrefab, stateGO.transform) : 
            stateGO.AddComponent<TMPro.TextMeshProUGUI>();
        stateText.text = "State Log";
        stateText.color = textColor;
        stateText.fontSize = 16;
        stateText.alignment = TMPro.TextAlignmentOptions.Left;
        
        RectTransform stateRect = stateGO.GetComponent<RectTransform>();
        stateRect.anchorMin = new Vector2(0.7f, 0.1f);
        stateRect.anchorMax = new Vector2(0.98f, 0.9f);
        stateRect.offsetMin = Vector2.zero;
        stateRect.offsetMax = Vector2.zero;
        
        // Assign references to UI controller
        uiController.cameraDisplay = cameraDisplay;
        uiController.debugText = debugText;
        uiController.faceCountText = countText;
        uiController.stateLogText = stateText;
        uiController.switchCameraButton = switchButton;
    }
    
    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.localPosition = Vector3.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        return go;
    }
}