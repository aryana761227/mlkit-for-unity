using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Visualizes face mesh data as 3D triangulated meshes without camera display
/// </summary>
public class FaceMeshVisualizer : MonoBehaviour
{
    [Header("Face Mesh Detection")]
    public MLKitManager mlkitManager;
    
    [Header("Visualization Settings")]
    public Material meshMaterial;
    public Material wireframeMaterial;
    [Range(0.1f, 10f)]
    public float meshScale = 5f;
    [Range(-5f, 5f)]
    public float zOffset = -2f;
    
    [Header("Display Options")]
    public bool showMesh = true;
    public bool showWireframe = true;
    public bool showPoints = false;
    public bool flipHorizontally = true;
    
    [Header("Colors")]
    public Color meshColor = new Color(1f, 0.8f, 0.6f, 0.7f);
    public Color wireframeColor = Color.blue;
    public Color pointColor = Color.red;
    
    private List<GameObject> meshObjects = new List<GameObject>();
    private FaceMeshData currentMeshData;
    
    void Start()
    {
        // Get MLKit manager if not assigned
        if (mlkitManager == null)
        {
            mlkitManager = MLKitManager.Instance;
            if (mlkitManager == null)
            {
                Debug.LogError("MLKitManager not found! Please assign it or ensure it exists in the scene.");
                enabled = false;
                return;
            }
        }
        
        // Subscribe to face mesh detection events
        mlkitManager.OnFaceMeshDetectionComplete += HandleFaceMeshResult;
        
        // Create default materials if not assigned
        CreateDefaultMaterials();
        
        // Configure MLKit for face mesh detection
        mlkitManager.ConfigureFaceMeshDetection(true, false);
        
        Debug.Log("FaceMeshVisualizer initialized and listening for face mesh data");
    }
    
    private void CreateDefaultMaterials()
    {
        if (meshMaterial == null)
        {
            meshMaterial = new Material(Shader.Find("Standard"));
            meshMaterial.color = meshColor;
            meshMaterial.SetFloat("_Mode", 3); // Transparent mode
            meshMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            meshMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            meshMaterial.SetInt("_ZWrite", 0);
            meshMaterial.DisableKeyword("_ALPHATEST_ON");
            meshMaterial.EnableKeyword("_ALPHABLEND_ON");
            meshMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            meshMaterial.renderQueue = 3000;
        }
        
        if (wireframeMaterial == null)
        {
            wireframeMaterial = new Material(Shader.Find("Unlit/Color"));
            wireframeMaterial.color = wireframeColor;
        }
    }
    
    private void HandleFaceMeshResult(string result)
    {
        // Parse the face mesh data
        currentMeshData = FaceMeshData.ParseBinaryResult(result);
        
        // Visualize the mesh
        VisualizeFaceMesh(currentMeshData);
    }
    
    private void VisualizeFaceMesh(FaceMeshData meshData)
    {
        // Clear existing mesh objects
        ClearMeshObjects();
        
        if (meshData.meshes.Count == 0)
        {
            Debug.Log("No face meshes detected");
            return;
        }
        
        Debug.Log($"Visualizing {meshData.meshes.Count} face meshes");
        
        for (int i = 0; i < meshData.meshes.Count; i++)
        {
            FaceMesh faceMesh = meshData.meshes[i];
            CreateMeshVisualization(faceMesh, i);
        }
    }
    
    private void CreateMeshVisualization(FaceMesh faceMesh, int meshIndex)
    {
        if (faceMesh.points.Count == 0)
        {
            Debug.LogWarning($"Face mesh {meshIndex} has no points");
            return;
        }
        
        // Create parent object for this face mesh
        GameObject meshParent = new GameObject($"FaceMesh_{meshIndex}");
        meshParent.transform.SetParent(transform);
        meshObjects.Add(meshParent);
        
        // Transform points to Unity coordinate system
        Vector3[] transformedPoints = TransformPoints(faceMesh.GetPointsArray());
        
        // Create mesh visualization
        if (showMesh && faceMesh.triangles.Count > 0)
        {
            CreateTriangulatedMesh(meshParent, transformedPoints, faceMesh.GetTriangleIndices(), meshIndex);
        }
        
        // Create wireframe visualization
        if (showWireframe && faceMesh.triangles.Count > 0)
        {
            CreateWireframeMesh(meshParent, transformedPoints, faceMesh.triangles, meshIndex);
        }
        
        // Create point visualization
        if (showPoints)
        {
            CreatePointVisualization(meshParent, transformedPoints, meshIndex);
        }
        
        Debug.Log($"Created visualization for mesh {meshIndex}: {faceMesh.points.Count} points, {faceMesh.triangles.Count} triangles");
    }
    
    private Vector3[] TransformPoints(Vector3[] originalPoints)
    {
        Vector3[] transformed = new Vector3[originalPoints.Length];
        
        for (int i = 0; i < originalPoints.Length; i++)
        {
            Vector3 point = originalPoints[i];
            
            // Apply scaling
            point *= meshScale;
            
            // Flip horizontally if needed (common for front-facing camera)
            if (flipHorizontally)
            {
                point.x = -point.x;
            }
            
            // Apply Z offset to move mesh in front of camera
            point.z += zOffset;
            
            transformed[i] = point;
        }
        
        return transformed;
    }
    
    private void CreateTriangulatedMesh(GameObject parent, Vector3[] points, int[] triangles, int meshIndex)
    {
        GameObject meshObj = new GameObject($"TriangulatedMesh_{meshIndex}");
        meshObj.transform.SetParent(parent.transform);
        
        MeshFilter meshFilter = meshObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObj.AddComponent<MeshRenderer>();
        
        Mesh mesh = new Mesh();
        mesh.name = $"FaceMesh_{meshIndex}";
        mesh.vertices = points;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        meshFilter.mesh = mesh;
        meshRenderer.material = meshMaterial;
        
        Debug.Log($"Created triangulated mesh with {points.Length} vertices and {triangles.Length / 3} triangles");
    }
    
    private void CreateWireframeMesh(GameObject parent, Vector3[] points, List<FaceTriangle> triangles, int meshIndex)
    {
        GameObject wireframeObj = new GameObject($"WireframeMesh_{meshIndex}");
        wireframeObj.transform.SetParent(parent.transform);
        
        LineRenderer lineRenderer = wireframeObj.AddComponent<LineRenderer>();
        lineRenderer.material = wireframeMaterial;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.useWorldSpace = false;
        
        // Create lines for triangle edges
        List<Vector3> linePoints = new List<Vector3>();
        
        foreach (FaceTriangle triangle in triangles)
        {
            if (triangle.vertex1 < points.Length && triangle.vertex2 < points.Length && triangle.vertex3 < points.Length)
            {
                // Add triangle edges
                linePoints.Add(points[triangle.vertex1]);
                linePoints.Add(points[triangle.vertex2]);
                
                linePoints.Add(points[triangle.vertex2]);
                linePoints.Add(points[triangle.vertex3]);
                
                linePoints.Add(points[triangle.vertex3]);
                linePoints.Add(points[triangle.vertex1]);
            }
        }
        
        if (linePoints.Count > 0)
        {
            lineRenderer.positionCount = linePoints.Count;
            lineRenderer.SetPositions(linePoints.ToArray());
        }
        
        Debug.Log($"Created wireframe with {linePoints.Count / 2} line segments");
    }
    
    private void CreatePointVisualization(GameObject parent, Vector3[] points, int meshIndex)
    {
        GameObject pointsObj = new GameObject($"Points_{meshIndex}");
        pointsObj.transform.SetParent(parent.transform);
        
        for (int i = 0; i < points.Length; i++)
        {
            GameObject pointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObj.name = $"Point_{i}";
            pointObj.transform.SetParent(pointsObj.transform);
            pointObj.transform.localPosition = points[i];
            pointObj.transform.localScale = Vector3.one * 0.05f;
            
            Renderer renderer = pointObj.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Unlit/Color"));
            renderer.material.color = pointColor;
        }
        
        Debug.Log($"Created {points.Length} point visualizations");
    }
    
    private void ClearMeshObjects()
    {
        foreach (GameObject obj in meshObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        meshObjects.Clear();
    }
    
    // Public methods for runtime control
    public void ToggleMesh()
    {
        showMesh = !showMesh;
        if (currentMeshData != null)
        {
            VisualizeFaceMesh(currentMeshData);
        }
    }
    
    public void ToggleWireframe()
    {
        showWireframe = !showWireframe;
        if (currentMeshData != null)
        {
            VisualizeFaceMesh(currentMeshData);
        }
    }
    
    public void TogglePoints()
    {
        showPoints = !showPoints;
        if (currentMeshData != null)
        {
            VisualizeFaceMesh(currentMeshData);
        }
    }
    
    public void SetMeshScale(float scale)
    {
        meshScale = scale;
        if (currentMeshData != null)
        {
            VisualizeFaceMesh(currentMeshData);
        }
    }
    
    void OnDestroy()
    {
        if (mlkitManager != null)
        {
            mlkitManager.OnFaceMeshDetectionComplete -= HandleFaceMeshResult;
        }
        
        ClearMeshObjects();
    }
    
    void OnValidate()
    {
        // Update material colors when changed in inspector
        if (meshMaterial != null)
        {
            meshMaterial.color = meshColor;
        }
        
        if (wireframeMaterial != null)
        {
            wireframeMaterial.color = wireframeColor;
        }
    }
}