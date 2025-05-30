using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class FaceMeshVisualizationController : MonoBehaviour
{
    [Header("References")]
    public MLKitManager mlkitManager;
    public RawImage cameraDisplay;
    public GameObject meshVisualizationContainer;
    
    [Header("UI Elements")]
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI statsText;
    public Button switchCameraButton;
    public Button toggleWireframeButton;
    public Button togglePointsButton;
    public Toggle highAccuracyToggle;
    
    [Header("Visualization Settings")]
    public Material wireframeMaterial;
    public Material pointMaterial;
    public float meshScale = 10f;
    public float meshDepthOffset = 5f;
    public bool showWireframe = true;
    public bool showPoints = false;
    public float pointSize = 0.02f;
    public Color wireframeColor = Color.green;
    public Color pointColor = Color.red;
    
    [Header("Performance")]
    public float updateInterval = 0.033f; // 30 FPS
    
    private FaceMeshDetectionData lastMeshData;
    private float lastUpdateTime;
    private GameObject currentMeshObject;
    private List<GameObject> pointObjects = new List<GameObject>();
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    
    void Start()
    {
        // Find MLKitManager if not assigned
        if (mlkitManager == null)
        {
            mlkitManager = MLKitManager.Instance;
            if (mlkitManager == null)
            {
                Debug.LogError("No MLKitManager found!");
                enabled = false;
                return;
            }
        }
        
        // Set detection mode to face mesh
        mlkitManager.SetDetectionMode(MLKitManager.DetectionMode.FACE_MESH);
        
        // Subscribe to events
        mlkitManager.OnFaceMeshComplete += HandleFaceMeshResult;
        mlkitManager.OnCameraInitializedComplete += HandleCameraInitialized;
        
        // Setup UI buttons
        if (switchCameraButton != null)
            switchCameraButton.onClick.AddListener(mlkitManager.SwitchCamera);
            
        if (toggleWireframeButton != null)
            toggleWireframeButton.onClick.AddListener(ToggleWireframe);
            
        if (togglePointsButton != null)
            togglePointsButton.onClick.AddListener(TogglePoints);
            
        if (highAccuracyToggle != null)
        {
            highAccuracyToggle.isOn = mlkitManager.useHighAccuracyMesh;
            highAccuracyToggle.onValueChanged.AddListener(OnHighAccuracyChanged);
        }
        
        // Create materials if not assigned
        if (wireframeMaterial == null)
        {
            wireframeMaterial = new Material(Shader.Find("Sprites/Default"));
            wireframeMaterial.color = wireframeColor;
        }
        
        if (pointMaterial == null)
        {
            pointMaterial = new Material(Shader.Find("Sprites/Default"));
            pointMaterial.color = pointColor;
        }
        
        // Create mesh visualization container if not assigned
        if (meshVisualizationContainer == null)
        {
            meshVisualizationContainer = new GameObject("MeshVisualizationContainer");
            meshVisualizationContainer.transform.SetParent(transform);
        }
        
        // Start camera
        mlkitManager.StartCamera();
    }
    
    void Update()
    {
        // Update camera display
        if (cameraDisplay != null && mlkitManager?.DisplayTexture != null)
        {
            cameraDisplay.texture = mlkitManager.DisplayTexture;
        }
        
        // Update stats
        UpdateStats();
    }
    
    void HandleCameraInitialized(string message)
    {
        var success = message.ToLower().Contains("success");
        if (debugText is not null)
        {
            debugText.text = success ? "Camera initialized" : $"Camera failed: {message}";
        }
    }
    
    void HandleFaceMeshResult(string result)
    {
        // Throttle updates
        if (Time.time - lastUpdateTime < updateInterval)
            return;
            
        lastUpdateTime = Time.time;
        
        // Parse face mesh data
        lastMeshData = FaceMeshDetectionData.ParseBinaryResult(result);
        
        if (debugText != null)
        {
            debugText.text = $"Meshes detected: {lastMeshData.meshes.Count}";
        }
        
        // Visualize the first mesh
        if (lastMeshData.meshes.Count > 0)
        {
            VisualizeMesh(lastMeshData.meshes[0]);
        }
        else
        {
            // Clear visualization if no faces
            ClearVisualization();
        }
    }
    
    void VisualizeMesh(FaceMeshData meshData)
    {
        // Create or update mesh object
        if (currentMeshObject == null)
        {
            currentMeshObject = new GameObject("FaceMesh");
            currentMeshObject.transform.SetParent(meshVisualizationContainer.transform);
            meshFilter = currentMeshObject.AddComponent<MeshFilter>();
            meshRenderer = currentMeshObject.AddComponent<MeshRenderer>();
        }
        
        // Create Unity mesh
        Mesh mesh = new Mesh();
        mesh.name = "FaceMesh";
        
        // Convert points to Unity coordinate system
        Vector3[] vertices = new Vector3[meshData.points.Count];
        for (int i = 0; i < meshData.points.Count; i++)
        {
            // ML Kit coordinates: x,y are normalized screen coordinates (0-1), z is depth
            // Convert to Unity world space centered around origin
            float x = (meshData.points[i].x - 0.5f) * meshScale;
            float y = -(meshData.points[i].y - 0.5f) * meshScale; // Flip Y
            float z = meshData.points[i].z * 0.1f + meshDepthOffset; // Scale down Z
            
            vertices[i] = new Vector3(x, y, z);
        }
        
        mesh.vertices = vertices;
        mesh.triangles = meshData.GetTriangles();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        // Apply mesh
        meshFilter.mesh = mesh;
        
        // Set material based on wireframe setting
        if (showWireframe)
        {
            meshRenderer.material = wireframeMaterial;
            meshRenderer.material.SetFloat("_Wireframe", 1);
        }
        else
        {
            meshRenderer.enabled = false;
        }
        
        // Visualize points if enabled
        if (showPoints)
        {
            VisualizePoints(vertices);
        }
        else
        {
            ClearPoints();
        }
        
        // Update mesh object visibility
        currentMeshObject.SetActive(showWireframe || showPoints);
    }
    
    void VisualizePoints(Vector3[] vertices)
    {
        // Ensure we have enough point objects
        while (pointObjects.Count < vertices.Length)
        {
            GameObject pointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObj.transform.SetParent(meshVisualizationContainer.transform);
            pointObj.transform.localScale = Vector3.one * pointSize;
            pointObj.GetComponent<MeshRenderer>().material = pointMaterial;
            
            // Remove collider for performance
            Destroy(pointObj.GetComponent<Collider>());
            
            pointObjects.Add(pointObj);
        }
        
        // Position and activate points
        for (int i = 0; i < vertices.Length; i++)
        {
            pointObjects[i].transform.localPosition = vertices[i];
            pointObjects[i].SetActive(true);
        }
        
        // Deactivate unused points
        for (int i = vertices.Length; i < pointObjects.Count; i++)
        {
            pointObjects[i].SetActive(false);
        }
    }
    
    void ClearPoints()
    {
        foreach (var point in pointObjects)
        {
            point.SetActive(false);
        }
    }
    
    void ClearVisualization()
    {
        if (currentMeshObject != null)
        {
            currentMeshObject.SetActive(false);
        }
        ClearPoints();
    }
    
    void ToggleWireframe()
    {
        showWireframe = !showWireframe;
        if (meshRenderer != null)
        {
            meshRenderer.enabled = showWireframe;
        }
        
        if (toggleWireframeButton != null)
        {
            var buttonText = toggleWireframeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = showWireframe ? "Hide Wireframe" : "Show Wireframe";
            }
        }
    }
    
    void TogglePoints()
    {
        showPoints = !showPoints;
        
        if (togglePointsButton != null)
        {
            var buttonText = togglePointsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = showPoints ? "Hide Points" : "Show Points";
            }
        }
    }
    
    void OnHighAccuracyChanged(bool value)
    {
        mlkitManager.ConfigureFaceMesh(value);
    }
    
    void UpdateStats()
    {
        if (statsText != null && lastMeshData != null && lastMeshData.meshes.Count > 0)
        {
            var mesh = lastMeshData.meshes[0];
            statsText.text = $"FPS: {(int)(1f / Time.deltaTime)}\n" +
                           $"Points: {mesh.points.Count}\n" +
                           $"Triangles: {mesh.triangleIndices.Count / 3}\n" +
                           $"Contours: {mesh.contours.Count}";
        }
    }
    
    void OnDestroy()
    {
        if (mlkitManager != null)
        {
            mlkitManager.OnFaceMeshComplete -= HandleFaceMeshResult;
            mlkitManager.OnCameraInitializedComplete -= HandleCameraInitialized;
        }
        
        // Clean up points
        foreach (var point in pointObjects)
        {
            if (point != null)
                Destroy(point);
        }
    }
}