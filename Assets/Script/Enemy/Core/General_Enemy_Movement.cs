using UnityEngine;

public class General_Enemy_Movement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody2D rb;
    private Transform parentTransform;

    public void Initialized(Rigidbody2D rb, Transform parentTransform)
    {
        this.rb = rb;
        this.parentTransform = parentTransform;
    }

    public void MoveTowards(Vector2 target, float moveSpeed)
    {
        Vector2 direction = (target - (Vector2)parentTransform.position).normalized;

        // Only flip if moving horizontally and not at the target
        if (Mathf.Abs(direction.x) > 0.01f)
        {
            if (direction.x > 0 && parentTransform.localScale.x > 0)
            {
                parentTransform.localScale = new Vector3(-Mathf.Abs(parentTransform.localScale.x), parentTransform.localScale.y, parentTransform.localScale.z);
            }
            else if (direction.x < 0 && parentTransform.localScale.x < 0)
            {
                parentTransform.localScale = new Vector3(Mathf.Abs(parentTransform.localScale.x), parentTransform.localScale.y, parentTransform.localScale.z);
            }
        }

            rb.linearVelocity = direction * moveSpeed;
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }
}
