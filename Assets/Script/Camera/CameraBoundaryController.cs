using UnityEngine;
using Unity.Cinemachine;

public class CameraBoundaryController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    private CinemachineConfiner2D confiner;
    
    void Awake()
    {
        if (cinemachineCamera == null)
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        }
        
        if (cinemachineCamera != null)
        {
            confiner = cinemachineCamera.GetComponent<CinemachineConfiner2D>();
        }
    }
    
    public void DisableBoundary()
    {
        if (confiner != null)
        {
            confiner.enabled = false;
        }
    }
    
    public void EnableBoundary()
    {
        if (confiner != null)
        {
            confiner.enabled = true;
        }
    }
}
