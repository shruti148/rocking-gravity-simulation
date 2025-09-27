
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public float boostDuration = 5f;
    public float speedMultiplier = 2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ActivateBoost(boostDuration, speedMultiplier);
                Destroy(gameObject);
            }
        }
    }
}
