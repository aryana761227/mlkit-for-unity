using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;

namespace Sample.Scripts.Core.Utility
{
    public class CameraPermissionHandler
    {
        private static CameraPermissionHandler _instance;
        public static CameraPermissionHandler Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CameraPermissionHandler();
                return _instance;
            }
        }

        /// <summary>
        /// Request camera permission and return result via callback
        /// </summary>
        /// <param name="callback">Callback with bool result - true if granted, false if denied</param>
        public void RequestPermission(Action<bool> callback)
        {
            if (callback == null)
            {
                Debug.LogError("CameraPermissionHandler: Callback cannot be null");
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            
            // Check if permission is already granted
            if (Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.Log("CameraPermissionHandler: Camera permission already granted");
                callback(true);
                return;
            }

            // Start coroutine to handle permission request
            CoroutineHelper.Instance.StartCoroutine(RequestPermissionCoroutine(callback));
            
#elif UNITY_EDITOR
            
            // In editor, always return true for testing
            Debug.Log("CameraPermissionHandler: Running in editor, permission granted");
            callback(true);
            
#else
            // On other platforms, assume permission is granted
            Debug.Log("CameraPermissionHandler: Non-Android platform, permission assumed granted");
            callback(true);
            
#endif
        }

        /// <summary>
        /// Check if camera permission is currently granted
        /// </summary>
        /// <returns>True if permission is granted, false otherwise</returns>
        public bool HasPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Permission.HasUserAuthorizedPermission(Permission.Camera);
#else
            return true; // Assume granted on non-Android platforms
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        
        private IEnumerator RequestPermissionCoroutine(Action<bool> callback)
        {
            Debug.Log("CameraPermissionHandler: Requesting camera permission...");
            
            // Request the permission
            Permission.RequestUserPermission(Permission.Camera);
            
            // Wait a short time for the dialog to appear
            yield return new WaitForSeconds(0.5f);
            
            // Poll for permission result with timeout
            float timeout = 30f; // 30 second timeout
            float elapsed = 0f;
            bool dialogClosed = false;
            
            while (elapsed < timeout && !dialogClosed)
            {
                // Check if permission was granted
                if (Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    Debug.Log("CameraPermissionHandler: Camera permission granted");
                    callback(true);
                    yield break;
                }
                
                // Wait and increment timer
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
                
                // Simple heuristic: if we've waited more than 2 seconds and still no permission,
                // and the application is focused, assume user denied or dialog closed
                if (elapsed > 2f && Application.isFocused)
                {
                    // Additional check: try requesting again to see if dialog is still active
                    // If permission is still not granted after reasonable time, assume denial
                    if (elapsed > 5f)
                    {
                        dialogClosed = true;
                    }
                }
            }
            
            // If we reach here, either timeout occurred or permission was denied
            Debug.Log("CameraPermissionHandler: Camera permission denied or timeout");
            callback(false);
        }
        
#endif
    }

    /// <summary>
    /// Helper class to run coroutines for non-MonoBehaviour classes
    /// </summary>
    internal class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper _instance;
        
        public static CoroutineHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CoroutineHelper");
                    _instance = go.AddComponent<CoroutineHelper>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
    }
}