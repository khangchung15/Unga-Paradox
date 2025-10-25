using UnityEngine;
using System.Collections;

public class CameraFollowObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerTransform;

    [Header("Flip Rotation Stats")]
    [SerializeField] private float _flipYRotationTime = 0.5f;

    private Coroutine _turnCoroutine;
    private ScientistController _player; // Changed from Player to ScientistController
    private bool _isFacingRight;

    private void Awake()
    {
        // Get the ScientistController component instead of Player
        _player = _playerTransform.gameObject.GetComponent<ScientistController>();
        
        // Initialize facing direction based on current player state
        if (_player != null)
        {
            _isFacingRight = (_player.facing == ScientistController.PlayerDirection.Right);
        }
        else
        {
            Debug.LogError("ScientistController component not found on player transform!");
        }
    }

    private void Update()
    {
        // Follow player position
        transform.position = _playerTransform.position;

        // Check if player changed direction and call turn if needed
        if (_player != null)
        {
            bool currentFacingRight = (_player.facing == ScientistController.PlayerDirection.Right);
            if (currentFacingRight != _isFacingRight)
            {
                CallTurn();
            }
        }
    }

    public void CallTurn() 
    {
        // Stop existing coroutine before starting new one
        if (_turnCoroutine != null)
        {
            StopCoroutine(_turnCoroutine);
        }
        _turnCoroutine = StartCoroutine(FlipYLerp());
    }

    private IEnumerator FlipYLerp()
    {
        float startRotation = transform.localEulerAngles.y;
        float endRotationAmount = DetermineEndRotation();
        float yRotation = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < _flipYRotationTime)
        {
            elapsedTime += Time.deltaTime;
            
            yRotation = Mathf.Lerp(startRotation, endRotationAmount, (elapsedTime / _flipYRotationTime));
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
            yield return null;
        }

        // Ensure final rotation is exact
        transform.rotation = Quaternion.Euler(0f, endRotationAmount, 0f);
    }

    private float DetermineEndRotation()
    {
        _isFacingRight = !_isFacingRight;

        if (_isFacingRight)
        {
            return 0f; // Facing right = 0 degrees
        }
        else
        {
            return 180f; // Facing left = 180 degrees
        }
    }
}