using UnityEngine;

public class HoveringEffect : MonoBehaviour
{
    [Header("Hover Settings")]
    [Tooltip("The strength of horizontal hovering movement")]
    public float horizontalStrength = 0.1f;
    
    [Tooltip("The strength of vertical hovering movement")]
    public float verticalStrength = 0.2f;
    
    [Tooltip("Speed of the hovering movement")]
    public float hoverSpeed = 1f;
    
    [Header("Randomization")]
    [Tooltip("If true, each instance will have slightly different hover values")]
    public bool randomizeValues = true;
    
    [Tooltip("Random variation range (percentage of original values)")]
    public float randomVariation = 0.3f;
    
    [Header("Offset Settings")]
    [Tooltip("Individual horizontal offset for variation between objects")]
    public float horizontalOffset = 0f;
    
    [Tooltip("Individual vertical offset for variation between objects")]
    public float verticalOffset = 0f;

    private Vector3 startPosition;
    private float randomHoverSpeed;
    private float randomHorizontalStrength;
    private float randomVerticalStrength;

    void Start()
    {
        // Store the initial position
        startPosition = transform.position;
        
        // Apply randomization if enabled
        if (randomizeValues)
        {
            randomHoverSpeed = hoverSpeed * Random.Range(1f - randomVariation, 1f + randomVariation);
            randomHorizontalStrength = horizontalStrength * Random.Range(1f - randomVariation, 1f + randomVariation);
            randomVerticalStrength = verticalStrength * Random.Range(1f - randomVariation, 1f + randomVariation);
            
            // Randomize offsets for variation between objects
            horizontalOffset = Random.Range(0f, Mathf.PI * 2f);
            verticalOffset = Random.Range(0f, Mathf.PI * 2f);
        }
        else
        {
            randomHoverSpeed = hoverSpeed;
            randomHorizontalStrength = horizontalStrength;
            randomVerticalStrength = verticalStrength;
        }
    }

    void Update()
    {
        // Calculate hover movement using sine waves for smooth oscillation
        float horizontalHover = Mathf.Sin((Time.time * randomHoverSpeed) + horizontalOffset) * randomHorizontalStrength;
        float verticalHover = Mathf.Sin((Time.time * randomHoverSpeed * 1.5f) + verticalOffset) * randomVerticalStrength;
        
        // Apply the hover effect to the position
        transform.position = startPosition + new Vector3(horizontalHover, verticalHover, 0f);
    }

    // Method to reset to original position
    public void ResetPosition()
    {
        transform.position = startPosition;
    }

    // Method to update the start position (useful if the object moves)
    public void UpdateStartPosition()
    {
        startPosition = transform.position;
    }

    // Method to enable/disable the hover effect
    public void SetHoverEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            transform.position = startPosition;
        }
    }
}