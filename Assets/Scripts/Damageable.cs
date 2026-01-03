using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    // Existing variables and events
    public UnityEvent<int, Vector2> damageableHit;
    public UnityEvent<int, int> healthChanged;

    [SerializeField] private int _maxHealth = 100;
    public int MaxHealth
    {
        get => _maxHealth;
        set => _maxHealth = value;
    }

    [SerializeField] private int _health = 100;
    public int Health
    {
        get => _health;
        set
        {
            _health = Mathf.Clamp(value, 0, MaxHealth);
            healthChanged?.Invoke(_health, MaxHealth);
            if (_health <= 0)
            {
                IsAlive = false;
            }
        }
    }

    [SerializeField] private bool _isAlive = true;
    private Rigidbody2D rb; // Added Rigidbody reference
    private Animator animator;

    public bool IsAlive
    {
        get => _isAlive;
        set
        {
            _isAlive = value;
            animator.SetBool(AnimationStrings.isAlive, value);

            // New: Stop movement on death
            if (!value)
            {
                rb.velocity = Vector2.zero;
                animator.SetBool(AnimationStrings.canMove, false);
                LockVelocity = true;
            }
            Debug.Log("IsAlive set to " + value);
        }
    }

    // Rest of existing variables
    [SerializeField] private bool isInvincible = false;
    private float timeSinceHit = 0f;
    public float invincibilityTime = 0.25f;

    public bool LockVelocity
    {
        get => animator.GetBool(AnimationStrings.lockVelocity);
        set => animator.SetBool(AnimationStrings.lockVelocity, value);
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>(); // New: Get Rigidbody

        if (animator == null)
            Debug.LogError("❌ Missing Animator on " + gameObject.name);
        if (rb == null)
            Debug.LogError("❌ Missing Rigidbody2D on " + gameObject.name);
    }

    // Rest of existing Update() and Hit() methods remain unchanged
    // ...

    public bool Hit(int damage, Vector2 knockback)
    {
        if (IsAlive && !isInvincible)
        {
            Health -= damage;
            isInvincible = true;
            animator.SetTrigger(AnimationStrings.hitTrigger);
            LockVelocity = true;
            damageableHit?.Invoke(damage, knockback);
            CharacterEvents.characterDamaged.Invoke(gameObject, damage);
            StartCoroutine(UnlockVelocityAfterDelay());
            return true;
        }
        return false;
    }

    private IEnumerator UnlockVelocityAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        if (IsAlive) // Only unlock if still alive
        {
            LockVelocity = false;
        }
    }

    // Rest of existing Heal() method
    public bool Heal(int healthRestore)
    {
        if (IsAlive && Health < MaxHealth)
        {
            int maxHeal = Mathf.Max(MaxHealth - Health, 0);
            int actualHeal = Mathf.Min(maxHeal, healthRestore);
            Health += actualHeal;
            CharacterEvents.characterHealed(gameObject, actualHeal);
            return true;
        }
        return false;
    }
}
