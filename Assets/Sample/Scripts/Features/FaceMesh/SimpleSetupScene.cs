using UnityEngine;

/// <summary>
/// Simple scene setup that only creates the face mesh triangle visualization
/// No UI, no camera display, just triangles
/// </summary>
public class SimpleSetupScene : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool setupOnStart = true;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupFaceMeshVisualization();
        }
    }
    
    [ContextMenu("Setup Face Mesh Visualization")]
    public void SetupFaceMeshVisualization()
    {
        // Setup camera for viewing
        SetupCamera();
        
        // Create MLKit manager
        SetupMLKit();
        
        // Create face mesh visualizer
        SetupVisualizer();
        
        Debug.Log("Face mesh triangle visualization setup complete!");
    }
    
    private void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraGO = new GameObject("Main Camera");
            mainCamera = cameraGO.AddComponent<Camera>();
            cameraGO.tag = "MainCamera";
        }
        
        // Position camera to view the face mesh triangles
        mainCamera.transform.position = new Vector3(0, 0, 5);
        mainCamera.transform.rotation = Quaternion.identity;
        mainCamera.fieldOfView = 60f;
        mainCamera.backgroundColor = Color.black; // Black background to see triangles clearly
    }
    
    private void SetupMLKit()
    {
        // Create MLKit manager if it doesn't exist
        MLKitManager mlkitManager = MLKitManager.Instance;
        if (mlkitManager == null)
        {
            GameObject mlkitGO = new GameObject("MLKitManager");
            mlkitManager = mlkitGO.AddComponent<MLKitManager>();
        }
        
        // Configure for face mesh detection only
        mlkitManager.enableFaceMesh = true;
        mlkitManager.useFaceMeshHighAccuracy = false; // Fast mode for better performance
        mlkitManager.enableLandmarks = false; // Don't need landmarks
        mlkitManager.enableContours = false; // Don't need contours
        
        Debug.Log("MLKit configured for face mesh triangles only");
    }
    
    private void SetupVisualizer()
    {
        // Create visualizer object
        GameObject visualizerGO = new GameObject("SimpleFaceMeshVisualizer");
        SimpleFaceMeshVisualizer visualizer = visualizerGO.AddComponent<SimpleFaceMeshVisualizer>();
        
        // Configure for good triangle visibility
        visualizer.meshScale = 5f;
        visualizer.zOffset = -2f;
        visualizer.flipHorizontally = true;
        visualizer.triangleColor = new Color(0.0f, 0.8f, 1.0f, 0.7f); // Nice blue color
        
        Debug.Log("Face mesh triangle visualizer created");
    }
}