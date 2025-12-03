using UnityEngine;

public class DestroyScript : MonoBehaviour
{
    public static DestroyScript instance { get; private set; }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }
}
