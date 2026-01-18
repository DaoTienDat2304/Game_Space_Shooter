using UnityEngine;

/// <summary>
/// Script quản lý animation cho Power Up (xoay và nhấp nháy)
/// Có thể dùng Animator hoặc animation code-based
/// </summary>
public class PowerUpAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Tốc độ xoay (độ/giây)")]
    public float rotationSpeed = 90f;

    [Tooltip("Tốc độ scale animation (pulse)")]
    public float pulseSpeed = 2f;

    [Tooltip("Độ lớn của pulse (scale)")]
    public float pulseAmount = 0.2f;

    [Tooltip("Có bật pulse animation không?")]
    public bool enablePulse = true;

    [Tooltip("Có bật rotation không?")]
    public bool enableRotation = true;

    private Vector3 baseScale;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        baseScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Nếu có Animator component, ưu tiên dùng Animator
        if (GetComponent<Animator>() != null)
        {
            enableRotation = false;
            enablePulse = false;
            Debug.Log("Animator detected - using Animator instead of code animation");
        }
    }

    private void Update()
    {
        // Xoay
        if (enableRotation)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

        // Pulse (phóng to thu nhỏ)
        if (enablePulse)
        {
            float scale = baseScale.x + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = baseScale * scale;
        }
    }
}
