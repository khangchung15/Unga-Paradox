using System;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameObject Player;
    [Header("Detection Settings")]
    private float detectionRange;
    [SerializeField] private LayerMask Layers_To_Detect;

    public bool hasLineOfSight = false;

    public float DetectionRange
    { 
        get => detectionRange; 
        set => detectionRange = Mathf.Max(0f, value);
    }

    public SightState CurrentSightState { get; private set; } = SightState.None;
    public enum SightState { None, Player, Obstacle, OutOfRange }
    public event Action<SightState> OnSightStateChanged;

    readonly float detectionInterval = 0.5f; // how often to check for player, optimize performance for less constant checks
    private float detectionTimer = 0f;

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        if (transform != null )
        {
            transform.position = transform.position;
        }
    }

    private void FixedUpdate()
    {

        if (Player == null)
            return;
        
        if (!IsPlayerWithinDetectionRange()) // stops raycast if player is out of range to optimize performance
            return;

        SightState newState = GetSightStateToPlayer();
        if (newState != CurrentSightState)
        {
            CurrentSightState = newState;
            hasLineOfSight = (CurrentSightState == SightState.Player);
            OnSightStateChanged?.Invoke(newState);
        }   

        switch (CurrentSightState)
        {
            case SightState.Player:
                
                DrawDetectionLine(Color.blueViolet);
                break;
            case SightState.Obstacle:
                DrawDetectionLine(Color.red);
                break;   
            default:
                //DrawDetectionLine(Color.gray);
                break;
        }
    }
    private bool IsPlayerWithinDetectionRange() // run this before casting raycast to optimize game performance
    {
        float distanceToPlayer = Vector2.Distance(transform.position, Player.transform.position);
        if (distanceToPlayer > detectionRange)
        {
            detectionTimer += Time.fixedDeltaTime;
            if (detectionTimer < detectionInterval)
                return false;
            detectionTimer = 0f;

            // update state to OutOfRange if necessary
            if (CurrentSightState != SightState.OutOfRange)
            {
                CurrentSightState = SightState.OutOfRange;
                OnSightStateChanged?.Invoke(CurrentSightState);
            }
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
            detectionRange,
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
        transform != null ? transform.position : transform.position,
        Player.transform.position,
        color
    );
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.darkMagenta;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

