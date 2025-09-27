using UnityEngine;

public class PatternPrefabsSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform player;            // Player
    public float spawnDistance = 30f;   // Distance ahead of player to spawn
    public float defaultSegmentLength = 10f; // Default width when no Ground present

    [Header("Level Segment Prefabs")]
    public GameObject[] patterns;

    [Header("Spawn Parent")]
    public Transform spawnParent;

    [Header("Auto Despawn")]
    public float despawnDistance = 20f; // How far behind the player to despawn prefabs

    private float lastX;

    void Start()
    {
        lastX = player.position.x;

        // Initially spawn 3 segments
        for (int i = 0; i < 3; i++)
            SpawnPattern();
    }

    void Update()
    {
        // Spawn prefabs ahead of the player
        if (player.position.x + spawnDistance > lastX)
            SpawnPattern();

        // Auto-despawn prefabs behind the player
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
                Debug.LogWarning("Ground is missing BoxCollider2D: " + prefab.name);
            }
        }
        else
        {
            Debug.LogWarning("Prefab has no Ground: " + prefab.name);
        }

        // Align on X axis to lastX (align Ground left edge)
        float offsetX = lastX - groundLeftEdge;
        float targetY = 0f; // Ground Y = 0
        newObj.transform.position += new Vector3(offsetX, targetY - groundY, 0);

        // Update lastX to the Ground right edge
        lastX += groundRightEdge - groundLeftEdge;
    }
}
