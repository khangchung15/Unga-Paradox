using UnityEngine;

public class PivotRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 90f; // Degrees per second
    public float minAngle = -45f;     // Minimum rotation angle
    public float maxAngle = 45f;      // Maximum rotation angle
    public bool startFromMin = true;  // Start from min angle or max angle
    
    [Header("Object References")]
    public Transform pivotObject;     // The object to rotate around
    
    private float currentAngle;
    private bool rotatingForward = true;
    private Vector3 pivotOffset;      // Offset from rotating object's center to the pivot end
    
    void Start()
    {
        // Calculate the offset from this object's center to the pivot point
        if (pivotObject != null)
        {
            pivotOffset = transform.position - pivotObject.position;
        }
        
        // Set initial rotation
        if (startFromMin)
        {
            currentAngle = minAngle;
            rotatingForward = true;
        }
        else
        {
            currentAngle = maxAngle;
            rotatingForward = false;
        }
        
        ApplyRotation();
    }
    
    void Update()
    {
        // Calculate rotation direction
        float direction = rotatingForward ? 1f : -1f;
        
        // Update angle
        currentAngle += rotationSpeed * direction * Time.deltaTime;
        
        // Check if we need to reverse direction
        if (rotatingForward && currentAngle >= maxAngle)
        {
            currentAngle = maxAngle;
            rotatingForward = false;
        }
        else if (!rotatingForward && currentAngle <= minAngle)
        {
            currentAngle = minAngle;
            rotatingForward = true;
        }
        
        // Apply the rotation
        ApplyRotation();
    }
    
    void ApplyRotation()
    {
        if (pivotObject != null)
        {
            // Create rotation quaternion
            Quaternion rotation = Quaternion.Euler(0f, 0f, currentAngle);
            
            // Rotate the offset vector
            Vector3 rotatedOffset = rotation * pivotOffset;
            
            // Set position and rotation
            transform.position = pivotObject.position + rotatedOffset;
            transform.rotation = rotation;
        }
    }
    
    // Method to set which end should be the pivot (call this before Start if needed)
    public void SetPivotToEnd(bool rightEnd = true)
    {
        if (pivotObject != null)
        {
            // Calculate offset based on which end should pivot
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                float halfWidth = sprite.bounds.extents.x;
                pivotOffset = rightEnd ? new Vector3(-halfWidth, 0, 0) : new Vector3(halfWidth, 0, 0);
            }
            else
            {
                // Fallback using local scale
                float halfWidth = transform.localScale.x / 2f;
                pivotOffset = rightEnd ? new Vector3(-halfWidth, 0, 0) : new Vector3(halfWidth, 0, 0);
            }
        }
    }
}