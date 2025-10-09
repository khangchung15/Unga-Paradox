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

    /// <summary>Change to a new state; no-op if state is identical.</summary>
    public void ChangeState(EnemyState newState)
    {
        if (newState == CurrentState) 
            return;

        var old = CurrentState;
        CurrentState = newState;
        OnStateChanged?.Invoke(old, newState);
    }

    /// <summary>Forcefully set state without invoking animation (rare). Use ChangeState normally.</summary>
    public void SetStateWithoutAnimation(EnemyState newState)
    {
        if (newState == CurrentState) return;
        var old = CurrentState;
        CurrentState = newState;
        OnStateChanged?.Invoke(old, newState);
    }

    public EnemyState GetState() => CurrentState;

    public bool HasState() => true; // always has an enum state
    
}