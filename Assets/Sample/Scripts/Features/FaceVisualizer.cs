using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class FaceVisualizer : MonoBehaviour
{
    [SerializeField] public FaceDetectionGameController gameController;
    [SerializeField] public GameObject landMarkPrefab;
    [SerializeField] public GameObject contourPrefab;
    [SerializeField] private float multiplier;
    
    private List<Transform> landmarkInstances;
    private List<Transform> contourInstances;

    private bool initialized = false;

    void Update()
    {
        if (gameController == null || gameController.LastDetectionData == null)
            return;
        if (gameController.LastDetectionData.faces == null || gameController.LastDetectionData.faces.Count == 0)
        {
            return;
        }
        var firstFace = gameController.LastDetectionData.faces.First();
        if (firstFace == null)
            return;
        if (!initialized)
        {
            initialized = true;
            landmarkInstances = new List<Transform>();
            contourInstances = new List<Transform>();
            for (int i = 0; i < firstFace.landmarks.Count; i++)
            {
                landmarkInstances.Add(Instantiate(landMarkPrefab, transform).transform);
            }
            foreach (var (_, vector2s) in firstFace.contours)
            {   
                foreach (var _ in vector2s)
                {
                    contourInstances.Add(Instantiate(contourPrefab, transform).transform);
                }
            }
        }
        DrawFace(firstFace);
    }

    void DrawFace(FaceData faceData)
    {
        int index = 0;
        foreach (var landmarkPos in faceData.landmarks.Values)
        {
            landmarkInstances[index].position = new Vector3(-landmarkPos.x * multiplier, -landmarkPos.y * multiplier, 0);
            landmarkInstances[index].gameObject.SetActive(true);
            index++;
        }
        index = 0;
        foreach (var contour in faceData.contours)
        {
            foreach (var vector2 in contour.Value)
            {
                contourInstances[index].position = new Vector3(-vector2.x * multiplier, -vector2.y * multiplier, 0);
                contourInstances[index].gameObject.SetActive(true);
                index++;
            }
        }
    }
}
