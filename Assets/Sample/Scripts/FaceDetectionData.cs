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

                        // Normalize coordinates if needed
                        Rect bounds = new Rect(left, top, width, height);

                        float smile = 0, leftEye = 0, rightEye = 0, upsetness = 0;
                        int currentIndex = System.Array.IndexOf(parts, part);

                        if (currentIndex + 1 < parts.Length && parts[currentIndex + 1].StartsWith("SMILE:"))
                            smile = float.Parse(parts[currentIndex + 1].Replace("SMILE:", ""));

                        if (currentIndex + 2 < parts.Length && parts[currentIndex + 2].StartsWith("LEFT_EYE:"))
                            leftEye = float.Parse(parts[currentIndex + 2].Replace("LEFT_EYE:", ""));

                        if (currentIndex + 3 < parts.Length && parts[currentIndex + 3].StartsWith("RIGHT_EYE:"))
                            rightEye = float.Parse(parts[currentIndex + 3].Replace("RIGHT_EYE:", ""));

                        if (currentIndex + 4 < parts.Length && parts[currentIndex + 4].StartsWith("UPSETNESS:"))
                            upsetness = float.Parse(parts[currentIndex + 4].Replace("UPSETNESS:", ""));

                        data.faces.Add(new FaceData(bounds, smile, leftEye, rightEye, upsetness));
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