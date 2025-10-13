using System;
using UnityEngine;

/// <summary>
/// Small MonoBehaviour state manager that also drives Animator states.
/// - Call ChangeState(...) to change state and automatically play the configured animation.
/// - Subclasses or other components can subscribe to OnStateChanged for additional reactions.
/// </summary>
public class EnemyStateMachine : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Wandering,
        Chasing,
        BasicAttack,
        Dead
    }

    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    /// <summary>Invoked after a successful state change: (oldState, newState).</summary>
    public event Action<EnemyState, EnemyState> OnStateChanged;
    private EnemyState? pendingState;

    /// <summary>Change to a new state; no-op if state is identical.</summary>
    public void ChangeState(EnemyState newState)
    {
        if (newState == CurrentState)
            return;

        // If currently attacking, queue outgoing changes instead of switching immediately.
        if (CurrentState == EnemyState.BasicAttack && newState != EnemyState.BasicAttack)
        {
            pendingState = newState;
            //Debug.Log($"[EnemyStateMachine] ChangeState: queued '{newState}' while in BasicAttack");
            return;
        }

        var old = CurrentState;
        CurrentState = newState;
        OnStateChanged?.Invoke(old, newState);
    }

    
    public void ForceChangeState(EnemyState newState)
    {
        if (newState == CurrentState) return;
        var old = CurrentState;
        CurrentState = newState;
        OnStateChanged?.Invoke(old, newState);
    }

    /// <summary>
    /// Apply any pending state that was queued while in BasicAttack.
    /// Will force the change even if the state machine still reports BasicAttack.
    /// </summary>
    public void ApplyPendingState()
    {
        if (!pendingState.HasValue) return;
        var target = pendingState.Value;
        pendingState = null;
        //Debug.Log($"[EnemyStateMachine] ApplyPendingState -> applying '{target}'");
        ForceChangeState(target);
    }

    public EnemyState GetState() => CurrentState;

    public bool HasState() => true; // always has an enum state
    
}