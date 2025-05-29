using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple face mesh visualizer that only shows triangles without any UI
/// </summary>
public class SimpleFaceMeshVisualizer : MonoBehaviour
{
    [Header("Face Mesh Detection")]
    public MLKitManager mlkitManager;
    
    [Header("Visualization Settings")]
    [Range(0.1f, 10f)]
    public float meshScale = 5f;
    [Range(-5f, 5f)]
    public float zOffset = -2f;
    public bool flipHorizontally = true;
    
    [Header("Triangle Display")]
    public Material triangleMaterial;
    public Color triangleColor = new Color(0.0f, 0.7f, 1.0f, 0.6f);
    
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
                // Create MLKit manager
                GameObject mlkitGO = new GameObject("MLKitManager");
                mlkitManager = mlkitGO.AddComponent<MLKitManager>();
            }
        }
        
        // Configure MLKit settings (these will be applied when camera starts)
        mlkitManager.enableFaceMesh = true;
        mlkitManager.useFaceMeshHighAccuracy = false; // Fast mode
        mlkitManager.enableLandmarks = false; // Disable landmarks
        mlkitManager.enableContours = false; // Disable contours
        
        // Subscribe to face mesh detection events
        mlkitManager.OnFaceMeshDetectionComplete += HandleFaceMeshResult;
        
        // Create default material if not assigned
        CreateDefaultMaterial();
        
        Debug.Log("SimpleFaceMeshVisualizer ready - will show triangles only");
    }
    
    private void CreateDefaultMaterial()
    {
        if (triangleMaterial == null)
        {
            triangleMaterial = new Material(Shader.Find("Standard"));
            triangleMaterial.color = triangleColor;
            
            // Make it transparent
            triangleMaterial.SetFloat("_Mode", 3); // Transparent mode
            triangleMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            triangleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            triangleMaterial.SetInt("_ZWrite", 0);
            triangleMaterial.DisableKeyword("_ALPHATEST_ON");
            triangleMaterial.EnableKeyword("_ALPHABLEND_ON");
            triangleMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            triangleMaterial.renderQueue = 3000;
        }
    }
    
    private void HandleFaceMeshResult(string result)
    {
        // Parse the face mesh data
        currentMeshData = FaceMeshData.ParseBinaryResult(result);
        
        // Show triangles
        ShowFaceMeshTriangles(currentMeshData);
    }
    
    private void ShowFaceMeshTriangles(FaceMeshData meshData)
    {
        // Clear existing mesh objects
        ClearMeshObjects();
        
        if (meshData.meshes.Count == 0)
        {
            return; // No face detected
        }
        
        // Process each detected face
        for (int i = 0; i < meshData.meshes.Count; i++)
        {
            FaceMesh faceMesh = meshData.meshes[i];
            CreateTriangleMesh(faceMesh, i);
        }
    }
    
    private void CreateTriangleMesh(FaceMesh faceMesh, int meshIndex)
    {
        if (faceMesh.points.Count == 0 || faceMesh.triangles.Count == 0)
        {
            return; // No valid mesh data
        }
        
        // Create parent object for this face mesh
        GameObject meshParent = new GameObject($"FaceMesh_{meshIndex}");
        meshParent.transform.SetParent(transform);
        meshObjects.Add(meshParent);
        
        // Transform points to Unity coordinate system
        Vector3[] transformedPoints = TransformPoints(faceMesh.GetPointsArray());
        
        // Create the triangulated mesh
        CreateUnityMesh(meshParent, transformedPoints, faceMesh.GetTriangleIndices(), meshIndex);
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
    
    private void CreateUnityMesh(GameObject parent, Vector3[] points, int[] triangles, int meshIndex)
    {
        GameObject meshObj = new GameObject($"FaceTriangles_{meshIndex}");
        meshObj.transform.SetParent(parent.transform);
        
        MeshFilter meshFilter = meshObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObj.AddComponent<MeshRenderer>();
        
        // Create Unity mesh
        Mesh mesh = new Mesh();
        mesh.name = $"FaceMesh_{meshIndex}";
        mesh.vertices = points;
        mesh.triangles = triangles;
        
        // Calculate normals for proper lighting
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        // Assign to components
        meshFilter.mesh = mesh;
        meshRenderer.material = triangleMaterial;
        
        Debug.Log($"Created face mesh with {points.Length} vertices and {triangles.Length / 3} triangles");
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
        // Update material color when changed in inspector
        if (triangleMaterial != null)
        {
            triangleMaterial.color = triangleColor;
        }
    }
}