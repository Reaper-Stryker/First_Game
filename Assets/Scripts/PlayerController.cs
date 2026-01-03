using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float airSpeed = 3f;
    public float jumpImpulse = 10f;

    [Header("Input System")]
    public PlayerInput playerInput;

    private Vector2 moveInput;
    private Rigidbody2D rb;
    private Animator animator;
    private TouchingDirections touchingDirections;
    private Damageable damageable;
    private bool isFacingRight = true;
    private bool isRunning = false;

    private bool CanMove => animator.GetBool(AnimationStrings.canMove);
    private bool IsAlive => animator.GetBool(AnimationStrings.isAlive);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        damageable = GetComponent<Damageable>();

        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        // Ensure Input System is properly initialized
        if (playerInput != null && playerInput.actions != null)
        {
            playerInput.actions.Enable();
            Debug.Log("Input Actions enabled in Awake");
        }
        else
        {
            Debug.LogError("PlayerInput or actions are null!");
        }
    }

    private void Start()
    {
        // Additional safety check for build
        if (playerInput != null)
        {
            playerInput.enabled = true;
            if (playerInput.actions != null)
            {
                playerInput.actions.Enable();
                Debug.Log($"PlayerInput enabled. Current control scheme: {playerInput.currentControlScheme}");
            }
        }
    }

    private void FixedUpdate()
    {
        // Add debug logging for build troubleshooting
        if (Time.fixedTime % 1f < Time.fixedDeltaTime) // Log once per second
        {
            Debug.Log($"MoveInput: {moveInput}, CanMove: {CanMove}, IsAlive: {IsAlive}, LockVelocity: {damageable.LockVelocity}");
        }

        if (!damageable.LockVelocity && CanMove && IsAlive)
        {
            float moveSpeed = GetCurrentMoveSpeed();
            rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
        }
        else if (!IsAlive || damageable.LockVelocity)
        {
            // Stop horizontal movement when dead or locked
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        animator.SetBool(AnimationStrings.IsMoving, Mathf.Abs(rb.velocity.x) > 0.1f && CanMove);
        animator.SetBool(AnimationStrings.IsRunning, isRunning && CanMove);
        animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
    }

    private float GetCurrentMoveSpeed()
    {
        if (touchingDirections.IsOnWall || !CanMove) return 0f;
        return touchingDirections.IsGrounded ?
            (isRunning ? runSpeed : walkSpeed) :
            airSpeed;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log($"OnMove called: {context.ReadValue<Vector2>()} - Phase: {context.phase}");

        if (!IsAlive)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = context.ReadValue<Vector2>();
        UpdateFacingDirection();
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        Debug.Log($"OnRun called - Phase: {context.phase}");

        if (!IsAlive) return;

        if (context.started) isRunning = true;
        else if (context.canceled) isRunning = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log($"OnJump called - Phase: {context.phase}");

        if (context.started && touchingDirections.IsGrounded && CanMove && IsAlive)
        {
            animator.SetTrigger(AnimationStrings.jump);
            rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log($"OnAttack called - Phase: {context.phase}");

        if (context.started && IsAlive && CanMove)
        {
            animator.SetTrigger(AnimationStrings.attack);
        }
    }

    private void UpdateFacingDirection()
    {
        if (moveInput.x > 0 && !isFacingRight) Flip();
        else if (moveInput.x < 0 && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.enabled = true;
            if (playerInput.actions != null)
            {
                playerInput.actions.Enable();
            }
        }
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            if (playerInput.actions != null)
            {
                playerInput.actions.Disable();
            }
            playerInput.enabled = false;
        }
    }

    // Fallback input for testing (remove after fixing)
    private void Update()
    {
        // Old Input System fallback for debugging
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            Debug.Log("Old Input: Left key detected");
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            Debug.Log("Old Input: Right key detected");
        }
    }
}
