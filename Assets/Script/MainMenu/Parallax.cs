using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;   // new Input System
#endif

public class MenuParallax : MonoBehaviour
{
    [Tooltip("How far the UI moves (pixels) from center at screen edges.")]
    public float offsetMultiplier = 30f;
    public float smoothTime = 0.2f;

    private RectTransform rt;
    private Vector2 startAnchored;
    private Vector2 velocity;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        startAnchored = rt.anchoredPosition;
    }

    void Update()
    {
        Vector2 mouse01;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null) return;
        Vector2 mouse = Mouse.current.position.ReadValue();
        mouse01 = new Vector2(mouse.x / Screen.width, mouse.y / Screen.height);
#else
        mouse01 = Camera.main != null
            ? (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition)
            : new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
#endif

        // center the range around 0 (so center of screen = no offset)
        Vector2 centered = mouse01 - new Vector2(0.5f, 0.5f);

        Vector2 target = startAnchored + centered * offsetMultiplier;
        rt.anchoredPosition = Vector2.SmoothDamp(rt.anchoredPosition, target, ref velocity, smoothTime);
    }
}
