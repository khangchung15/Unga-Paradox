using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boundsCollider;
    
    private void Awake()
    {
        // Auto-get if not set
        if (boundsCollider == null)
            boundsCollider = GetComponent<BoxCollider2D>();
    }

    public BoxCollider2D GetBoundsCollider() => boundsCollider;
}