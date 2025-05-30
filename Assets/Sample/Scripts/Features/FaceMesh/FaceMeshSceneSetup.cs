using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Sets up a complete face mesh visualization scene
/// </summary>
public class FaceMeshSceneSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoSetup = true;
    
    [Header("UI Layout")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f);
    public Color panelColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    public Color buttonColor = new Color(0.3f, 0.6f, 0.9f);
    public Color textColor = Color.white;
    
    private Canvas canvas;
    private MLKitManager mlkitManager;
    private FaceMeshVisualizationController meshController;
    
    void Awake()
    {
        if (autoSetup)
        {
            SetupFaceMeshScene();
        }
    }
    
    [ContextMenu("Setup Face Mesh Scene")]
    public void SetupFaceMeshScene()
    {
        // Create main camera if needed
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            mainCamera.tag = "MainCamera";
            cameraObj.AddComponent<AudioListener>();
        }
        
        // Position camera for 3D view
        mainCamera.transform.position = new Vector3(0, 0, -10);
        mainCamera.transform.rotation = Quaternion.identity;
        mainCamera.backgroundColor = backgroundColor;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        
        // Find or create MLKitManager
        mlkitManager = FindObjectOfType<MLKitManager>();
        if (mlkitManager == null)
        {
            GameObject mlkitObj = new GameObject("MLKitManager");
            mlkitManager = mlkitObj.AddComponent<MLKitManager>();
            mlkitManager.detectionMode = MLKitManager.DetectionMode.FACE_MESH;
            mlkitManager.useHighAccuracyMesh = true;
        }
        
        // Create canvas
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("UI Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create UI layout
        CreateUILayout();
        
        // Create mesh visualization controller
        GameObject meshControllerObj = new GameObject("FaceMeshVisualizationController");
        meshController = meshControllerObj.AddComponent<FaceMeshVisualizationController>();
        
        // Create visualization container
        GameObject vizContainer = new GameObject("3D Visualization Container");
        vizContainer.transform.position = new Vector3(0, 0, 0);
        
        // Link everything together
        LinkComponents();
        
        Debug.Log("Face Mesh scene setup complete!");
    }
    
    private void CreateUILayout()
    {
        // Create main UI panel (left side)
        GameObject panelObj = CreateUIElement("Control Panel", canvas.transform);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = panelColor;
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0.25f, 1);
        panelRect.offsetMin = new Vector2(10, 10);
        panelRect.offsetMax = new Vector2(-10, -10);
        
        // Add vertical layout group
        VerticalLayoutGroup layoutGroup = panelObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.spacing = 10;
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        
        // Title
        GameObject titleObj = CreateUIElement("Title", panelObj.transform);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Face Mesh Visualization";
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = textColor;
        titleText.alignment = TextAlignmentOptions.Center;
        
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 40;
        
        // Camera display (small preview)
        GameObject cameraPreviewObj = CreateUIElement("Camera Preview", panelObj.transform);
        RawImage cameraDisplay = cameraPreviewObj.AddComponent<RawImage>();
        cameraDisplay.color = Color.white;
        
        AspectRatioFitter aspectFitter = cameraPreviewObj.AddComponent<AspectRatioFitter>();
        aspectFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        aspectFitter.aspectRatio = 1.777f; // 16:9
        
        LayoutElement cameraLayout = cameraPreviewObj.AddComponent<LayoutElement>();
        cameraLayout.preferredHeight = 200;
        
        // Debug text
        GameObject debugObj = CreateUIElement("Debug Text", panelObj.transform);
        TextMeshProUGUI debugText = debugObj.AddComponent<TextMeshProUGUI>();
        debugText.text = "Waiting for face mesh...";
        debugText.fontSize = 16;
        debugText.color = textColor;
        debugText.alignment = TextAlignmentOptions.Center;
        
        LayoutElement debugLayout = debugObj.AddComponent<LayoutElement>();
        debugLayout.preferredHeight = 30;
        
        // Stats text
        GameObject statsObj = CreateUIElement("Stats", panelObj.transform);
        TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
        statsText.text = "FPS: 0\nPoints: 0\nTriangles: 0";
        statsText.fontSize = 14;
        statsText.color = textColor;
        statsText.alignment = TextAlignmentOptions.Left;
        
        LayoutElement statsLayout = statsObj.AddComponent<LayoutElement>();
        statsLayout.preferredHeight = 80;
        
        // Separator
        CreateSeparator(panelObj.transform);
        
        // High Accuracy Toggle
        GameObject accuracyToggleObj = CreateToggleWithLabel(panelObj.transform, "High Accuracy Mode", true);
        Toggle highAccuracyToggle = accuracyToggleObj.GetComponentInChildren<Toggle>();
        
        // Buttons
        GameObject switchCameraBtn = CreateButton(panelObj.transform, "Switch Camera", buttonColor);
        GameObject toggleWireframeBtn = CreateButton(panelObj.transform, "Hide Wireframe", buttonColor);
        GameObject togglePointsBtn = CreateButton(panelObj.transform, "Show Points", buttonColor);
        
        // Store references for linking
        StoreUIReferences(cameraDisplay, debugText, statsText, 
                         switchCameraBtn.GetComponent<Button>(),
                         toggleWireframeBtn.GetComponent<Button>(),
                         togglePointsBtn.GetComponent<Button>(),
                         highAccuracyToggle);
    }
    
    private void CreateSeparator(Transform parent)
    {
        GameObject separator = CreateUIElement("Separator", parent);
        Image sepImage = separator.AddComponent<Image>();
        sepImage.color = new Color(1, 1, 1, 0.2f);
        
        LayoutElement sepLayout = separator.AddComponent<LayoutElement>();
        sepLayout.preferredHeight = 2;
        sepLayout.preferredWidth = -1;
    }
    
    private GameObject CreateToggleWithLabel(Transform parent, string label, bool defaultValue)
    {
        GameObject container = CreateUIElement(label + " Container", parent);
        HorizontalLayoutGroup hLayout = container.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 10;
        hLayout.childAlignment = TextAnchor.MiddleLeft;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = false;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = false;
        
        LayoutElement containerLayout = container.AddComponent<LayoutElement>();
        containerLayout.preferredHeight = 30;
        
        // Toggle
        GameObject toggleObj = CreateUIElement("Toggle", container.transform);
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = defaultValue;
        
        // Toggle background
        GameObject bgObj = CreateUIElement("Background", toggleObj.transform);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.5f, 0.5f, 0.5f);
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(40, 20);
        
        // Checkmark
        GameObject checkObj = CreateUIElement("Checkmark", bgObj.transform);
        Image checkImage = checkObj.AddComponent<Image>();
        checkImage.color = buttonColor;
        
        RectTransform checkRect = checkObj.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.1f, 0.1f);
        checkRect.anchorMax = new Vector2(0.9f, 0.9f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;
        
        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;
        
        // Label
        GameObject labelObj = CreateUIElement("Label", container.transform);
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 16;
        labelText.color = textColor;
        
        LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.flexibleWidth = 1;
        
        return container;
    }
    
    private GameObject CreateButton(Transform parent, string text, Color color)
    {
        GameObject buttonObj = CreateUIElement(text + " Button", parent);
        Button button = buttonObj.AddComponent<Button>();
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;
        
        LayoutElement buttonLayout = buttonObj.AddComponent<LayoutElement>();
        buttonLayout.preferredHeight = 40;
        
        // Button text
        GameObject textObj = CreateUIElement("Text", buttonObj.transform);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 18;
        buttonText.color = textColor;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        button.targetGraphic = buttonImage;
        
        return buttonObj;
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
    
    // Temporary storage for UI references
    private RawImage tempCameraDisplay;
    private TextMeshProUGUI tempDebugText;
    private TextMeshProUGUI tempStatsText;
    private Button tempSwitchCameraBtn;
    private Button tempToggleWireframeBtn;
    private Button tempTogglePointsBtn;
    private Toggle tempHighAccuracyToggle;
    
    private void StoreUIReferences(RawImage cameraDisplay, TextMeshProUGUI debugText, 
                                  TextMeshProUGUI statsText, Button switchCameraBtn,
                                  Button toggleWireframeBtn, Button togglePointsBtn,
                                  Toggle highAccuracyToggle)
    {
        tempCameraDisplay = cameraDisplay;
        tempDebugText = debugText;
        tempStatsText = statsText;
        tempSwitchCameraBtn = switchCameraBtn;
        tempToggleWireframeBtn = toggleWireframeBtn;
        tempTogglePointsBtn = togglePointsBtn;
        tempHighAccuracyToggle = highAccuracyToggle;
    }
    
    private void LinkComponents()
    {
        if (meshController != null)
        {
            meshController.mlkitManager = mlkitManager;
            meshController.cameraDisplay = tempCameraDisplay;
            meshController.debugText = tempDebugText;
            meshController.statsText = tempStatsText;
            meshController.switchCameraButton = tempSwitchCameraBtn;
            meshController.toggleWireframeButton = tempToggleWireframeBtn;
            meshController.togglePointsButton = tempTogglePointsBtn;
            meshController.highAccuracyToggle = tempHighAccuracyToggle;
            
            // Find visualization container
            GameObject vizContainer = GameObject.Find("3D Visualization Container");
            if (vizContainer != null)
            {
                meshController.meshVisualizationContainer = vizContainer;
            }
        }
    }
}