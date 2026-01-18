using UnityEngine;

/// <summary>
/// Script xử lý behavior của Enemy: health, shooting, movement
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Số máu của Enemy")]
    public int health = 2;

    [Tooltip("Sát thương khi va chạm với Player")]
    public int damageOnCollision = 1;

    [Header("Shooting Settings")]
    [Tooltip("Prefab đạn của Enemy")]
    public GameObject projectilePrefab;

    [Tooltip("Xác suất bắn (0-100)")]
    [Range(0, 100)]
    public int shootChance = 30;

    [Tooltip("Thời gian bắn tối thiểu (giây)")]
    public float shootTimeMin = 0.5f;

    [Tooltip("Thời gian bắn tối đa (giây)")]
    public float shootTimeMax = 2f;

    [Header("Effects")]
    [Tooltip("Hiệu ứng nổ khi Enemy chết")]
    public GameObject destructionFX;

    [Tooltip("Hiệu ứng khi Enemy bị trúng đạn")]
    public GameObject hitEffect;

    private bool hasShot = false;

    private void Start()
    {
        // Đảm bảo Enemy có tag "Enemy"
        if (!gameObject.CompareTag("Enemy"))
        {
            Debug.LogWarning($"Enemy {gameObject.name} chưa có tag 'Enemy'!");
        }

        // Đảm bảo SpriteRenderer có sorting layer/order cao (render trước Background)
        SetupSpriteRenderer();

        // Lên lịch bắn ngẫu nhiên
        ScheduleShooting();
    }

    /// <summary>
    /// Setup SpriteRenderer để đảm bảo render trước Background (không bị làm mờ)
    /// </summary>
    void SetupSpriteRenderer()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Đảm bảo sorting layer là "Enemy" (cao hơn Background)
            spriteRenderer.sortingLayerName = "Enemy";
            
            // Đảm bảo sorting order cao (render trước Background)
            if (spriteRenderer.sortingOrder <= 0)
            {
                spriteRenderer.sortingOrder = 5; // Cao hơn Background
            }

            // Đảm bảo color không bị tối (không nhợt nhạt)
            Color currentColor = spriteRenderer.color;
            if (currentColor.a < 0.9f || (currentColor.r + currentColor.g + currentColor.b) / 3f < 0.8f)
            {
                currentColor.r = Mathf.Min(1f, currentColor.r * 1.2f);
                currentColor.g = Mathf.Min(1f, currentColor.g * 1.2f);
                currentColor.b = Mathf.Min(1f, currentColor.b * 1.2f);
                currentColor.a = Mathf.Min(1f, currentColor.a * 1.1f);
                spriteRenderer.color = currentColor;
            }
        }
    }

    /// <summary>
    /// Lên lịch bắn ngẫu nhiên
    /// </summary>
    void ScheduleShooting()
    {
        if (projectilePrefab != null && shootChance > 0)
        {
            // Chọn thời gian bắn ngẫu nhiên
            float shootTime = Random.Range(shootTimeMin, shootTimeMax);
            
            // Tính xác suất bắn
            float randomValue = Random.Range(0f, 100f);
            
            if (randomValue < shootChance)
            {
                Invoke(nameof(Shoot), shootTime);
            }
        }
    }

    /// <summary>
    /// Enemy bắn đạn
    /// </summary>
    void Shoot()
    {
        if (projectilePrefab != null && !hasShot)
        {
            Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            hasShot = true;
        }
    }

    /// <summary>
    /// Enemy nhận damage
    /// </summary>
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                if (effect.GetComponent<ParticleSystemAutoFix>() == null)
                    effect.AddComponent<ParticleSystemAutoFix>();
                Destroy(effect, 0.5f);
            }
        }
    }

    /// <summary>
    /// Xử lý khi Enemy chết
    /// </summary>
    void Die()
    {
        if (destructionFX != null)
        {
            GameObject explosion = Instantiate(destructionFX, transform.position, Quaternion.identity);
            if (explosion.GetComponent<ParticleSystemAutoFix>() == null)
                explosion.AddComponent<ParticleSystemAutoFix>();
        }

        // Hủy Enemy
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu va chạm với Player
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(damageOnCollision);
            }
            // Enemy cũng chết khi va chạm với Player
            Die();
        }
    }
}
