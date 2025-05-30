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

        private void Awake()
        {
            mlkitManager = MLKitManager.Instance;
            if (mlkitManager == null)
            {
                GameObject mlkitGO = new GameObject("MLKitManager");
                mlkitManager = mlkitGO.AddComponent<MLKitManager>();
            }
            ConfigureMlkit();
            MLKitManager.Instance.OnFaceMeshPointsDetectionComplete += OnFaceMeshPointsDetectionReceivedFromAndroid;
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
            if (string.IsNullOrEmpty(result))
            {
                Debug.LogError("Face mesh result string is null or empty.");
                return;
            }
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"Base64 decoding failed: {e.Message}");
                return;
            }

            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);

            if (bytes.Length < 4)
            {
                Debug.LogError("Byte array too small to contain point count.");
                return;
            }

            int pointCount = reader.ReadInt32();

            if (!initialized)
            {
                Initialize(pointCount);
            }

            Debug.Log($"[Unity] Received {pointCount} face mesh points");

            int requiredSize = 4 + pointCount * (4 + 4 * 3); // int + index(int) + x/y/z(float)
            if (bytes.Length < requiredSize)
            {
                Debug.LogError($"Expected {requiredSize} bytes but received {bytes.Length}. Aborting.");
                return;
            }

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