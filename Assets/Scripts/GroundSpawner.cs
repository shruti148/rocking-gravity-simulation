using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject groundPrefab;
    public GameObject obstaclePrefab;
    public GameObject coinPrefab;
    public float groundLength = 10f;
    public int initialGroundCount = 5;
    public Transform player;

    [Header("Obstacle / Coin Settings")]
    public float obstacleChance = 0.3f;
    public float coinChance = 0.5f;
    public float spawnHeight = 1.5f;

    private float spawnX = 0f;

    void Start()
    {
        // Spawn initial ground pieces
        for (int i = 0; i < initialGroundCount; i++)
        {
            SpawnGround();
        }
    }

    void Update()
    {
        // Continue spawning when player approaches the last piece
        if (player.position.x + groundLength * 2 > spawnX)
        {
            SpawnGround();
        }
    }

    void SpawnGround()
    {
        GameObject ground = Instantiate(groundPrefab, new Vector3(spawnX, 0, 0), Quaternion.identity);

        // Randomly spawn obstacles
        if (Random.value < obstacleChance)
        {
            Vector3 obstaclePos = new Vector3(spawnX + Random.Range(2f, groundLength - 2f), 0.5f, 0);
            Instantiate(obstaclePrefab, obstaclePos, Quaternion.identity);
        }

        // Randomly spawn coins
        if (Random.value < coinChance)
        {
            Vector3 coinPos = new Vector3(spawnX + Random.Range(1f, groundLength - 1f), spawnHeight, 0);
            Instantiate(coinPrefab, coinPos, Quaternion.identity);
        }

        spawnX += groundLength;
    }
}
