using UnityEngine;

public class PatternPrefabsSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform player;            
    public float spawnDistance = 30f;   
    public float defaultSegmentLength = 10f; 

    [Header("Level Segment Prefabs")]
    public GameObject[] patterns;       

    [Header("Spawn Parent")]
    public Transform spawnParent;       

    [Header("Auto Despawn")]
    public float despawnDistance = 20f; 

    private float lastX; 

    void Start()
    {
        // First segment: force place under the player
        SpawnPattern(true);

        // Spawn a few more ahead
        for (int i = 0; i < 2; i++)
            SpawnPattern();
    }

    void Update()
    {
        if (player.position.x + spawnDistance > lastX)
            SpawnPattern();

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

    void SpawnPattern(bool isFirst = false)
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
                Bounds bounds = groundCollider.bounds;
                groundLeftEdge = bounds.min.x;
                groundRightEdge = bounds.max.x;
                groundY = bounds.min.y;
            }
        }

        if (isFirst)
        {
            // ✅ Place the first segment so that its left edge is at player.position.x - 1f
            float targetX = player.position.x - 1f; // leave a little space so player stands on it
            float offsetX = targetX - groundLeftEdge;
            newObj.transform.position += new Vector3(offsetX, -groundY, 0);

            // Update lastX
            lastX = newObj.GetComponentInChildren<BoxCollider2D>().bounds.max.x;
        }
        else
        {
            float offsetX = lastX - groundLeftEdge;
            newObj.transform.position += new Vector3(offsetX, -groundY, 0);

            lastX += groundRightEdge - groundLeftEdge;
        }
    }
}
