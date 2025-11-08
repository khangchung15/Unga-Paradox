using UnityEngine;
using Unity.Cinemachine;

public class CameraSizeTrigger : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float targetOrthographicSize = 5f;
    [SerializeField] private float transitionSpeed = 2f;
    
    [Header("Optional: Restore on Exit")]
    [SerializeField] private bool restoreOnExit = true;
    
    private float originalSize;
    private bool isTransitioning = false;
    private float currentTargetSize;

    void Start()
    {
        // Store the original orthographic size
        if (virtualCamera != null)
        {
            originalSize = virtualCamera.m_Lens.OrthographicSize;
            currentTargetSize = originalSize;
        }
        else
        {
            Debug.LogError("Virtual Camera not assigned!");
        }
    }

    void Update()
    {
        // Smoothly transition to target size
        if (isTransitioning && virtualCamera != null)
        {
            float currentSize = virtualCamera.m_Lens.OrthographicSize;
            float newSize = Mathf.Lerp(currentSize, currentTargetSize, Time.deltaTime * transitionSpeed);
            
            virtualCamera.m_Lens.OrthographicSize = newSize;
            
            // Stop transitioning when close enough
            if (Mathf.Abs(newSize - currentTargetSize) < 0.01f)
            {
                virtualCamera.m_Lens.OrthographicSize = currentTargetSize;
                isTransitioning = false;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering has a specific tag (optional)
        if (other.CompareTag("Player"))
        {
            currentTargetSize = targetOrthographicSize;
            isTransitioning = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (restoreOnExit && other.CompareTag("Player"))
        {
            currentTargetSize = originalSize;
            isTransitioning = true;
        }
    }
}