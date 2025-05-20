using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Setup script for adding face landmarks and contours visualization to a scene
/// </summary>
public class FaceLandmarksSetup : MonoBehaviour
{
    [Header("Face Detection")]
    public bool autoSetup = true;
    public bool createDemoUI = true;
    
    [Header("UI References")]
    public Canvas canvas;
    public RawImage cameraDisplay;
    
    private MLKitManager mlkitManager;
    private FaceDetectionGameController gameController;
    private FaceDetectionUIController uiController;
    
    void Awake()
    {
        if (autoSetup)
        {
            SetupFaceLandmarksSystem();
        }
    }
    
    [ContextMenu("Setup Face Landmarks System")]
    public void SetupFaceLandmarksSystem()
    {
        // Find or create references to existing components
        FindExistingComponents();
        
        // Create missing components
        CreateMissingComponents();
        
        // Set up demo UI if requested
        if (createDemoUI)
        {
            SetupDemoUI();
        }
        
        Debug.Log("Face landmarks system has been set up successfully!");
    }
    
    private void FindExistingComponents()
    {
        // Find the MLKit manager
        mlkitManager = FindObjectOfType<MLKitManager>();
        if (mlkitManager == null)
        {
            Debug.Log("MLKitManager not found, will create a new one.");
        }
        
        // Find the game controller
        gameController = FindObjectOfType<FaceDetectionGameController>();
        if (gameController == null)
        {
            Debug.Log("FaceDetectionGameController not found, will create a new one.");
        }
        
        // Find the UI controller
        uiController = FindObjectOfType<FaceDetectionUIController>();
        if (uiController == null)
        {
            Debug.Log("FaceDetectionUIController not found, will create a new one.");
        }
        
        // Find canvas
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.Log("Canvas not found, will create a new one.");
            }
        }
        
        // Find camera display
        if (cameraDisplay == null && uiController != null)
        {
            cameraDisplay = uiController.cameraDisplay;
        }
        
        // Find landmark visualizer
    }
    
    private void CreateMissingComponents()
    {
        // Create MLKit manager if needed
        if (mlkitManager == null)
        {
            GameObject mlkitObj = new GameObject("MLKitManager");
            mlkitManager = mlkitObj.AddComponent<MLKitManager>();
            mlkitManager.enableLandmarks = true;
            mlkitManager.enableContours = true;
        }
        
        // Create game controller if needed
        if (gameController == null)
        {
            GameObject gameControllerObj = new GameObject("FaceDetectionGameController");
            gameController = gameControllerObj.AddComponent<FaceDetectionGameController>();
            gameController.DetectionInterval = 0.2f;
            gameController.EnableStateLogging = true;
        }
        
        // Create canvas if needed
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create camera display if needed
        if (cameraDisplay == null)
        {
            GameObject displayObj = new GameObject("CameraDisplay");
            displayObj.transform.SetParent(canvas.transform, false);
            
            cameraDisplay = displayObj.AddComponent<RawImage>();
            cameraDisplay.color = Color.white;
            
            RectTransform rectTransform = displayObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(20, 20);
            rectTransform.offsetMax = new Vector2(-20, -20);
            
            // Now that we have a camera display, if there's a UI controller, assign it
            if (uiController != null)
            {
                uiController.cameraDisplay = cameraDisplay;
            }
        }
        
        // Create UI controller if needed
        if (uiController == null)
        {
            GameObject uiControllerObj = new GameObject("FaceDetectionUIController");
            uiController = uiControllerObj.AddComponent<FaceDetectionUIController>();
            uiController.gameController = gameController;
            uiController.cameraDisplay = cameraDisplay;
        }
        
    }
    
    private void SetupDemoUI()
    {
        // Create parent panel for UI controls
        GameObject panelObj = new GameObject("FaceLandmarksDemoPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0.3f, 1);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Add title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelRect, false);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Face Landmarks Demo";
        titleText.color = Color.white;
        titleText.fontSize = 24;
        titleText.alignment = TextAlignmentOptions.Center;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.9f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = new Vector2(10, 0);
        titleRect.offsetMax = new Vector2(-10, 0);
        
        // Add toggles
        Toggle landmarksToggle = CreateToggle(panelRect, "Show Landmarks", 0.8f);
        Toggle contoursToggle = CreateToggle(panelRect, "Show Contours", 0.75f);
        Toggle labelsToggle = CreateToggle(panelRect, "Show Labels", 0.7f);
        
        // Add stats text
        GameObject statsObj = new GameObject("Stats");
        statsObj.transform.SetParent(panelRect, false);
        
        TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
        statsText.text = "FPS: 0\nFaces: 0\nLandmarks: 0\nContour Points: 0\nMemory: 0 MB";
        statsText.color = Color.white;
        statsText.fontSize = 16;
        statsText.alignment = TextAlignmentOptions.Left;
        
        RectTransform statsRect = statsObj.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 0.4f);
        statsRect.anchorMax = new Vector2(1, 0.65f);
        statsRect.offsetMin = new Vector2(10, 0);
        statsRect.offsetMax = new Vector2(-10, 0);
        
        // Add reset button
        GameObject buttonObj = new GameObject("ResetButton");
        buttonObj.transform.SetParent(panelRect, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.8f);
        
        Button resetButton = buttonObj.AddComponent<Button>();
        resetButton.targetGraphic = buttonImage;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.2f, 0.1f);
        buttonRect.anchorMax = new Vector2(0.8f, 0.2f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // Add button text
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Reset";
        buttonText.color = Color.white;
        buttonText.fontSize = 18;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
    }
    
    private Toggle CreateToggle(RectTransform parent, string label, float anchorY)
    {
        GameObject toggleObj = new GameObject(label + "Toggle");
        toggleObj.transform.SetParent(parent, false);
        
        RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0, anchorY - 0.05f);
        toggleRect.anchorMax = new Vector2(1, anchorY);
        toggleRect.offsetMin = new Vector2(10, 0);
        toggleRect.offsetMax = new Vector2(-10, 0);
        
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(toggleObj.transform, false);
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f);
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.5f);
        bgRect.anchorMax = new Vector2(0, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta = new Vector2(20, 20);
        bgRect.anchoredPosition = new Vector2(10, 0);
        
        GameObject checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(bgObj.transform, false);
        
        Image checkImage = checkObj.AddComponent<Image>();
        checkImage.color = Color.white;
        
        RectTransform checkRect = checkObj.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.2f, 0.2f);
        checkRect.anchorMax = new Vector2(0.8f, 0.8f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;
        
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(toggleObj.transform, false);
        
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.color = Color.white;
        labelText.fontSize = 16;
        labelText.alignment = TextAlignmentOptions.Left;
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(40, 0);
        labelRect.offsetMax = new Vector2(0, 0);
        
        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;
        toggle.isOn = true;
        
        return toggle;
    }
}