using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]
public class Knight : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float walkStopRate = 0.05f;
    public DetectionZone attackZone;
    public DetectionZone cliffDetectionZone;

    [Header("Combat")]
    public float attackCooldown = 2f;
    public Vector2 knockbackPower = new Vector2(2f, 3f);

    // Component references
    private Rigidbody2D rb;
    private TouchingDirections touchingDirections;
    private Animator animator;
    private Damageable damageable;

    // Movement state
    private Vector2 walkDirectionVector = Vector2.right;
    private bool _hasTarget = false;

    public enum WalkableDirection { Right, Left }
    private WalkableDirection _walkDirection;

    public WalkableDirection WalkDirection
    {
        get => _walkDirection;
        set
        {
            if (_walkDirection != value)
            {
                // Flip sprite
                transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                walkDirectionVector = value == WalkableDirection.Right ? Vector2.right : Vector2.left;
            }
            _walkDirection = value;
        }
    }

    public bool HasTarget
    {
        get => _hasTarget;
        private set
        {
            _hasTarget = value;
            animator.SetBool(AnimationStrings.hasTarget, value);
        }
    }

    public bool CanMove => animator.GetBool(AnimationStrings.canMove);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        animator = GetComponent<Animator>();
        damageable = GetComponent<Damageable>();
    }

    private void Start()
    {
        // Initialize facing direction based on starting scale
        WalkDirection = transform.localScale.x > 0 ? WalkableDirection.Right : WalkableDirection.Left;
    }

    private void Update()
    {
        HasTarget = attackZone.detectedColliders.Count > 0;

        if (HasTarget && AttackCooldown <= 0)
        {
            // Trigger attack
            animator.SetTrigger(AnimationStrings.attack);
            AttackCooldown = attackCooldown;
        }
        else if (AttackCooldown > 0)
        {
            AttackCooldown -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (touchingDirections.IsGrounded)
        {
            // Flip if wall ahead or no ground detected
            if (touchingDirections.IsOnWall || cliffDetectionZone.detectedColliders.Count == 0)
            {
                FlipDirection();
            }

            if (CanMove && !HasTarget)
            {
                // Regular movement
                rb.velocity = new Vector2(walkSpeed * walkDirectionVector.x, rb.velocity.y);
            }
            else
            {
                // Smooth stop when can't move
                rb.velocity = new Vector2(
                    Mathf.Lerp(rb.velocity.x, 0, walkStopRate),
                    rb.velocity.y
                );
            }
        }
    }

    private void FlipDirection()
    {
        WalkDirection = WalkDirection == WalkableDirection.Right
            ? WalkableDirection.Left
            : WalkableDirection.Right;
    }

    public float AttackCooldown
    {
        get => animator.GetFloat(AnimationStrings.attackCooldown);
        set => animator.SetFloat(AnimationStrings.attackCooldown, Mathf.Max(value, 0));
    }

    // Called by animation event during attack
    public void OnAttack()
    {
        if (attackZone.detectedColliders.Count > 0)
        {
            foreach (Collider2D collider in attackZone.detectedColliders)
            {
                if (collider.TryGetComponent<Damageable>(out var damageable))
                {
                    Vector2 knockback = new Vector2(
                        knockbackPower.x * walkDirectionVector.x,
                        knockbackPower.y
                    );

                    damageable.Hit(1, knockback);
                }
            }
        }
    }

    // Death handling
    private void OnEnable()
    {
        damageable.healthChanged.AddListener(OnHealthChanged);
    }

    private void OnDisable()
    {
        damageable.healthChanged.RemoveListener(OnHealthChanged);
    }

    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        if (currentHealth <= 0)
        {
            // Disable enemy on death
            gameObject.SetActive(false);
        }
    }
}
