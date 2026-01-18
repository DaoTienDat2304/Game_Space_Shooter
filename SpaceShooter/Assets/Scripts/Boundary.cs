using UnityEngine;

/// <summary>
/// Script quản lý boundary (giới hạn màn hình): hủy objects ra khỏi màn hình
/// </summary>
public class Boundary : MonoBehaviour
{
    private BoxCollider2D boundaryCollider;

    private void Start()
    {
        boundaryCollider = GetComponent<BoxCollider2D>();
        
        if (boundaryCollider == null)
        {
            boundaryCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        // Đảm bảo là trigger
        boundaryCollider.isTrigger = true;

        // Tự động resize theo viewport
        ResizeCollider();
    }

    /// <summary>
    /// Resize collider theo viewport của camera
    /// </summary>
    void ResizeCollider()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null && boundaryCollider != null)
        {
            Vector2 viewportSize = mainCam.ViewportToWorldPoint(new Vector2(1, 1)) * 2;
            viewportSize.x *= 1.5f;
            viewportSize.y *= 1.5f;
            boundaryCollider.size = viewportSize;
        }
    }

    /// <summary>
    /// Kiểm tra và hủy objects ra khỏi màn hình (chạy mỗi frame)
    /// </summary>
    private void Update()
    {
        // Kiểm tra tất cả enemies trong scene
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (Camera.main != null)
        {
            float bottomY = Camera.main.ViewportToWorldPoint(Vector2.zero).y - 2f;
            
            foreach (GameObject enemy in enemies)
            {
                // Hủy enemies ở dưới màn hình
                if (enemy != null && enemy.transform.position.y < bottomY)
                {
                    Destroy(enemy);
                }
            }
        }

        // Kiểm tra projectiles
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        if (Camera.main != null)
        {
            float topY = Camera.main.ViewportToWorldPoint(Vector2.one).y + 2f;
            float bottomY = Camera.main.ViewportToWorldPoint(Vector2.zero).y - 2f;
            float leftX = Camera.main.ViewportToWorldPoint(Vector2.zero).x - 2f;
            float rightX = Camera.main.ViewportToWorldPoint(Vector2.one).x + 2f;

            foreach (GameObject projectile in projectiles)
            {
                if (projectile != null)
                {
                    Vector3 pos = projectile.transform.position;
                    // Hủy nếu ra ngoài màn hình
                    if (pos.y > topY || pos.y < bottomY || pos.x < leftX || pos.x > rightX)
                    {
                        Destroy(projectile);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Khi object ra khỏi boundary (exit trigger) - backup method
    /// </summary>
    private void OnTriggerExit2D(Collider2D collision)
    {
        // Hủy projectiles ra khỏi màn hình
        if (collision != null && collision.CompareTag("Projectile"))
        {
            Destroy(collision.gameObject);
        }
        // Hủy bonus ra khỏi màn hình
        else if (collision != null && collision.CompareTag("Bonus"))
        {
            Destroy(collision.gameObject);
        }
        // Hủy enemies ra khỏi màn hình dưới (nếu di chuyển xuống)
        else if (collision != null && collision.CompareTag("Enemy"))
        {
            if (Camera.main != null)
            {
                float bottomY = Camera.main.ViewportToWorldPoint(Vector2.zero).y - 2f;
                // Chỉ hủy nếu enemy ở dưới màn hình
                if (collision.transform.position.y < bottomY)
                {
                    Destroy(collision.gameObject);
                }
            }
        }
    }
}
