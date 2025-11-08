using UnityEngine;
using Cinemachine;

public class CameraSizeTrigger : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float targetOrthographicSize = 5f;
    [SerializeField] private float transitionSpeed = 2f;
    
    [Header("Cinemachine Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    
    private float originalOrthographicSize;
    private bool isInTrigger = false;
    
    void Start()
    {
        // If no virtual camera is assigned, try to find one
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }
        
        if (virtualCamera != null)
        {
            originalOrthographicSize = virtualCamera.m_Lens.OrthographicSize;
        }
        else
        {
            Debug.LogError("No Cinemachine Virtual Camera found! Please assign one in the inspector.");
        }
    }
    
    void Update()
    {
        if (virtualCamera == null) return;
        
        // Smoothly transition the camera size
        float targetSize = isInTrigger ? targetOrthographicSize : originalOrthographicSize;
        virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(
            virtualCamera.m_Lens.OrthographicSize, 
            targetSize, 
            transitionSpeed * Time.deltaTime
        );
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInTrigger = true;
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInTrigger = false;
        }
    }
    
    // Optional: For instant size change instead of smooth transition
    private void ChangeCameraSizeInstant(float newSize)
    {
        if (virtualCamera != null)
        {
            virtualCamera.m_Lens.OrthographicSize = newSize;
        }
    }
}