using UnityEngine;
using UnityEngine.SceneManagement;
using Sample.Scripts.Core.Utility;

/// <summary>
/// Manages the intro scene - handles camera permission and scene transition
/// </summary>
public class IntroSceneManager : MonoBehaviour
{
    private const string SampleScenesMlkitFaceDetectionDemo = "Sample/Scenes/MLKitFaceMeshDetectionDemo";
    [Tooltip("Delay before requesting permission (in seconds)")]
    public float requestDelay = 1f;
    
    void Start()
    {
        Debug.Log("IntroSceneManager: Starting camera permission flow");
        
        // Wait a moment then request permission
        Invoke(nameof(RequestCameraPermission), requestDelay);
    }
    
    void RequestCameraPermission()
    {
        Debug.Log("IntroSceneManager: Requesting camera permission");
        CameraPermissionHandler.Instance.RequestPermission(OnPermissionResult);
    }
    
    void OnPermissionResult(bool granted)
    {
        if (granted)
        {
            Debug.Log("IntroSceneManager: Camera permission granted - loading next scene");
            LoadNextScene();
        }
        else
        {
            Debug.Log("IntroSceneManager: Camera permission denied - exiting application");
            ExitApplication();
        }
    }
    
    void LoadNextScene()
    {
        Debug.Log($"IntroSceneManager: Loading scene: {SampleScenesMlkitFaceDetectionDemo}");
        SceneManager.LoadScene(SampleScenesMlkitFaceDetectionDemo);
    }
    
    void ExitApplication()
    {
        Debug.Log("IntroSceneManager: Exiting application due to permission denial");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}