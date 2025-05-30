using System;
using System.Collections.Generic;
using System.Linq;
using Sample.Scripts.Core.Utility;
using Sample.Scripts.Features.Mesh;
using UnityEditor;
using UnityEngine;

namespace Sample.Scripts.Features
{
    public class Face3dPointVisualizer : MonoBehaviour
    {
        [SerializeField] public FaceMeshPointDetectionController gameController;
        [SerializeField] public GameObject pointPrefab;
        [SerializeField] public GameObject pointPrefab2;
        [SerializeField] private float multiplier;
        private Transform[] points;
        private bool initialized = false;

        void Update()
        {
            var facePoints = gameController.LastDetectionData;

            if (!initialized)
            {
                if (facePoints.Points is { Length: > 0 })
                {
                    Initialize(facePoints);
                }
                else
                {
                    // Still waiting for detection data
                    return;
                }
            }

            if (facePoints.Points == null || facePoints.Points.Length == 0)
            {
                return;
            }

            DrawFace(facePoints);
        }

        private void Initialize(FaceMeshPositionsData facePoints)
        {
            if (facePoints == null)
            {
                Debug.LogError("FaceMeshPositionsData is null!");
                return;
            }

            if (facePoints.Points == null || facePoints.Points.Length == 0)
            {
                Debug.LogError("FaceMeshPositionsData.positions is null or empty!");
                return;
            }
            initialized = true;
            points = new Transform[facePoints.Points.Length];
            for (int i = 0; i < facePoints.Points.Length; i++)
            {
                var rand = UnityEngine.Random.Range(0, 2);
                if (rand == 0)
                { 
                    points[i] = Instantiate(pointPrefab, transform).transform;
                }
                else
                {
                    points[i] = Instantiate(pointPrefab2, transform).transform;
                }
            }
        }

        private void DrawFace(FaceMeshPositionsData facePoints)
        {
            for (var index = 0; index < facePoints.Points.Length; index++)
            {
                var pos3D = facePoints.Points[index];
                points[index].position =
                    new Vector3(-pos3D.x * multiplier, -pos3D.y * multiplier, -pos3D.z * multiplier);
                points[index].gameObject.SetActive(true);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Create Points")]
        public void CreatePoints()
        {
            try
            {
                DeletePoints();
            }
            catch (Exception e)
            {
                //
            }
            var txtFile =
                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Sample/SampleData/face_points.txt");
            var text = txtFile.text;
            var pointsRaw = text.Split('\n');
            List<Vector3> pointsToCreate = new List<Vector3>();
            for (int i = 0; i < pointsRaw.Length; i++)
            {
                try
                {
                    var onlyVector = pointsRaw[i].Split(':')[1];
                    var substring = onlyVector.Substring(1, onlyVector.Length - 2);
                    var strings = substring.Split(',');
                    var pointRaw = strings.Select(float.Parse).ToList();
                    var position = multiplier * new Vector3(pointRaw[0], pointRaw[1], pointRaw[2]);
                    pointsToCreate.Add( position);
                }
                catch (Exception e)
                {
                    var exception = new Exception($"<<i: {i}>>", e);
                    Debug.LogException(exception);
                }
            }
            foreach (var vector3 in pointsToCreate)
            {
                Instantiate(pointPrefab, transform).transform.position = vector3;
            }
        }

        [ContextMenu("Delete Points")]
        public void DeletePoints()
        {
            List<GameObject> gameObjects = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++)
            {
                gameObjects.Add(transform.GetChild(i).gameObject);
            }
            foreach (var o in gameObjects)
            {
                DestroyImmediate(o);
            }
        }
#endif
    }
}