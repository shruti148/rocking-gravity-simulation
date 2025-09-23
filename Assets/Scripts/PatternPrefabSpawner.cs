using UnityEngine;

public class PatternPrefabsSpawner : MonoBehaviour
{
    [Header("生成设置")]
    public Transform player;            // 玩家
    public float spawnDistance = 30f;   // 玩家前方多少距离生成
    public float defaultSegmentLength = 10f; // 没有 Ground 时的默认宽度

    [Header("关卡片段 Prefabs")]
    public GameObject[] patterns;

    [Header("生成父物体")]
    public Transform spawnParent;

    [Header("自动销毁")]
    public float despawnDistance = 20f; // 玩家后方多远销毁 prefab

    private float lastX;

    void Start()
    {
        lastX = player.position.x;

        // 初始生成 3 段
        for (int i = 0; i < 3; i++)
            SpawnPattern();
    }

    void Update()
    {
        // 生成玩家前方的 prefab
        if (player.position.x + spawnDistance > lastX)
            SpawnPattern();

        // 自动销毁玩家后方 prefab
        if (spawnParent != null)
        {
            for (int i = spawnParent.childCount - 1; i >= 0; i--)
            {
                Transform child = spawnParent.GetChild(i);
                if (child.position.x + despawnDistance < player.position.x)
                    Destroy(child.gameObject);
            }
        }
    }

    void SpawnPattern()
    {
        if (patterns.Length == 0) return;

        int index = UnityEngine.Random.Range(0, patterns.Length);
        GameObject prefab = patterns[index];

        GameObject newObj = Instantiate(prefab);
        if (spawnParent != null)
            newObj.transform.SetParent(spawnParent);

        Transform ground = newObj.transform.Find("Ground");

        float groundLeftEdge = 0f;
        float groundRightEdge = defaultSegmentLength;
        float groundY = 0f;

        if (ground != null)
        {
            BoxCollider2D groundCollider = ground.GetComponent<BoxCollider2D>();
            if (groundCollider != null)
            {
                float scaleX = ground.localScale.x;
                groundLeftEdge  = ground.position.x + groundCollider.offset.x * scaleX - (groundCollider.size.x * scaleX) / 2f;
                groundRightEdge = ground.position.x + groundCollider.offset.x * scaleX + (groundCollider.size.x * scaleX) / 2f;
                groundY = ground.position.y;
            }
            else
            {
                Debug.LogWarning("Ground 缺少 BoxCollider2D：" + prefab.name);
            }
        }
        else
        {
            Debug.LogWarning("Prefab 没有 Ground：" + prefab.name);
        }

        // X 轴对齐 lastX（Ground 左边缘对齐）
        float offsetX = lastX - groundLeftEdge;
        float targetY = 0f; // 地面 Y=0
        newObj.transform.position += new Vector3(offsetX, targetY - groundY, 0);

        // 更新 lastX 为 Ground 右边缘
        lastX += groundRightEdge - groundLeftEdge;
    }
}
