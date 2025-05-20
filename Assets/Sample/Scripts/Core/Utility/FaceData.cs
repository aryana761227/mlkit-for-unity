using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FaceData
{
    public Rect bounds;
    public float smileProbability;
    public float leftEyeOpenProbability;
    public float rightEyeOpenProbability;
    public float upsetnessProbability;
    
    // New properties for facial landmarks
    public Dictionary<FaceLandmarkType, Vector2> landmarks = new Dictionary<FaceLandmarkType, Vector2>();
    public Dictionary<FaceContourType, List<Vector2>> contours = new Dictionary<FaceContourType, List<Vector2>>();
    
    // Enum to map landmark types to ML Kit constants
    public enum FaceLandmarkType
    {
        LeftEye = 0,
        RightEye = 1,
        LeftEar = 3,
        RightEar = 4,
        LeftCheek = 5,
        RightCheek = 6,
        NoseBase = 7,
        MouthLeft = 8,
        MouthRight = 9,
        MouthBottom = 10
    }
    
    // Enum to map contour types to ML Kit constants
    public enum FaceContourType
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
        NoseBridge = 12,
        NoseBottom = 13
    }
    
    public FaceData(Rect bounds, float smile, float leftEye, float rightEye, float upsetness)
    {
        this.bounds = bounds;
        this.smileProbability = smile;
        this.leftEyeOpenProbability = leftEye;
        this.rightEyeOpenProbability = rightEye;
        this.upsetnessProbability = upsetness;
    }
    
    // Add a landmark
    public void AddLandmark(FaceLandmarkType type, Vector2 position)
    {
        landmarks[type] = position;
    }
    
    // Add a contour
    public void AddContour(FaceContourType type, List<Vector2> points)
    {
        contours[type] = points;
    }
    
    // Helper to check if a specific landmark exists
    public bool HasLandmark(FaceLandmarkType type)
    {
        return landmarks.ContainsKey(type);
    }
    
    // Helper to check if a specific contour exists
    public bool HasContour(FaceContourType type)
    {
        return contours.ContainsKey(type);
    }
}