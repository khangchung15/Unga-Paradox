using UnityEngine;

public class DontDestroyScript : MonoBehaviour
{
    public static DontDestroyScript instance { get; private set; }
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        if (GameObject.Find("Scientist") == null)
            DontDestroyOnLoad(this.gameObject);
    }
}
