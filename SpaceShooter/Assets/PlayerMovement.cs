using UnityEngine;

/// <summary>
/// Script điều khiển Player: di chuyển theo chuột, quản lý borders, health và destruction
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Tốc độ di chuyển")]
    public float moveSpeed = 30f;
    
    [Tooltip("Khoảng cách offset từ viền màn hình (0.0 = sát viền hoàn toàn, 0.1 = rất sát)")]
    public float borderOffset = 0.1f;
    
    [Tooltip("Tự động tính offset dựa trên kích thước sprite Player (khuyến nghị: true)")]
    public bool autoCalculateOffset = true;

    [Header("Health Settings")]
    [Tooltip("Số máu của Player")]
    public int health = 3;

    [Header("Destruction Effect")]
    [Tooltip("Prefab hiệu ứng nổ khi Player chết")]
    public GameObject destructionFX;

    // Borders của màn hình (tự động tính toán)
    private float minX, maxX, minY, maxY;
    private Camera mainCamera;

    // Singleton instance để các script khác có thể truy cập
    public static PlayerMovement instance;

    private void Awake()
    {
        // Đảm bảo chỉ có 1 instance
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        CalculateBorders();
        
        // Đảm bảo Player có tag "Player"
        if (!gameObject.CompareTag("Player"))
        {
            Debug.LogWarning("Player chưa có tag 'Player'! Hãy thêm tag trong Inspector.");
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
            // Đảm bảo sorting layer là "Player" (cao hơn Background)
            spriteRenderer.sortingLayerName = "Player";
            
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

    private void Update()
    {
        HandleMovement();
    }

    /// <summary>
    /// Xử lý di chuyển Player theo vị trí touch/mouse (hỗ trợ cả desktop và mobile)
    /// </summary>
    void HandleMovement()
    {
        Vector2 screenPosition = Vector2.zero;
        bool hasInput = false;

        // Kiểm tra touch TRƯỚC (mobile)
        if (Input.touchCount > 0)
        {
            // Mobile: Lấy vị trí touch đầu tiên (kéo đến đâu máy bay theo đó)
            Touch touch = Input.touches[0];
            screenPosition = touch.position;
            hasInput = true;
        }
        // Kiểm tra mouse SAU (desktop - chỉ khi không có touch)
        else
        {
            // Desktop/Editor: Lấy vị trí chuột (luôn có giá trị, kể cả khi không click)
            Vector3 mousePos = Input.mousePosition;
            if (mousePos.x >= 0 && mousePos.y >= 0)
            {
                screenPosition = mousePos;
                hasInput = true;
            }
        }

        // Nếu không có input, không di chuyển
        if (!hasInput)
        {
            return;
        }

        // Validate screen position (tránh lỗi trên mobile)
        screenPosition.x = Mathf.Clamp(screenPosition.x, 0, Screen.width);
        screenPosition.y = Mathf.Clamp(screenPosition.y, 0, Screen.height);

        // Convert sang world space
        Vector3 targetPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane));
        targetPosition.z = 0;

        // Di chuyển mượt mà đến vị trí target
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            moveSpeed * Time.deltaTime
        );

        // Giới hạn Player trong borders
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minX, maxX),
            Mathf.Clamp(transform.position.y, minY, maxY),
            0
        );
    }

    /// <summary>
    /// Tính toán borders dựa trên viewport của camera
    /// </summary>
    void CalculateBorders()
    {
        // Lấy góc dưới trái và trên phải của viewport
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(Vector2.zero);
        Vector3 topRight = mainCamera.ViewportToWorldPoint(Vector2.one);

        // Tính offset dựa trên kích thước sprite nếu cần
        float offset = borderOffset;
        if (autoCalculateOffset)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                // Lấy kích thước thực tế của sprite trong world space
                Bounds spriteBounds = spriteRenderer.bounds;
                float spriteWidth = spriteBounds.extents.x;
                float spriteHeight = spriteBounds.extents.y;
                
                // Offset = một nửa kích thước sprite + margin nhỏ nhất để sprite không bị cắt
                offset = Mathf.Max(spriteWidth, spriteHeight) + 0.05f;
            }
            else
            {
                // Fallback nếu không có sprite
                offset = borderOffset;
            }
        }

        // Tính borders với offset (cho phép player di chuyển sát viền hơn)
        minX = bottomLeft.x + offset;
        minY = bottomLeft.y + offset;
        maxX = topRight.x - offset;
        maxY = topRight.y - offset;
    }

    /// <summary>
    /// Player nhận damage
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
            Debug.Log($"Player nhận damage! Health còn lại: {health}");
        }
    }

    /// <summary>
    /// Xử lý khi Player chết
    /// </summary>
    void Die()
    {
        if (destructionFX != null)
        {
            GameObject explosion = Instantiate(destructionFX, transform.position, Quaternion.identity);
            if (explosion.GetComponent<ParticleSystemAutoFix>() == null)
                explosion.AddComponent<ParticleSystemAutoFix>();
        }

        Debug.Log("Player đã chết!");
        
        // Hủy Player
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu va chạm với Enemy
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                TakeDamage(enemy.damageOnCollision);
                // Enemy cũng bị hủy khi va chạm với Player
                enemy.TakeDamage(999);
            }
            else
            {
                TakeDamage(1);
            }
        }
    }
}
