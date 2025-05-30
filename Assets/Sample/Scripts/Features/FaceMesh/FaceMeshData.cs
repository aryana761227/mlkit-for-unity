using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class FaceMeshData
{
    public int meshId;
    public Rect bounds;
    public List<Vector3> points = new List<Vector3>();
    public List<int> triangleIndices = new List<int>();
    public Dictionary<FaceMeshContourType, List<int>> contours = new Dictionary<FaceMeshContourType, List<int>>();
    
    // Enum to map contour types to ML Kit constants
    public enum FaceMeshContourType
    {
        Face = 1,
        LeftEyebrowTop = 2,
        LeftEyebrowBottom = 3,
        RightEyebrowTop = 4,
        RightEyebrowBottom = 5,
        LeftEye = 6,
        RightEye = 7,
        UpperLipTop = 8,
        UpperLipBottom = 9,
        LowerLipTop = 10,
        LowerLipBottom = 11,
        NoseBridge = 12
    }
    
    public FaceMeshData(int id, Rect bounds)
    {
        this.meshId = id;
        this.bounds = bounds;
    }
    
    // Convert triangle indices to Unity-friendly triangle array
    public int[] GetTriangles()
    {
        return triangleIndices.ToArray();
    }
    
    // Get contour points as Vector3 array
    public Vector3[] GetContourPoints(FaceMeshContourType type)
    {
        if (contours.ContainsKey(type))
        {
            List<Vector3> contourPoints = new List<Vector3>();
            foreach (int index in contours[type])
            {
                if (index >= 0 && index < points.Count)
                {
                    contourPoints.Add(points[index]);
                }
            }
            return contourPoints.ToArray();
        }
        return new Vector3[0];
    }
}

public class FaceMeshDetectionData
{
    public List<FaceMeshData> meshes = new List<FaceMeshData>();
    
    public static FaceMeshDetectionData ParseBinaryResult(string result)
    {
        FaceMeshDetectionData data = new FaceMeshDetectionData();
        
        if (string.IsNullOrEmpty(result))
        {
            Debug.LogError("Face mesh result is empty");
            return data;
        }
        
        if (result.StartsWith("ERROR"))
        {
            Debug.LogError("Face mesh detection error: " + result);
            return data;
        }
        
        if (!result.StartsWith("BINARY:"))
        {
            Debug.LogError("Unexpected face mesh result format: " + result);
            return data;
        }
        
        try
        {
            // Extract base64 data
            string base64Data = result.Substring(7); // Remove "BINARY:" prefix
            byte[] binaryData = Convert.FromBase64String(base64Data);
            
            // Parse binary data
            int offset = 0;
            
            // Read mesh count
            int meshCount = BitConverter.ToInt32(binaryData, offset);
            offset += 4;
            
            Debug.Log($"Parsing {meshCount} face meshes from binary data");
            
            for (int m = 0; m < meshCount; m++)
            {
                // Read mesh ID
                int meshId = BitConverter.ToInt32(binaryData, offset);
                offset += 4;
                
                // Read bounds (left, top, width, height)
                float left = BitConverter.ToSingle(binaryData, offset);
                offset += 4;
                float top = BitConverter.ToSingle(binaryData, offset);
                offset += 4;
                float width = BitConverter.ToSingle(binaryData, offset);
                offset += 4;
                float height = BitConverter.ToSingle(binaryData, offset);
                offset += 4;
                
                Rect bounds = new Rect(left, top, width, height);
                FaceMeshData mesh = new FaceMeshData(meshId, bounds);
                
                // Read points
                int pointCount = BitConverter.ToInt32(binaryData, offset);
                offset += 4;
                
                Debug.Log($"Mesh {meshId}: Reading {pointCount} points");
                
                for (int i = 0; i < pointCount; i++)
                {
                    float x = BitConverter.ToSingle(binaryData, offset);
                    offset += 4;
                    float y = BitConverter.ToSingle(binaryData, offset);
                    offset += 4;
                    float z = BitConverter.ToSingle(binaryData, offset);
                    offset += 4;
                    
                    // Note: We may need to adjust coordinate system
                    // ML Kit uses screen coordinates, Unity uses different coordinate system
                    mesh.points.Add(new Vector3(x, y, z));
                }
                
                // Read triangles
                int triangleCount = BitConverter.ToInt32(binaryData, offset);
                offset += 4;
                
                Debug.Log($"Mesh {meshId}: Reading {triangleCount} triangles");
                
                for (int i = 0; i < triangleCount; i++)
                {
                    int idx1 = BitConverter.ToInt32(binaryData, offset);
                    offset += 4;
                    int idx2 = BitConverter.ToInt32(binaryData, offset);
                    offset += 4;
                    int idx3 = BitConverter.ToInt32(binaryData, offset);
                    offset += 4;
                    
                    // Add triangle indices
                    mesh.triangleIndices.Add(idx1);
                    mesh.triangleIndices.Add(idx2);
                    mesh.triangleIndices.Add(idx3);
                }
                
                // Read contours
                int contourCount = BitConverter.ToInt32(binaryData, offset);
                offset += 4;
                
                Debug.Log($"Mesh {meshId}: Reading {contourCount} contours");
                
                for (int c = 0; c < contourCount; c++)
                {
                    int contourType = BitConverter.ToInt32(binaryData, offset);
                    offset += 4;
                    int contourPointCount = BitConverter.ToInt32(binaryData, offset);
                    offset += 4;
                    
                    List<int> contourIndices = new List<int>();
                    for (int i = 0; i < contourPointCount; i++)
                    {
                        int pointIndex = BitConverter.ToInt32(binaryData, offset);
                        offset += 4;
                        contourIndices.Add(pointIndex);
                    }
                    
                    if (Enum.IsDefined(typeof(FaceMeshData.FaceMeshContourType), contourType))
                    {
                        mesh.contours[(FaceMeshData.FaceMeshContourType)contourType] = contourIndices;
                    }
                }
                
                data.meshes.Add(mesh);
            }
            
            Debug.Log($"Successfully parsed {data.meshes.Count} face meshes");
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing face mesh binary data: " + e.Message + "\n" + e.StackTrace);
        }
        
        return data;
    }
}