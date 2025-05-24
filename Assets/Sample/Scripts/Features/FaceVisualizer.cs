using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FaceVisualizer : MonoBehaviour
{
    [SerializeField] public FaceDetectionGameController gameController;
    [SerializeField] public GameObject landMarkPrefab;
    [SerializeField] public GameObject contourPrefab;

    private Transform[] landmarkInstances;
    private Transform[] contourInstances;

    private bool initialized = false;

    void Update()
    {

        if (gameController == null || gameController.LastDetectionData == null)
            return;

        var firstFace = gameController.LastDetectionData.faces.First();
        if (firstFace == null)
            return;
        if (!initialized)
        {
            initialized = true;
            landmarkInstances = new Transform[firstFace.landmarks.Count];
            contourInstances = new Transform[firstFace.contours.Count];
            for (int i = 0; i < firstFace.landmarks.Count; i++)
            {
                landmarkInstances[i] = Instantiate(landMarkPrefab).transform;
            }
            for (int i = 0; i < firstFace.contours.Count; i++)
            {
                contourInstances[i] = Instantiate(contourPrefab).transform;
            }
        }
        DrawFace(firstFace);
    }

    void DrawFace(FaceData faceData)
    {
        // Draw landmarks
        if (faceData.landmarks != null)
        {

            int index = 0;
            foreach (var landmarkPos in faceData.landmarks.Values)
            {
                if (index >= 0 && index < landmarkInstances.Length)
                {
                    landmarkInstances[index].position = new Vector3(landmarkPos.x, landmarkPos.y, 0);
                    landmarkInstances[index].gameObject.SetActive(true);
                }
                index++;
            }
        }

        // Draw contours
        if (faceData.contours != null)
        {
            foreach (var contour in faceData.contours)
            {
                var index = 0;
                foreach (var contourPos in contour.Value)
                {
                    if (index >= 0 && index < contourInstances.Length)
                    {
                        contourInstances[index].position = new Vector3(contourPos.x, contourPos.y, 0);
                        contourInstances[index].gameObject.SetActive(true);
                    }
                    index++;
                }
            }
        }
    }
}
