using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Sets up a simple demo scene for face mesh visualization without camera display
/// </summary>
public class FaceMeshDemoScene : MonoBehaviour
{
    [Header("Scene Setup")]
    public bool autoSetup = true;
    public Camera mainCamera;
    
    [Header("UI Elements")]
    public Canvas uiCanvas;
    public TextMeshProUGUI statusText;
    public Button toggleMeshButton;
    public Button toggleWireframeButton;
    public Button togglePointsButton;
    public Slider scaleSlider;
    
    private MLKitManager mlkitManager;
    private FaceMeshVisualizer meshVisualizer;
    
    void Start()
    {
        if (autoSetup)
        {
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        SetupCamera();
        SetupMLKit();
        SetupFaceMeshVisualizer();
        SetupUI();
        
        Debug.Log("Face Mesh Demo Scene setup complete!");
    }
    
    private void SetupCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraGO = new GameObject("Main Camera");
                mainCamera = cameraGO.AddComponent<Camera>();
                cameraGO.tag = "MainCamera";
            }
        }
        
        // Position camera to view the face mesh
        mainCamera.transform.position = new Vector3(0, 0, 5);
        mainCamera.transform.rotation = Quaternion.identity;
        mainCamera.fieldOfView = 60f;
        mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
    }
    
    private void SetupMLKit()
    {
        // Find or create MLKit manager
        mlkitManager = MLKitManager.Instance;
        if (mlkitManager == null)
        {
            GameObject mlkitGO = new GameObject("MLKitManager");
            mlkitManager = mlkitGO.AddComponent<MLKitManager>();
        }
        
        // Configure for face mesh detection
        mlkitManager.enableFaceMesh = true;
        mlkitManager.useFaceMeshHighAccuracy = false; // Start with fast mode
        mlkitManager.enableLandmarks = false; // Disable landmarks for this demo
        mlkitManager.enableContours = false; // Disable contours for this demo
    }
    
    private void SetupFaceMeshVisualizer()
    {
        // Create face mesh visualizer
        GameObject visualizerGO = new GameObject("FaceMeshVisualizer");
        meshVisualizer = visualizerGO.AddComponent<FaceMeshVisualizer>();
        
        // Configure visualizer
        meshVisualizer.mlkitManager = mlkitManager;
        meshVisualizer.meshScale = 5f;
        meshVisualizer.zOffset = -2f;
        meshVisualizer.showMesh = true;
        meshVisualizer.showWireframe = true;
        meshVisualizer.showPoints = false;
        meshVisualizer.flipHorizontally = true;
    }
    
    private void SetupUI()
    {
        // Create or find canvas
        if (uiCanvas == null)
        {
            GameObject canvasGO = new GameObject("UI Canvas");
            uiCanvas = canvasGO.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Create UI panel
        GameObject panelGO = CreateUIElement("Control Panel", uiCanvas.transform);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0.3f, 1);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Create status text
        statusText = CreateTextElement("Status Text", panelRect, "Face Mesh Demo\nInitializing...", 
            new Vector2(0, 0.8f), new Vector2(1, 1), 18);
        
        // Create control buttons
        toggleMeshButton = CreateButton("Toggle Mesh", panelRect, "Toggle Mesh", 
            new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.75f));
        toggleMeshButton.onClick.AddListener(() => {
            if (meshVisualizer != null) meshVisualizer.ToggleMesh();
        });
        
        toggleWireframeButton = CreateButton("Toggle Wireframe", panelRect, "Toggle Wireframe", 
            new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.6f));
        toggleWireframeButton.onClick.AddListener(() => {
            if (meshVisualizer != null) meshVisualizer.ToggleWireframe();
        });
        
        togglePointsButton = CreateButton("Toggle Points", panelRect, "Toggle Points", 
            new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.45f));
        togglePointsButton.onClick.AddListener(() => {
            if (meshVisualizer != null) meshVisualizer.TogglePoints();
        });
        
        // Create scale slider
        GameObject sliderGO = CreateUIElement("Scale Slider", panelRect);
        RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.1f, 0.2f);
        sliderRect.anchorMax = new Vector2(0.9f, 0.3f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        
        scaleSlider = sliderGO.AddComponent<Slider>();
        scaleSlider.minValue = 0.1f;
        scaleSlider.maxValue = 10f;
        scaleSlider.value = 5f;
        scaleSlider.onValueChanged.AddListener((value) => {
            if (meshVisualizer != null) meshVisualizer.SetMeshScale(value);
        });
        
        // Add slider background and handle
        GameObject sliderBG = CreateUIElement("Background", sliderGO.transform);
        Image sliderBGImage = sliderBG.AddComponent<Image>();
        sliderBGImage.color = new Color(0.3f, 0.3f, 0.3f);
        RectTransform sliderBGRect = sliderBG.GetComponent<RectTransform>();
        sliderBGRect.anchorMin = Vector2.zero;
        sliderBGRect.anchorMax = Vector2.one;
        sliderBGRect.offsetMin = Vector2.zero;
        sliderBGRect.offsetMax = Vector2.zero;
        
        GameObject sliderHandle = CreateUIElement("Handle", sliderGO.transform);
        Image sliderHandleImage = sliderHandle.AddComponent<Image>();
        sliderHandleImage.color = new Color(0.8f, 0.8f, 0.8f);
        
        scaleSlider.targetGraphic = sliderBGImage;
        scaleSlider.handleRect = sliderHandle.GetComponent<RectTransform>();
        
        // Add slider label
        CreateTextElement("Scale Label", panelRect, "Mesh Scale", 
            new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.2f), 14);
        
        // Add instructions
        CreateTextElement("Instructions", panelRect, 
            "Look at the camera to see\nyour face mesh visualization.\n\nUse controls to toggle\ndifferent view modes.", 
            new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.12f), 12);
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
    
    private TextMeshProUGUI CreateTextElement(string name, Transform parent, string text, 
        Vector2 anchorMin, Vector2 anchorMax, int fontSize)
    {
        GameObject textGO = CreateUIElement(name, parent);
        TextMeshProUGUI textComponent = textGO.AddComponent<TextMeshProUGUI>();
        
        textComponent.text = text;
        textComponent.color = Color.white;
        textComponent.fontSize = fontSize;
        textComponent.alignment = TextAlignmentOptions.Left;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = anchorMin;
        textRect.anchorMax = anchorMax;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return textComponent;
    }
    
    private Button CreateButton(string name, Transform parent, string buttonText, 
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject buttonGO = CreateUIElement(name, parent);
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.8f);
        
        Button button = buttonGO.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchorMin;
        buttonRect.anchorMax = anchorMax;
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // Add button text
        GameObject textGO = CreateUIElement("Text", buttonGO.transform);
        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.color = Color.white;
        text.fontSize = 16;
        text.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return button;
    }
    
    void Update()
    {
        // Update status text
        if (statusText != null)
        {
            string status = "Face Mesh Demo\n";
            
            if (mlkitManager != null)
            {
                status += mlkitManager.name + " Ready\n";
            }
            
            if (meshVisualizer != null)
            {
                status += "Visualizer Active\n";
                status += $"Scale: {meshVisualizer.meshScale:F1}\n";
                status += $"Mesh: {(meshVisualizer.showMesh ? "ON" : "OFF")}\n";
                status += $"Wireframe: {(meshVisualizer.showWireframe ? "ON" : "OFF")}\n";
                status += $"Points: {(meshVisualizer.showPoints ? "ON" : "OFF")}";
            }
            
            statusText.text = status;
        }
    }
    
    void OnDestroy()
    {
        // Cleanup if needed
    }
}