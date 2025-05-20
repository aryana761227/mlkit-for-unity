using UnityEngine;
using System.Text;

/// <summary>
/// Simple component to log face landmarks data to the console
/// </summary>
public class FaceLandmarksLogger : MonoBehaviour
{
    [SerializeField] public FaceDetectionGameController gameController;
    
    [Header("Logging Settings")]
    [Tooltip("Log detailed information about each detected face")]
    [SerializeField] private bool logDetectionResults = true;
    
    [Tooltip("Log details about landmarks")]
    [SerializeField] private bool logLandmarks = true;
    
    [Tooltip("Log details about contours")]
    [SerializeField] private bool logContours = true;
    
    [Tooltip("Log events at this interval (seconds)")]
    [SerializeField] public float logInterval = 1.0f;
    
    private float lastLogTime = 0f;
    
    void Start()
    {
        // Find game controller if not assigned
        if (gameController == null)
        {
            gameController = FindObjectOfType<FaceDetectionGameController>();
            if (gameController == null)
            {
                Debug.LogError("No FaceDetectionGameController found! Please assign one in the inspector.");
                enabled = false;
                return;
            }
        }
        
        // Subscribe to detection events
        gameController.OnFaceDetectionResult += LogFaceDetectionResults;
        
        Debug.Log("FaceLandmarksLogger initialized and listening for detection events");
    }
    
    private void LogFaceDetectionResults(FaceDetectionData data, string rawResult)
    {
        // Check if it's time to log
        if (Time.time - lastLogTime < logInterval)
            return;
            
        lastLogTime = Time.time;
        
        if (!logDetectionResults)
            return;
            
        int faceCount = data.faces.Count;
        Debug.Log($"Detected {faceCount} faces");
        
        for (int i = 0; i < faceCount; i++)
        {
            FaceData face = data.faces[i];
            
            StringBuilder log = new StringBuilder();
            log.AppendLine($"Face {i + 1}:");
            log.AppendLine($"  Position: ({face.bounds.x:F2}, {face.bounds.y:F2}, {face.bounds.width:F2}, {face.bounds.height:F2})");
            log.AppendLine($"  Smile: {face.smileProbability:F2}");
            log.AppendLine($"  Left Eye Open: {face.leftEyeOpenProbability:F2}");
            log.AppendLine($"  Right Eye Open: {face.rightEyeOpenProbability:F2}");
            log.AppendLine($"  Upset: {face.upsetnessProbability:F2}");
            
            if (logLandmarks)
            {
                log.AppendLine($"  Landmarks: {face.landmarks.Count}");
                foreach (var landmark in face.landmarks)
                {
                        log.AppendLine($"    {landmark.Key.ToString()}: ({landmark.Value.x:F2}, {landmark.Value.y:F2})");
                }
            }
            
            if (logContours)
            {
                log.AppendLine($"  Contours: {face.contours.Count}");
                foreach (var contour in face.contours)
                {
                    log.AppendLine($"    {contour.Key.ToString()}: {contour.Value.Count} points");
                    
                    // Log first 3 points only to avoid verbose logs
                    int pointsToLog = Mathf.Min(3, contour.Value.Count);
                    for (int j = 0; j < pointsToLog; j++)
                    {
                        log.AppendLine($"      Point {j}: ({contour.Value[j].x:F2}, {contour.Value[j].y:F2})");
                    }
                    
                    if (contour.Value.Count > pointsToLog)
                    {
                        log.AppendLine($"      ... and {contour.Value.Count - pointsToLog} more points");
                    }
                }
            }
            
            Debug.Log(log.ToString());
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (gameController != null)
        {
            gameController.OnFaceDetectionResult -= LogFaceDetectionResults;
        }
    }
    
}