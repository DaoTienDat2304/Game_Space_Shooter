using UnityEngine;

/// <summary>
/// Script di chuyển Enemy theo đường path
/// </summary>
public class EnemyPathMover : MonoBehaviour
{
    public Transform[] pathPoints;
    public float speed = 3f;
    public bool rotateByPath = false;

    private int currentPathIndex = 0;
    private bool isMoving = false;

    private void Start()
    {
        if (pathPoints != null && pathPoints.Length > 0)
        {
            isMoving = true;
            // Bắt đầu từ điểm đầu tiên
            transform.position = pathPoints[0].position;
        }
    }

    private void Update()
    {
        if (isMoving && pathPoints != null && pathPoints.Length > 0)
        {
            MoveAlongPath();
        }
    }

    void MoveAlongPath()
    {
        if (currentPathIndex >= pathPoints.Length)
        {
            // Đã đi hết đường, hủy enemy
            Destroy(gameObject);
            return;
        }

        Transform targetPoint = pathPoints[currentPathIndex];

        // Di chuyển đến điểm hiện tại
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            speed * Time.deltaTime
        );

        // Xoay theo hướng di chuyển nếu cần
        if (rotateByPath)
        {
            Vector3 direction = (targetPoint.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }

        // Kiểm tra đã đến điểm hiện tại chưa
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentPathIndex++;
        }
    }
}
