using UnityEngine;

public class TeleportDevice : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;

    [Header("Teleport Settings")]
    [SerializeField] private float maxTeleportDistance = 15f;
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private float playerRadius = 0.3f;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject teleportEffectPrefab;
    [SerializeField] private AudioClip teleportSound;

    [Header("Indicator Settings")]
    [SerializeField] private bool showIndicator = true;
    [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.5f);

    [Header("Camera Boundaries")]
    [SerializeField] private bool useCameraBounds = true;
    [SerializeField] private BoxCollider2D cameraBoundsCollider;
    [SerializeField] private float boundsPadding = 0.5f;

    private Camera cam;
    private Transform playerTransform;
    private AudioSource audioSource;
    private GameObject indicator;
    private SpriteRenderer indicatorRenderer;

    // Camera boundary cache
    private Bounds cameraWorldBounds;
    private bool hasCameraBounds;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        cam = Camera.main;
        SetupCameraBounds();
    }

    private void Start()
    {
        if (PlayerController.Instance != null)
            playerTransform = PlayerController.Instance.transform;
        else if (ScientistController.Instance != null)
            playerTransform = ScientistController.Instance.transform;

        CreateIndicator();
    }

    private void Update()
    {
        if (playerTransform != null && cam != null)
        {
            RotateTowardMouse();
            if (showIndicator)
                UpdateIndicatorPosition();
        }
    }

    private void SetupCameraBounds()
    {
        if (!useCameraBounds) return;

        // Try to find camera bounds automatically
        if (cameraBoundsCollider == null)
        {
            // Look for a bounds collider in the scene
            CameraBounds bounds = FindObjectOfType<CameraBounds>();
            if (bounds != null)
            {
                cameraBoundsCollider = bounds.GetComponent<BoxCollider2D>();
            }
            
            // If still null, try to find by tag or name
            if (cameraBoundsCollider == null)
            {
                GameObject boundsObj = GameObject.FindGameObjectWithTag("CameraBounds");
                if (boundsObj != null)
                    cameraBoundsCollider = boundsObj.GetComponent<BoxCollider2D>();
            }
        }

        hasCameraBounds = cameraBoundsCollider != null;
        if (hasCameraBounds)
        {
            cameraWorldBounds = cameraBoundsCollider.bounds;
        }
    }

    private void CreateIndicator()
    {
        indicator = new GameObject("TeleportIndicator");
        indicator.transform.SetParent(transform);
        
        indicatorRenderer = indicator.AddComponent<SpriteRenderer>();
        indicatorRenderer.sprite = CreateSimpleCircleSprite();
        indicatorRenderer.sortingOrder = 1000;
        indicator.transform.localScale = Vector3.one * 0.5f;
        indicator.SetActive(showIndicator);
    }

    private Sprite CreateSimpleCircleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float outerRadius = size / 2f - 1f;
        float innerRadius = outerRadius - 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= outerRadius && dist >= innerRadius)
                    colors[y * size + x] = Color.white;
                else
                    colors[y * size + x] = Color.clear;
            }
        }
        
        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private void RotateTowardMouse()
    {
        Vector3 mouseWorld = GetMouseWorldPosition();
        Vector2 direction = (mouseWorld - playerTransform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        if (direction.x < 0)
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(180, 0, -angle);
        else
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(cam.transform.position.z);
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;

        // Apply camera bounds if enabled
        if (useCameraBounds && hasCameraBounds)
        {
            worldPos = ClampPositionToCameraBounds(worldPos);
        }

        return worldPos;
    }

    private Vector3 ClampPositionToCameraBounds(Vector3 position)
    {
        if (!hasCameraBounds) return position;

        // Calculate camera viewport bounds in world coordinates
        float cameraHeight = 2f * cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;
        
        Vector3 cameraPos = cam.transform.position;
        Bounds cameraViewBounds = new Bounds(
            new Vector3(cameraPos.x, cameraPos.y, 0f),
            new Vector3(cameraWidth, cameraHeight, 0f)
        );

        // Clamp camera view bounds to the level bounds
        Bounds levelBounds = cameraWorldBounds;
        
        cameraViewBounds.min = new Vector3(
            Mathf.Max(cameraViewBounds.min.x, levelBounds.min.x + boundsPadding),
            Mathf.Max(cameraViewBounds.min.y, levelBounds.min.y + boundsPadding),
            0f
        );
        cameraViewBounds.max = new Vector3(
            Mathf.Min(cameraViewBounds.max.x, levelBounds.max.x - boundsPadding),
            Mathf.Min(cameraViewBounds.max.y, levelBounds.max.y - boundsPadding),
            0f
        );

        // Clamp the target position to the camera view bounds
        position.x = Mathf.Clamp(position.x, cameraViewBounds.min.x, cameraViewBounds.max.x);
        position.y = Mathf.Clamp(position.y, cameraViewBounds.min.y, cameraViewBounds.max.y);

        return position;
    }

    private void UpdateIndicatorPosition()
    {
        if (indicator == null || playerTransform == null) return;

        Vector3 targetPos = GetMouseWorldPosition();
        Vector3 playerPos = playerTransform.position;
        
        // Calculate direction from player to target
        Vector2 direction = (targetPos - playerPos).normalized;
        float dist = Vector2.Distance(playerPos, targetPos);
        
        // Limit by max teleport distance
        if (dist > maxTeleportDistance)
        {
            targetPos = playerPos + (Vector3)direction * maxTeleportDistance;
        }

        // Check for obstacles along the path
        RaycastHit2D hit = Physics2D.Linecast(playerPos, targetPos, obstacleLayers);
        if (hit.collider != null)
        {
            targetPos = hit.point - direction * (playerRadius + 0.05f);
        }

        // Re-clamp after obstacle check
        if (useCameraBounds && hasCameraBounds)
        {
            targetPos = ClampPositionToCameraBounds(targetPos);
        }

        // Final validation - check if target position is clear
        bool isValid = Physics2D.OverlapCircle(targetPos, playerRadius, obstacleLayers) == null;
        
        indicator.transform.position = targetPos;
        indicatorRenderer.color = isValid ? validColor : invalidColor;
    }

    public void Attack()
    {
        if (playerTransform == null || cam == null) return;

        Vector3 targetPos = GetMouseWorldPosition();
        Vector3 playerPos = playerTransform.position;
        
        // Calculate direction from player to target
        Vector2 direction = (targetPos - playerPos).normalized;
        float dist = Vector2.Distance(playerPos, targetPos);
        
        // Limit by max teleport distance
        if (dist > maxTeleportDistance)
        {
            targetPos = playerPos + (Vector3)direction * maxTeleportDistance;
        }

        // Check for obstacles along the path
        RaycastHit2D hit = Physics2D.Linecast(playerPos, targetPos, obstacleLayers);
        if (hit.collider != null)
        {
            targetPos = hit.point - direction * (playerRadius + 0.05f);
        }

        // Re-clamp after obstacle check
        if (useCameraBounds && hasCameraBounds)
        {
            targetPos = ClampPositionToCameraBounds(targetPos);
        }

        // Final validation
        if (Physics2D.OverlapCircle(targetPos, playerRadius, obstacleLayers) != null)
        {
            return;
        }

        // Perform teleport
        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, playerPos, Quaternion.identity);

        playerTransform.position = targetPos;

        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, targetPos, Quaternion.identity);

        if (audioSource != null && teleportSound != null)
            audioSource.PlayOneShot(teleportSound);
    }

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    // Public method to set camera bounds at runtime if needed
    public void SetCameraBounds(BoxCollider2D boundsCollider)
    {
        cameraBoundsCollider = boundsCollider;
        hasCameraBounds = boundsCollider != null;
        if (hasCameraBounds)
        {
            cameraWorldBounds = boundsCollider.bounds;
        }
    }

    private void OnDestroy()
    {
        if (indicator != null)
            Destroy(indicator);
    }
}