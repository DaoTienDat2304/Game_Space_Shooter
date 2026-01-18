using UnityEngine;

/// <summary>
/// Script xử lý Power Up Bonus: tăng sức mạnh vũ khí cho Player
/// </summary>
public class Bonus : MonoBehaviour
{
    [Header("Bonus Settings")]
    [Tooltip("Tốc độ rơi xuống")]
    public float fallSpeed = 2f;

    [Tooltip("Tốc độ xoay")]
    public float rotationSpeed = 90f;

    private void Start()
    {
        // Đảm bảo có tag "Bonus"
        if (!gameObject.CompareTag("Bonus"))
        {
            Debug.LogWarning($"Bonus {gameObject.name} chưa có tag 'Bonus'!");
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
            // Đảm bảo sorting layer là "Bonus" (cao hơn Background)
            spriteRenderer.sortingLayerName = "Bonus";
            
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
        // Rơi xuống
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);

        // Xoay
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Khi Player chạm vào Bonus
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Tăng sức mạnh vũ khí cho Player
            PlayerShooting shooting = collision.GetComponent<PlayerShooting>();
            if (shooting != null)
            {
                shooting.IncreaseWeaponPower();
                Debug.Log("Player nhận Power Up!");
            }

            // Hủy Bonus
            Destroy(gameObject);
        }
    }
}
