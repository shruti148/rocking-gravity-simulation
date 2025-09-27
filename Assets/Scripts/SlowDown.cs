using UnityEngine;

public class SlowDown : MonoBehaviour
{
    public float slowDownDuration = 3f;
    public float speedMultiplier = 0.5f; // 0.5f means 50% of normal speed

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ActivateSlowDown(slowDownDuration, speedMultiplier);
                Destroy(gameObject);
            }
        }
    }
}
