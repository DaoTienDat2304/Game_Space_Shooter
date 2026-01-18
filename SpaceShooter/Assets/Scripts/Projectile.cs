using UnityEngine;

/// <summary>
/// Script xử lý đạn (của Player và Enemy)
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("Sát thương của đạn")]
    public int damage = 1;

    [Tooltip("Tốc độ đạn")]
    public float speed = 10f;

    [Tooltip("Direction: 1 = lên trên (Player), -1 = xuống dưới (Enemy)")]
    public Vector2 direction = Vector2.up;

    [Tooltip("Đây có phải đạn của Enemy không?")]
    public bool isEnemyBullet = false;

    [Tooltip("Đạn có bị hủy khi va chạm không?")]
    public bool destroyOnHit = true;

    private void Start()
    {
        // Đảm bảo có tag "Projectile"
        if (!gameObject.CompareTag("Projectile"))
        {
            Debug.LogWarning($"Projectile {gameObject.name} chưa có tag 'Projectile'!");
        }

        // Tự động xác định hướng dựa trên isEnemyBullet
        if (isEnemyBullet)
        {
            direction = Vector2.down; // Enemy bắn xuống
        }
        else
        {
            direction = Vector2.up; // Player bắn lên
        }

        // Đảm bảo SpriteRenderer có sorting layer/order cao (render trước Background)
        SetupSpriteRenderer();
    }

    /// <summary>
    /// Setup SpriteRenderer để đảm bảo render trước Background (không bị làm mờ)
    /// </summary>
    void SetupSpriteRenderer()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Đảm bảo sorting layer là "Projectile" (cao hơn Background)
            spriteRenderer.sortingLayerName = "Projectile";
            
            // Đảm bảo sorting order cao (render trước Background)
            // Background thường có order = 0, Projectile cần order > 0
            if (spriteRenderer.sortingOrder <= 0)
            {
                spriteRenderer.sortingOrder = 5; // Cao hơn Background
            }

            // Đảm bảo color không bị tối (không nhợt nhạt)
            Color currentColor = spriteRenderer.color;
            if (currentColor.a < 0.9f || (currentColor.r + currentColor.g + currentColor.b) / 3f < 0.8f)
            {
                // Làm sáng màu
                currentColor.r = Mathf.Min(1f, currentColor.r * 1.2f);
                currentColor.g = Mathf.Min(1f, currentColor.g * 1.2f);
                currentColor.b = Mathf.Min(1f, currentColor.b * 1.2f);
                currentColor.a = Mathf.Min(1f, currentColor.a * 1.1f);
                spriteRenderer.color = currentColor;
            }
        }
    }

    private void Update()
    {
        // Di chuyển đạn
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isEnemyBullet)
        {
            // Đạn của Enemy va chạm với Player
            if (collision.CompareTag("Player"))
            {
                PlayerMovement player = collision.GetComponent<PlayerMovement>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }

                if (destroyOnHit)
                {
                    DestroyProjectile();
                }
            }
        }
        else
        {
            // Đạn của Player va chạm với Enemy
            if (collision.CompareTag("Enemy"))
            {
                Enemy enemy = collision.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }

                if (destroyOnHit)
                {
                    DestroyProjectile();
                }
            }
        }
    }

    /// <summary>
    /// Hủy đạn
    /// </summary>
    void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
