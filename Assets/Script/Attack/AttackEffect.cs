using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void DisableCollider()
    {
        GetComponent<Collider2D>().enabled = false;
    }

    public void EnableCollider()
    {
        GetComponent<Collider2D>().enabled = true;
    }
}
