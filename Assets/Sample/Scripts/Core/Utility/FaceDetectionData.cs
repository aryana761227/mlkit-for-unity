using UnityEngine;
using System.Collections.Generic;

public class FaceDetectionData
{
    public List<FaceData> faces = new List<FaceData>();

    public static FaceDetectionData ParseResult(string result)
    {
        FaceDetectionData data = new FaceDetectionData();

        if (string.IsNullOrEmpty(result) || result.StartsWith("ERROR"))
        {
            Debug.LogError("Face detection error: " + result);
            return data;
        }

        try
        {
            string[] parts = result.Split('|');
            int faceCount = 0;
            Dictionary<int, FaceData> faceDict = new Dictionary<int, FaceData>();

            // First pass: create all face objects
            foreach (string part in parts)
            {
                if (part.StartsWith("FACES_COUNT:"))
                {
                    faceCount = int.Parse(part.Replace("FACES_COUNT:", ""));
                }
                else if (part.StartsWith("FACE:"))
                {
                    string[] coords = part.Replace("FACE:", "").Split(',');
                    if (coords.Length >= 4)
                    {
                        // Get the raw values for position
                        float left = float.Parse(coords[0]);
                        float top = float.Parse(coords[1]);
                        float width = float.Parse(coords[2]);
                        float height = float.Parse(coords[3]);

                        // Create bounds
                        Rect bounds = new Rect(left, top, width, height);

                        float smile = 0, leftEye = 0, rightEye = 0, upsetness = 0;
                        int currentIndex = System.Array.IndexOf(parts, part);
                        int faceIndex = data.faces.Count;

                        if (currentIndex + 1 < parts.Length && parts[currentIndex + 1].StartsWith("SMILE:"))
                            smile = float.Parse(parts[currentIndex + 1].Replace("SMILE:", ""));

                        if (currentIndex + 2 < parts.Length && parts[currentIndex + 2].StartsWith("LEFT_EYE:"))
                            leftEye = float.Parse(parts[currentIndex + 2].Replace("LEFT_EYE:", ""));

                        if (currentIndex + 3 < parts.Length && parts[currentIndex + 3].StartsWith("RIGHT_EYE:"))
                            rightEye = float.Parse(parts[currentIndex + 3].Replace("RIGHT_EYE:", ""));

                        if (currentIndex + 4 < parts.Length && parts[currentIndex + 4].StartsWith("UPSETNESS:"))
                            upsetness = float.Parse(parts[currentIndex + 4].Replace("UPSETNESS:", ""));

                        FaceData faceData = new FaceData(bounds, smile, leftEye, rightEye, upsetness);
                        data.faces.Add(faceData);
                        faceDict[faceIndex] = faceData;
                    }
                }
            }

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].StartsWith("LANDMARK:"))
                {
                    string[] landmarkData = parts[i].Replace("LANDMARK:", "").Split(',');
                    if (landmarkData.Length >= 4)
                    {
                        int faceIndex = int.Parse(landmarkData[0]);
                        int landmarkType = int.Parse(landmarkData[1]);
                        float x = float.Parse(landmarkData[2]);
                        float y = float.Parse(landmarkData[3]);

                        if (faceDict.ContainsKey(faceIndex))
                        {
                            faceDict[faceIndex].AddLandmark((FaceData.FaceLandmarkType)landmarkType, new Vector2(x, y));
                        }
                    }
                }
                else if (parts[i].StartsWith("CONTOUR_START:"))
                {
                    // Format: "CONTOUR_START:faceIndex,type,pointCount"
                    string[] contourStart = parts[i].Replace("CONTOUR_START:", "").Split(',');
                    if (contourStart.Length >= 3)
                    {
                        int faceIndex = int.Parse(contourStart[0]);
                        int contourType = int.Parse(contourStart[1]);
                        int pointCount = int.Parse(contourStart[2]);
                        
                        List<Vector2> contourPoints = new List<Vector2>();
                        
                        // Read the next pointCount entries as contour points
                        for (int j = 1; j <= pointCount && (i + j) < parts.Length; j++)
                        {
                            if (parts[i + j].StartsWith("CONTOUR_POINT:"))
                            {
                                string[] pointData = parts[i + j].Replace("CONTOUR_POINT:", "").Split(',');
                                if (pointData.Length >= 2)
                                {
                                    float x = float.Parse(pointData[0]);
                                    float y = float.Parse(pointData[1]);
                                    contourPoints.Add(new Vector2(x, y));
                                }
                            }
                        }
                        
                        if (faceDict.ContainsKey(faceIndex) && contourPoints.Count > 0)
                        {
                            faceDict[faceIndex].AddContour((FaceData.FaceContourType)contourType, contourPoints);
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing face detection result: " + e.Message);
        }

        return data;
    }
}