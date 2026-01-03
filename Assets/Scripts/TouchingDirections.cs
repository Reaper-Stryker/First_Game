using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchingDirections : MonoBehaviour
{
    [Header("Detection Settings")]
    public ContactFilter2D castFilter;
    public float groundDistance = 0.05f;
    public float wallDistance = 0.3f;  // Increased for better detection
    public float ceilingDistance = 0.05f;

    private CapsuleCollider2D touchingCol;
    private Animator animator;

    RaycastHit2D[] groundHits = new RaycastHit2D[5];
    RaycastHit2D[] wallHits = new RaycastHit2D[5];
    RaycastHit2D[] ceilingHits = new RaycastHit2D[5];

    [SerializeField] private bool _isGrounded;
    public bool IsGrounded
    {
        get => _isGrounded;
        private set
        {
            _isGrounded = value;
            animator.SetBool(AnimationStrings.IsGrounded, value);
        }
    }

    [SerializeField] private bool _isOnWall;
    public bool IsOnWall
    {
        get => _isOnWall;
        private set
        {
            _isOnWall = value;
            animator.SetBool(AnimationStrings.IsOnWall, value);
        }
    }

    [SerializeField] private bool _isOnCeiling;
    public bool IsOnCeiling
    {
        get => _isOnCeiling;
        private set
        {
            _isOnCeiling = value;
            animator.SetBool(AnimationStrings.IsOnCeiling, value);
        }
    }

    private Vector2 WallCheckDirection => transform.localScale.x > 0 ? Vector2.right : Vector2.left;

    void Awake()
    {
        touchingCol = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();

        if (touchingCol == null)
            Debug.LogError("❌ CapsuleCollider2D is MISSING on " + gameObject.name);

        if (animator == null)
            Debug.LogError("❌ Animator is MISSING on " + gameObject.name);

        // Ensure proper ContactFilter2D setup
        castFilter.useTriggers = false;
        castFilter.SetLayerMask(LayerMask.GetMask("Ground", "Wall"));
        castFilter.useLayerMask = true;
    }

    void FixedUpdate()
    {
        // Check if touchingCol is NULL before using it
        if (touchingCol == null) return;

        IsGrounded = touchingCol.Cast(Vector2.down, castFilter, groundHits, groundDistance) > 0;
        IsOnWall = touchingCol.Cast(WallCheckDirection, castFilter, wallHits, wallDistance) > 0;
        IsOnCeiling = touchingCol.Cast(Vector2.up, castFilter, ceilingHits, ceilingDistance) > 0;

        // Debugging Visuals
        Debug.DrawRay(transform.position, Vector2.down * groundDistance, Color.green);   // Ground
        Debug.DrawRay(transform.position, WallCheckDirection * wallDistance, Color.red); // Wall
        Debug.DrawRay(transform.position, Vector2.up * ceilingDistance, Color.blue);     // Ceiling

        Debug.Log($"✅ Grounded: {IsGrounded}, OnWall: {IsOnWall}, OnCeiling: {IsOnCeiling}, WallCheckDir: {WallCheckDirection}");
    }
}
