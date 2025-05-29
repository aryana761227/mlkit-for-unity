using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class FaceMeshData
{
    public List<FaceMesh> meshes = new List<FaceMesh>();

    public static FaceMeshData ParseBinaryResult(string result)
    {
        FaceMeshData data = new FaceMeshData();

        if (string.IsNullOrEmpty(result) || result.StartsWith("ERROR"))
        {
            Debug.LogError("Face mesh detection error: " + result);
            return data;
        }

        if (!result.StartsWith("BINARY:"))
        {
            Debug.LogError("Expected binary face mesh data");
            return data;
        }

        try
        {
            // Remove "BINARY:" prefix and decode base64
            string base64Data = result.Substring(7);
            byte[] binaryData = Convert.FromBase64String(base64Data);
            
            // Parse binary data
            data = ParseBinaryData(binaryData);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing face mesh binary result: " + e.Message);
        }

        return data;
    }

    private static FaceMeshData ParseBinaryData(byte[] data)
    {
        FaceMeshData meshData = new FaceMeshData();
        int offset = 0;

        if (data.Length < 4) return meshData;

        // Read mesh count
        int meshCount = BitConverter.ToInt32(data, offset);
        offset += 4;

        Debug.Log($"Parsing {meshCount} face meshes from binary data");

        for (int i = 0; i < meshCount; i++)
        {
            FaceMesh mesh = new FaceMesh();
            
            // Read mesh ID
            mesh.meshId = BitConverter.ToInt32(data, offset);
            offset += 4;

            // Read bounds (left, top, width, height)
            mesh.bounds = new Rect(
                BitConverter.ToSingle(data, offset),     // left
                BitConverter.ToSingle(data, offset + 4), // top
                BitConverter.ToSingle(data, offset + 8), // width
                BitConverter.ToSingle(data, offset + 12) // height
            );
            offset += 16;

            // Read points
            int pointCount = BitConverter.ToInt32(data, offset);
            offset += 4;

            for (int j = 0; j < pointCount; j++)
            {
                Vector3 point = new Vector3(
                    BitConverter.ToSingle(data, offset),     // x
                    BitConverter.ToSingle(data, offset + 4), // y
                    BitConverter.ToSingle(data, offset + 8)  // z
                );
                mesh.points.Add(point);
                offset += 12;
            }

            // Read triangles
            int triangleCount = BitConverter.ToInt32(data, offset);
            offset += 4;

            for (int j = 0; j < triangleCount; j++)
            {
                FaceTriangle triangle = new FaceTriangle(
                    BitConverter.ToInt32(data, offset),     // vertex 1 index
                    BitConverter.ToInt32(data, offset + 4), // vertex 2 index
                    BitConverter.ToInt32(data, offset + 8)  // vertex 3 index
                );
                mesh.triangles.Add(triangle);
                offset += 12;
            }

            // Read contours
            int contourCount = BitConverter.ToInt32(data, offset);
            offset += 4;

            for (int j = 0; j < contourCount; j++)
            {
                int contourType = BitConverter.ToInt32(data, offset);
                offset += 4;

                int contourPointCount = BitConverter.ToInt32(data, offset);
                offset += 4;

                List<int> contourIndices = new List<int>();
                for (int k = 0; k < contourPointCount; k++)
                {
                    contourIndices.Add(BitConverter.ToInt32(data, offset));
                    offset += 4;
                }

                mesh.contourIndices[contourType] = contourIndices;
            }

            meshData.meshes.Add(mesh);
            
            Debug.Log($"Parsed mesh {i}: {pointCount} points, {triangleCount} triangles, {contourCount} contours");
        }

        return meshData;
    }
}

[System.Serializable]
public class FaceMesh
{
    public int meshId;
    public Rect bounds;
    public List<Vector3> points = new List<Vector3>();
    public List<FaceTriangle> triangles = new List<FaceTriangle>();
    public Dictionary<int, List<int>> contourIndices = new Dictionary<int, List<int>>();
    
    // Helper methods
    public Vector3[] GetPointsArray()
    {
        return points.ToArray();
    }
    
    public int[] GetTriangleIndices()
    {
        List<int> indices = new List<int>();
        foreach (var triangle in triangles)
        {
            indices.Add(triangle.vertex1);
            indices.Add(triangle.vertex2);
            indices.Add(triangle.vertex3);
        }
        return indices.ToArray();
    }
    
    public List<Vector3> GetContourPoints(int contourType)
    {
        List<Vector3> contourPoints = new List<Vector3>();
        if (contourIndices.ContainsKey(contourType))
        {
            foreach (int index in contourIndices[contourType])
            {
                if (index >= 0 && index < points.Count)
                {
                    contourPoints.Add(points[index]);
                }
            }
        }
        return contourPoints;
    }
}

[System.Serializable]
public class FaceTriangle
{
    public int vertex1;
    public int vertex2;
    public int vertex3;
    
    public FaceTriangle(int v1, int v2, int v3)
    {
        vertex1 = v1;
        vertex2 = v2;
        vertex3 = v3;
    }
    
    public int[] GetIndices()
    {
        return new int[] { vertex1, vertex2, vertex3 };
    }
}