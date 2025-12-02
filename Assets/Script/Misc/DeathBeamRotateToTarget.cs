using UnityEngine;

public class DeathBeamRotateToTarget : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 2.0f;
    public float detectionRange = 10.0f;
    
    private bool targetDetected = false;
    private bool isRotationLocked = false;
    
    void Update()
    {
        if (target == null) return;
        
        float distance = Vector3.Distance(transform.position, target.position);
        
        if (distance <= detectionRange)
        {
            targetDetected = true;
        }
        
        if (targetDetected && !isRotationLocked)
        {
            Rotate();
        }
    }
    
    void Rotate()
    {
        Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle + 90f);
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
    
    public void LockRotation()
    {
        isRotationLocked = true;
    }
    
    public void UnlockRotation()
    {
        isRotationLocked = false;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = targetDetected ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
