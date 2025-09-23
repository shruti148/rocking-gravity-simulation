using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    [Header("生成设置")]
    public GameObject groundPrefab;
    public GameObject obstaclePrefab;
    public GameObject coinPrefab;
    public float groundLength = 10f;
    public int initialGroundCount = 5;
    public Transform player;

    [Header("障碍 / 硬币设置")]
    public float obstacleChance = 0.3f;
    public float coinChance = 0.5f;
    public float spawnHeight = 1.5f;

    private float spawnX = 0f;

    void Start()
    {
        // 先生成初始地面
        for (int i = 0; i < initialGroundCount; i++)
        {
            SpawnGround();
        }
    }

    void Update()
    {
        // 玩家接近最后一块地面时继续生成
        if (player.position.x + groundLength * 2 > spawnX)
        {
            SpawnGround();
        }
    }

    void SpawnGround()
    {
        GameObject ground = Instantiate(groundPrefab, new Vector3(spawnX, 0, 0), Quaternion.identity);

        // 随机生成障碍
        if (Random.value < obstacleChance)
        {
            Vector3 obstaclePos = new Vector3(spawnX + Random.Range(2f, groundLength - 2f), 0.5f, 0);
            Instantiate(obstaclePrefab, obstaclePos, Quaternion.identity);
        }

        // 随机生成硬币
        if (Random.value < coinChance)
        {
            Vector3 coinPos = new Vector3(spawnX + Random.Range(1f, groundLength - 1f), spawnHeight, 0);
            Instantiate(coinPrefab, coinPos, Quaternion.identity);
        }

        spawnX += groundLength;
    }
}
