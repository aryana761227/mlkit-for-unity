using System;
using System.IO;
using Sample.Scripts.Core.Utility;
using UnityEngine;

namespace Sample.Scripts.Features.Mesh
{
    public class FaceMeshPointDetectionController : MonoBehaviour
    {
        public FaceMeshPositionsData LastDetectionData = new FaceMeshPositionsData();
        private bool initialized = false;
        private MLKitManager mlkitManager;

        private void Start()
        {
            mlkitManager = MLKitManager.Instance;
            if (mlkitManager == null)
            {
                GameObject mlkitGO = new GameObject("MLKitManager");
                mlkitManager = mlkitGO.AddComponent<MLKitManager>();
            }
        
            // Subscribe to MLKit events
            mlkitManager.OnCameraInitializedComplete += HandleCameraInitialized;
            // Start camera
            ConfigureMlkit();
            MLKitManager.Instance.OnFaceMeshPointsDetectionComplete += OnFaceMeshPointsDetectionReceivedFromAndroid;
        }

        private void HandleCameraInitialized(string result)
        {
            throw new NotImplementedException();
        }

        private void ConfigureMlkit()
        {
            Debug.Log("MLKitManager.StartCamera called");
            mlkitManager.StartCamera();
            mlkitManager.EnableFaceMeshPointsDetector(true);
            mlkitManager.EnableFaceMeshTrianglesDetector(false);
            mlkitManager.EnableLightWeightFaceDetector(false);
        }

        private void OnFaceMeshPointsDetectionReceivedFromAndroid(string result)
        {
            byte[] bytes = Convert.FromBase64String(result);
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);
            int pointCount = reader.ReadInt32();
            if (!initialized)
            {
                Initialize(pointCount);
            }
            Debug.Log($"Received {pointCount} face mesh points");
            for (int i = 0; i < pointCount; i++)
            {
                int index = reader.ReadInt32();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                LastDetectionData.Indices[i] = index;
                LastDetectionData.Points[i] = new Vector3(x, y, z);
            }
        }

        private void Initialize(int pointCount)
        {
            initialized = true;
            LastDetectionData.Indices =  new int[pointCount];
            LastDetectionData.Points = new Vector3[pointCount];
        }
    }
}