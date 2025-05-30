using Sample.Scripts.Core.Utility;
using Sample.Scripts.Features.Mesh;
using UnityEngine;

namespace Sample.Scripts.Features
{
    public class Face3dPointVisualizer : MonoBehaviour
    {
        [SerializeField] public FaceMeshPointDetectionController gameController;
        [SerializeField] public GameObject pointPrefab;
        [SerializeField] private float multiplier;
        private Transform[] points;
        private bool initialized = false;

        void Update()
        {
            var facePoints = gameController.LastDetectionData;
            if (!initialized)
            {
                Initialize(facePoints);
            }
            DrawFace(facePoints);
        }

        private void Initialize(FaceMeshPositionsData facePoints)
        {
            initialized = true;
            points = new Transform[facePoints.Points.Length];
            for (int i = 0; i < facePoints.Points.Length; i++)
            {
                points[i] = Instantiate(pointPrefab, transform).transform;
            }
        }

        private void DrawFace(FaceMeshPositionsData facePoints)
        {
            for (var index = 0; index < facePoints.Points.Length; index++)
            {
                var pos3D = facePoints.Points[index];
                points[index].position =
                    new Vector3(-pos3D.x * multiplier, -pos3D.y * multiplier, 0);
                points[index].gameObject.SetActive(true);
            }
        }
    }
}