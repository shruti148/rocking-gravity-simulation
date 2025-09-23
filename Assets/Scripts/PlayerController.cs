using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float forwardSpeed = 5f;
    public float jumpForce = 12f;
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float secondJumpCost = 10f;
    public float dashCost = 15f;

    [Header("滑行设置")]
    public float slideDuration = 0.5f;
    public float slideSpeedMultiplier = 1.5f;
    public float slideCost = 5f;
    public Vector2 slideColliderSize = new Vector2(1f, 0.5f);
    public Vector2 slideColliderOffset = new Vector2(0f, -0.25f);
    public float slideScaleY = 0.5f; // 玩家视觉缩小比例

    [Header("生命值")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float healthDecreaseRate = 5f;

    [Header("地面检测")]
    public Transform groundCheck;
    public float checkRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("UI")]
    public Slider healthSlider;
    public GameObject gameOverPanel;

    [Header("受伤反馈")]
    public SpriteRenderer spriteRenderer;
    public Color hurtColor = Color.red;
    public float hurtFlashTime = 0.1f;
    public float invincibleTime = 1f;
    public float invincibleFlashInterval = 0.12f;

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 3f;

        boxCollider = GetComponent<BoxCollider2D>();
        originalColliderSize = boxCollider.size;
        originalColliderOffset = boxCollider.offset;

        originalScale = transform.localScale;

        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
    }

    void Update()
    {
        if (isDead) return;

        // 生命值持续减少
        currentHealth -= healthDecreaseRate * Time.deltaTime;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (currentHealth <= 0) Die();

        // 地面检测
        isGrounded = groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (isGrounded) jumpCount = 0;

        // 跳跃输入
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;

        // Dash 输入
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && !isDashing && !isSliding && currentHealth > dashCost)
            StartCoroutine(Dash());

        // Slide 输入
        if ((Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
            && isGrounded && !isSliding && !isDashing && currentHealth > slideCost)
            StartCoroutine(Slide());
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // 向前移动（Dash / Slide 时速度各自处理）
        if (!isDashing)
        {
            float speed = forwardSpeed;
            if (isSliding)
                speed *= slideSpeedMultiplier;

            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
        }

        // 跳跃 & 二段跳
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
                if (healthSlider != null) healthSlider.value = currentHealth;
                jumpCount = 2;
            }
            jumpPressed = false;
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        currentHealth -= dashCost;
        if (healthSlider != null) healthSlider.value = currentHealth;

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
        if (healthSlider != null) healthSlider.value = currentHealth;

        // 保存原始碰撞体和玩家缩放
        Vector2 originalSize = boxCollider.size;
        Vector2 originalOffset = boxCollider.offset;
        Vector3 originalPlayerScale = transform.localScale;

        // 改变碰撞体和玩家视觉大小
        boxCollider.size = slideColliderSize;
        boxCollider.offset = slideColliderOffset;
        transform.localScale = new Vector3(originalPlayerScale.x, originalPlayerScale.y * slideScaleY, originalPlayerScale.z);

        // 增加速度
        float originalSpeed = forwardSpeed;
        forwardSpeed *= slideSpeedMultiplier;

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 恢复碰撞体、玩家视觉大小和速度
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
            if (healthSlider != null) healthSlider.value = currentHealth;
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
        if (healthSlider != null) healthSlider.value = currentHealth;

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
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
