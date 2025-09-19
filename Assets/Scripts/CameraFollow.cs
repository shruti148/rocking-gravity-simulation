using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;   // 拖 Player
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(3, 5, -10); // 默认偏移

    private float fixedY;

    void Start()
    {
        fixedY = transform.position.y; // 锁定相机高度
    }

    void LateUpdate()
    {
        if (player == null) return;

        // 只跟随 X，Y 固定，Z 固定为 -10
        Vector3 targetPosition = new Vector3(player.position.x + offset.x, fixedY, -10f);

        // 平滑跟随
        Vector3 smoothed = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);

        transform.position = smoothed;
    }
}
