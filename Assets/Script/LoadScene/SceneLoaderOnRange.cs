using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoaderOnRange : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    
    [Header("Scene Loading Settings")]
    public string sceneName;
    public float detectionRange = 10.0f;
    
    [Header("Loading Options")]
    public bool loadOnEnter = true;
    public bool oneTimeLoad = true;
    public float loadDelay = 0f;
    public bool useAsyncLoading = true;
    
    [Header("Camera & Player Settings")]
    [Tooltip("Switch camera to this object before scene transition")]
    public bool switchCameraToThis = true;
    [Tooltip("Destroy player object after camera switch")]
    public bool destroyPlayer = true;
    [Tooltip("Delay after camera switch before scene transition starts")]
    public float cameraSwitchDelay = 1.5f;
    
    [Header("Loading Screen (Optional)")]
    public string loadingScreenSceneName = "LoadingScreen";
    public bool useLoadingScreen = false;
    
    [Header("Visual Feedback")]
    public bool showRangeAlways = true;
    public Color rangeColor = Color.cyan;
    
    private bool hasLoaded = false;
    private bool targetInRange = false;
    private bool isLoading = false;
    private Camera playerCamera;
    private GameObject playerObject;
    
    void Update()
    {
        if (target == null || hasLoaded && oneTimeLoad || isLoading)
            return;
            
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        bool wasInRange = targetInRange;
        targetInRange = distanceToTarget <= detectionRange;
        
        if (loadOnEnter && !wasInRange && targetInRange && !hasLoaded)
        {
            StartLoadSequence();
        }
        else if (!loadOnEnter && wasInRange && !targetInRange && !hasLoaded)
        {
            StartLoadSequence();
        }
    }
    
    void StartLoadSequence()
    {
        
        if (oneTimeLoad)
        {
            hasLoaded = true;
        }
        
        isLoading = true;
        StartCoroutine(SceneTransitionRoutine());
    }
    
    IEnumerator SceneTransitionRoutine()
    {
        // Store references
        playerObject = target?.gameObject;
        playerCamera = Camera.main;
        
        // Step 1: Switch camera to this object and handle player
        if (switchCameraToThis && playerCamera != null)
        {
            yield return StartCoroutine(SwitchCameraAndHandlePlayer());
        }
        
        // Step 2: Wait for additional delay if specified
        if (loadDelay > 0)
        {
            yield return new WaitForSeconds(loadDelay);
        }
        
        // Step 3: Load the scene
        yield return StartCoroutine(LoadSceneRoutine());
    }
    
    IEnumerator SwitchCameraAndHandlePlayer()
    {
        // Disable player controller if it exists
        PlayerController playerController = playerObject?.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Make camera follow this object instead of player
        if (playerCamera != null)
        {
            // Store original camera offset
            Vector3 cameraOffset = new Vector3(0, 0, playerCamera.transform.position.z);
            
            // Start camera follow coroutine
            StartCoroutine(FollowThisObject(cameraOffset));
        }
        
        // Destroy player object if requested
        if (destroyPlayer && playerObject != null)
        {
            // Optional: Hide player immediately for better visual effect
            SpriteRenderer playerSprite = playerObject.GetComponent<SpriteRenderer>();
            if (playerSprite != null)
            {
                playerSprite.enabled = false;
            }
            
            // Wait before destroying player
            yield return new WaitForSeconds(cameraSwitchDelay * 0.5f);
            
            Destroy(playerObject);
            playerObject = null;
        }
        else
        {
            // Just wait the full delay if not destroying player
            yield return new WaitForSeconds(cameraSwitchDelay);
        }
    }
    
    IEnumerator FollowThisObject(Vector3 cameraOffset)
    {
        while (isLoading) // Continue until scene loads
        {
            if (playerCamera != null)
            {
                playerCamera.transform.position = transform.position + cameraOffset;
            }
            yield return null;
        }
    }
    
    IEnumerator LoadSceneRoutine()
    {
        // Load loading screen first if specified
        if (useLoadingScreen && !string.IsNullOrEmpty(loadingScreenSceneName))
        {
            SceneManager.LoadScene(loadingScreenSceneName);
            yield return null;
        }
        
        // Load the target scene
        if (useAsyncLoading)
        {
            yield return StartCoroutine(LoadSceneAsync(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
    
    IEnumerator LoadSceneAsync(string sceneToLoad)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;
        
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            // Check if the load has finished (0.9 means the scene is loaded but not activated)
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            
            yield return null;
        }
    }
    
    // Visualize detection range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = rangeColor;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if (targetInRange && target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
    
    void OnDrawGizmos()
    {
        if (showRangeAlways)
        {
            Gizmos.color = new Color(rangeColor.r, rangeColor.g, rangeColor.b, 0.2f);
            Gizmos.DrawSphere(transform.position, detectionRange);
        }
    }
    
    // Public methods
    public void ManuallyLoadScene()
    {
        if (!isLoading && !hasLoaded)
        {
            StartLoadSequence();
        }
    }
    
    public void SetSceneToLoad(string newSceneName)
    {
        sceneName = newSceneName;
        hasLoaded = false;
        isLoading = false;
    }
    
    // Method to trigger from other scripts with custom target
    public void TriggerWithTarget(GameObject customTarget)
    {
        if (!isLoading && !hasLoaded && customTarget != null)
        {
            target = customTarget.transform;
            StartLoadSequence();
        }
    }
}