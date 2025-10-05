using UnityEngine;

public class General_Enemy_Detection : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameObject Player;
    [Header("Detection Settings")]
    [SerializeField] public float Detection_Range = 3.5f;
    [SerializeField] private LayerMask Layers_To_Detect;

    public bool hasLineOfSight = false;
    private enum SightState { None, Player, Obstacle, OutOfRange }

    private float detectionInterval = 0.2f; // how often to check for player, optimize performance for less constant checks
    private float detectionTimer = 0f;
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        if (transform.parent != null )
        {
            transform.position = transform.parent.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void FixedUpdate()
    {

        if (!IsPlayerWithinDetectionRange()) // stops raycast if player is out of range to optimize performance
            return;

        SightState state = GetSightStateToPlayer();
        //if (hit.collider == null)
        //    Debug.Log("Raycast hit nothing!");
        //else
        //    Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

        switch (state)
        {
            case SightState.Player:
                hasLineOfSight = true;
                //Debug.Log((transform.parent != null ? transform.parent.gameObject.name : gameObject.name) + " I see the Player!");
                DrawDetectionLine(Color.blueViolet);
                break;
            case SightState.Obstacle:
                hasLineOfSight = false;
                //Debug.Log((transform.parent != null ? transform.parent.gameObject.name : gameObject.name) + ": The player is in my range but is behind a wall!");
                DrawDetectionLine(Color.red);
                break;   
            default:
                hasLineOfSight = false;
                break;
        }
    }
    private bool IsPlayerWithinDetectionRange() // run this before casting raycast to optimize game performance
    {
        float distanceToPlayer = Vector2.Distance(transform.position, Player.transform.position);
        if (distanceToPlayer > Detection_Range)
        {
            detectionTimer += Time.fixedDeltaTime;
            if (detectionTimer < detectionInterval)
                return false;
            detectionTimer = 0f;

            hasLineOfSight = false;
            //Debug.Log((transform.parent != null ? transform.parent.gameObject.name : gameObject.name) + ": The player is outside of my range!");
            DrawDetectionLine(Color.gray);
            return false;
        }
        return true;
    }

    private SightState GetSightStateToPlayer()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Player.transform.position - transform.position,
            Detection_Range,
            Layers_To_Detect
        );

        if (hit.collider != null && hit.collider.CompareTag("Player"))
            return SightState.Player;
        if (hit.collider != null && (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Obstacle")))
            return SightState.Obstacle;
        return SightState.None;
    }
    private void DrawDetectionLine(Color color)
    {
        Debug.DrawLine(
        transform.parent != null ? transform.parent.position : transform.position,
        Player.transform.position,
        color
    );
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.darkMagenta;
        Gizmos.DrawWireSphere(transform.position, Detection_Range);
    }
}

