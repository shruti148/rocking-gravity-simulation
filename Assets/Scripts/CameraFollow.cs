using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;   // Drag Player here
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(3, 5, -10); // Default offset

    private float fixedY;

    void Start()
    {
        fixedY = transform.position.y; // Lock camera height
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Follow only X; Y is fixed; Z fixed at -10
        Vector3 targetPosition = new Vector3(player.position.x + offset.x, fixedY, -10f);

        // Smooth follow
        Vector3 smoothed = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);

        transform.position = smoothed;
    }
}
