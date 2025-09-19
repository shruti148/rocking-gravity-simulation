using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float forwardSpeed = 5f;
    public float jumpForce = 12f;

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
    public float hurtFlashTime = 0.2f;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool jumpPressed = false;
    private bool isDead = false;
    private Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (isDead) return;

        // 生命值持续减少
        currentHealth -= healthDecreaseRate * Time.deltaTime;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }

        // 地面检测
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // 跳跃输入
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpPressed = true;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // 向前移动
        rb.linearVelocity = new Vector2(forwardSpeed, rb.linearVelocity.y);

        // 跳跃
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("Coin"))
        {
            currentHealth += 20f;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            if (healthSlider != null)
                healthSlider.value = currentHealth;

            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Obstacle"))
        {
            TakeDamage(30f);
        }
    }

    void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (spriteRenderer != null)
            StartCoroutine(HurtFlash());

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator HurtFlash()
    {
        spriteRenderer.color = hurtColor;
        yield return new WaitForSeconds(hurtFlashTime);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; // 禁用物理

        Debug.Log("Game Over!");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // UI按钮绑定
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
