using UnityEngine;

[System.Serializable]
public class FaceData
{
    public Rect bounds;
    public float smileProbability;
    public float leftEyeOpenProbability;
    public float rightEyeOpenProbability;
    public float upsetnessProbability;
    
    public FaceData(Rect bounds, float smile, float leftEye, float rightEye, float upsetness)
    {
        this.bounds = bounds;
        this.smileProbability = smile;
        this.leftEyeOpenProbability = leftEye;
        this.rightEyeOpenProbability = rightEye;
        this.upsetnessProbability = upsetness;
    }
}