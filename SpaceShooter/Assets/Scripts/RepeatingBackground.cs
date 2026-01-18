using UnityEngine;

/// <summary>
/// Script tạo background scroll vô tận
/// </summary>
public class RepeatingBackground : MonoBehaviour
{
    [Header("Background Settings")]
    [Tooltip("Kích thước chiều cao của sprite background trong world space")]
    public float verticalSize = 10f;

    [Tooltip("Tốc độ scroll")]
    public float scrollSpeed = 2f;

    private void Start()
    {
        // Đảm bảo Background có sorting layer/order thấp (render sau Projectile)
        SetupSpriteRenderer();
    }

    private void Update()
    {
        // Di chuyển background xuống
        transform.Translate(Vector2.down * scrollSpeed * Time.deltaTime);

        // Nếu background đi xuống quá thấp, đưa lên trên
        if (transform.position.y < -verticalSize)
        {
            RepositionBackground();
        }
    }

    /// <summary>
    /// Setup SpriteRenderer để đảm bảo Background render sau Projectile (không làm mờ projectiles)
    /// </summary>
    void SetupSpriteRenderer()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Đảm bảo sorting layer là "Background" (thấp hơn Projectile)
            spriteRenderer.sortingLayerName = "Background";
            
            // Đảm bảo sorting order thấp (render sau Projectile)
            // Projectile có order = 5, Background cần order < 5
            if (spriteRenderer.sortingOrder >= 5)
            {
                spriteRenderer.sortingOrder = 0; // Thấp hơn Projectile
            }
        }
    }

    /// <summary>
    /// Đặt lại vị trí background lên trên khi đã đi xuống hết
    /// </summary>
    void RepositionBackground()
    {
        Vector2 offset = new Vector2(0, verticalSize * 2f);
        transform.position = (Vector2)transform.position + offset;
    }
}
