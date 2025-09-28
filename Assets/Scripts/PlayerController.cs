using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 5f;
    public float jumpForce = 12f;
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float secondJumpCost = 10f;
    public float dashCost = 15f;

    [Header("Slide Settings")]
    public float slideDuration = 0.5f;
    public float slideSpeedMultiplier = 1.5f;
    public float slideCost = 5f;
    public Vector2 slideColliderSize = new Vector2(1f, 0.5f);
    public Vector2 slideColliderOffset = new Vector2(0f, -0.25f);
    public float slideScaleY = 0.5f; // Visual scale reduction during slide

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float healthDecreaseRate = 5f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float checkRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Hurt Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color hurtColor = Color.red;
    public float hurtFlashTime = 0.1f;
    public float invincibleTime = 1f;
    public float invincibleFlashInterval = 0.12f;

    [Header("UI Manager")]
    public UIManager uiManager;   // Reference to UIManager

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool isGrounded = false;
    private bool jumpPressed = false;
    private int jumpCount = 0;
    private bool isDead = false;
    private bool isDashing = false;
    private bool isSliding = false;
    private bool isInvincible = false;
    private Color originalColor;

    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private Vector3 originalScale;

    // Coroutine references
    private Coroutine boostCoroutine;
    private Coroutine slowDownCoroutine;
    private float baseSpeed;

    // Distance tracking
    private float startX;
    private float currentDistance;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 3f;

        boxCollider = GetComponent<BoxCollider2D>();
        originalColliderSize = boxCollider.size;
        originalColliderOffset = boxCollider.offset;
        originalScale = transform.localScale;

        currentHealth = maxHealth;
        baseSpeed = forwardSpeed;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        // Record start position
        startX = transform.position.x;

        // Initialize UI
        if (uiManager != null)
        {
            uiManager.UpdateHealth(currentHealth, maxHealth);
            uiManager.UpdateDistance(0f);
        }
    }

    void Update()
    {
        if (isDead) return;

        // Health decreases over time
        currentHealth -= healthDecreaseRate * Time.deltaTime;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (uiManager != null)
            uiManager.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();

        // Ground check
        isGrounded = groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (isGrounded) jumpCount = 0;

        // Jump input
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;

        // Dash input
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && !isDashing && !isSliding && currentHealth > dashCost)
            StartCoroutine(Dash());

        // Slide input
        if ((Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
            && isGrounded && !isSliding && !isDashing && currentHealth > slideCost)
            StartCoroutine(Slide());

        // Update distance
        currentDistance = transform.position.x - startX;
        if (uiManager != null)
            uiManager.UpdateDistance(currentDistance);

        // Check win condition
        // if (uiManager != null && currentDistance >= uiManager.targetDistance)
        // {
        //     Win();
        // }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (!isDashing)
        {
            float speed = forwardSpeed;
            if (isSliding)
                speed *= slideSpeedMultiplier;

            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
        }

        if (jumpPressed && !isSliding && !isDashing)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpCount = 1;
            }
            else if (jumpCount == 1 && currentHealth > secondJumpCost)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                currentHealth -= secondJumpCost;
                if (uiManager != null) uiManager.UpdateHealth(currentHealth, maxHealth);
                jumpCount = 2;
            }
            jumpPressed = false;
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        currentHealth -= dashCost;
        if (uiManager != null) uiManager.UpdateHealth(currentHealth, maxHealth);

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(dashForce, 0);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    IEnumerator Slide()
    {
        isSliding = true;
        currentHealth -= slideCost;
        if (uiManager != null) uiManager.UpdateHealth(currentHealth, maxHealth);

        Vector2 originalSize = boxCollider.size;
        Vector2 originalOffset = boxCollider.offset;
        Vector3 originalPlayerScale = transform.localScale;

        boxCollider.size = slideColliderSize;
        boxCollider.offset = slideColliderOffset;
        transform.localScale = new Vector3(originalPlayerScale.x, originalPlayerScale.y * slideScaleY, originalPlayerScale.z);

        float originalSpeed = forwardSpeed;
        forwardSpeed *= slideSpeedMultiplier;

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        boxCollider.size = originalSize;
        boxCollider.offset = originalOffset;
        transform.localScale = originalPlayerScale;
        forwardSpeed = originalSpeed;

        isSliding = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("Coin"))
        {
            currentHealth += 20f;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            if (uiManager != null) uiManager.UpdateHealth(currentHealth, maxHealth);
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Obstacle"))
        {
            TakeDamage(30f);
        }
    }

    void TakeDamage(float damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        if (uiManager != null) uiManager.UpdateHealth(currentHealth, maxHealth);

        if (spriteRenderer != null)
            StartCoroutine(HurtThenInvincible());

        if (currentHealth <= 0) Die();
    }

    IEnumerator HurtThenInvincible()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = hurtColor;
        yield return new WaitForSeconds(hurtFlashTime);

        float elapsed = 0f;
        isInvincible = true;
        bool showHurt = false;

        while (elapsed < invincibleTime)
        {
            spriteRenderer.color = showHurt ? hurtColor : originalColor;
            showHurt = !showHurt;
            yield return new WaitForSeconds(invincibleFlashInterval);
            elapsed += invincibleFlashInterval;
        }

        spriteRenderer.color = originalColor;
        isInvincible = false;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        Debug.Log("Game Over!");
        if (uiManager != null) uiManager.ShowGameOver();
    }

    void Win()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        Debug.Log("You Win!");
        if (uiManager != null) uiManager.ShowWin();
    }

    public void ActivateBoost(float duration, float multiplier)
    {
        if (slowDownCoroutine != null)
        {
            StopCoroutine(slowDownCoroutine);
            slowDownCoroutine = null;
        }
        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
        }
        boostCoroutine = StartCoroutine(Boost(duration, multiplier));
    }

    private IEnumerator Boost(float duration, float multiplier)
    {
        isInvincible = true;
        forwardSpeed = baseSpeed * multiplier;

        yield return new WaitForSeconds(duration);

        forwardSpeed = baseSpeed;
        isInvincible = false;
        boostCoroutine = null;
    }

    public void ActivateSlowDown(float duration, float multiplier)
    {
        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
            boostCoroutine = null;
            isInvincible = false; // BUG FIX: Manually reset invincibility
        }
        if (slowDownCoroutine != null)
        {
            StopCoroutine(slowDownCoroutine);
        }
        slowDownCoroutine = StartCoroutine(SlowDown(duration, multiplier));
    }

    private IEnumerator SlowDown(float duration, float multiplier)
    {
        forwardSpeed = baseSpeed * multiplier;

        yield return new WaitForSeconds(duration);

        forwardSpeed = baseSpeed;
        slowDownCoroutine = null;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
