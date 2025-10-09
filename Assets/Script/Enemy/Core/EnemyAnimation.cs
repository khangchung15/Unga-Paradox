using UnityEngine;
using static UnityEngine.CullingGroup;

public class EnemyAnimation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Animator animator;
    private EnemyMovement enemyMovement;
    private EnemyStateMachine stateMachine;
    

    private float movementThreshold = 0.1f; // threshold to consider as moving
    //private float currentMoveSpeed;
    private EnemyStateMachine.EnemyState currentState;
    [SerializeField] private string isWalkingParam = "isWalking"; // Animator parameter name for walking 
    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyMovement = GetComponentInChildren<EnemyMovement>();
        stateMachine = GetComponent<EnemyStateMachine>();
        
        if (animator == null)
            throw new System.Exception("EnemyAnimation requires an Animator component.");
        if (enemyMovement == null)
            throw new System.Exception("EnemyAnimation requires an EnemyMovement component in children.");
        if (stateMachine == null)
            throw new System.Exception("EnemyAnimation requires an EnemyStateMachine component.");
    }

    private void Start()
    {
        
    }
    private void OnEnable()
    {
        if (stateMachine != null)
            stateMachine.OnStateChanged += OnStateChanged;

        // set initial animator value
        ApplyState(stateMachine != null ? stateMachine.GetState() : EnemyStateMachine.EnemyState.Idle);
    }
    private void OnDisable()
    {
        if (stateMachine != null)
            stateMachine.OnStateChanged -= OnStateChanged;
    }

    // Update is called once per frame
    private void Update()
    {
        //Debug.Log("Enemy Current State: " + (stateMachine != null ? stateMachine.GetState().ToString() : "No StateMachine"));
        //Debug.Log("Enemy Movement Speed: " + (enemyMovement != null ? enemyMovement.MovementSpeed.ToString("F2") : "No EnemyMovement"));
        //Debug.Log("Enemy Current Movement Status: " + HasMovement());
        if (animator != null) 
        {
            if (stateMachine != null && stateMachine.GetState() == EnemyStateMachine.EnemyState.Wandering)
            {
                SetWalking(HasMovement());
            }
        }
    }
    private void OnStateChanged(EnemyStateMachine.EnemyState oldState, EnemyStateMachine.EnemyState newState)
    {
        ApplyState(newState);
    }
    private void ApplyState(EnemyStateMachine.EnemyState state)
    {
        if (animator == null) 
            return;

        if (state == EnemyStateMachine.EnemyState.Wandering)
            SetWalking(HasMovement());
        else if (state == EnemyStateMachine.EnemyState.Idle) // Idle -> not walking
            SetWalking(false);
    }
    private bool HasMovement()
    {
        if (enemyMovement == null) 
            return false;
        // MovementSpeed is expected to be a readable property on EnemyMovement
        return enemyMovement.GetCurrentMovementSpeed > movementThreshold;
    }

    private void SetWalking(bool walking)
    {
        if (animator == null || string.IsNullOrEmpty(isWalkingParam)) return;
        animator.SetBool(isWalkingParam, walking);
    }

}
