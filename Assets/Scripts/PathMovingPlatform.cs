using UnityEngine;

public class PathMovingPlatform : MonoBehaviour
{
    [Header("路径点设置")]
    public Transform[] waypoints;   // 移动路径点
    public float speed = 2f;        // 平台移动速度
    public float waitTime = 0.5f;   // 到达点后停留时间

    private int currentIndex = 0;   // 当前目标点
    private float waitTimer = 0f;

    void Update()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        Vector3 direction = target.position - transform.position;

        if (direction.magnitude < 0.05f)
        {
            // 到达目标点 → 停留
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                currentIndex = (currentIndex + 1) % waypoints.Length;
                waitTimer = 0f;
            }
        }
        else
        {
            // 移动到目标点
            transform.position += direction.normalized * speed * Time.deltaTime;
        }
    }

    // 让玩家跟随平台移动
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            collision.transform.SetParent(transform);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            collision.transform.SetParent(null);
    }
}
