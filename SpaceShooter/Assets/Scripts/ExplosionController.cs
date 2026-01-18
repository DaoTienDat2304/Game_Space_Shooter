using System.Collections;
using UnityEngine;

/// <summary>
/// Script quản lý explosion effect với animation và particle system
/// </summary>
public class ExplosionController : MonoBehaviour
{
    [Header("Explosion Settings")]
    [Tooltip("Animation clip của explosion (nếu dùng Animation)")]
    public AnimationClip explosionAnimation;

    [Tooltip("Particle System (nếu dùng Particle System)")]
    public ParticleSystem explosionParticles;

    [Tooltip("Sprites để tạo animation frame-by-frame")]
    public Sprite[] explosionSprites;

    [Tooltip("Thời gian mỗi frame (giây)")]
    public float frameTime = 0.05f;

    [Tooltip("Có dùng sprite animation không?")]
    public bool useSpriteAnimation = false;

    private SpriteRenderer spriteRenderer;
    private int currentFrame = 0;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Nếu có Particle System, play nó - ĐẢM BẢO LUÔN PLAY VÀ KHÔNG BỊ LÀM MỜ
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            SetupParticleSystem(ps);
        }
        
        if (explosionParticles != null)
        {
            SetupParticleSystem(explosionParticles);
        }

        // Nếu có Animator, ưu tiên dùng Animator
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            useSpriteAnimation = false;
            // Animator sẽ tự động play animation
        }
        // Nếu dùng sprite animation frame-by-frame
        else if (useSpriteAnimation && explosionSprites != null && explosionSprites.Length > 0)
        {
            StartCoroutine(PlaySpriteAnimation());
        }
        // Nếu không có gì, tự hủy sau 1 giây
        else
        {
            StartCoroutine(AutoDestroy());
        }
    }

    /// <summary>
    /// Setup Particle System để đảm bảo không bị làm mờ/pause
    /// </summary>
    void SetupParticleSystem(ParticleSystem ps)
    {
        if (ps == null) return;

        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.playOnAwake = true;
        
        // Đảm bảo renderer enable và material đúng
        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
            if (renderer.sharedMaterial == null)
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
            }
        }
        
        // KHÔNG DÙNG Stop/Clear - chỉ đảm bảo đang play
        if (!ps.isPlaying && !ps.isPaused)
        {
            ps.Play();
        }
        else if (ps.isPaused)
        {
            ps.Play();
        }

        // Tự động thêm ParticleSystemGuard nếu chưa có (đảm bảo không bị làm mờ)
        if (ps.gameObject.GetComponent<ParticleSystemGuard>() == null)
        {
            ps.gameObject.AddComponent<ParticleSystemGuard>();
        }
    }

    /// <summary>
    /// Coroutine chạy sprite animation frame-by-frame
    /// </summary>
    IEnumerator PlaySpriteAnimation()
    {
        for (int i = 0; i < explosionSprites.Length; i++)
        {
            if (spriteRenderer != null && explosionSprites[i] != null)
            {
                spriteRenderer.sprite = explosionSprites[i];
            }
            yield return new WaitForSeconds(frameTime);
        }

        // Sau khi animation xong, hủy object
        Destroy(gameObject);
    }

    /// <summary>
    /// Tự hủy sau một khoảng thời gian
    /// </summary>
    IEnumerator AutoDestroy()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    /// <summary>
    /// Tạo explosion tại vị trí (helper method)
    /// </summary>
    public static void CreateExplosion(Vector3 position, GameObject explosionPrefab)
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
            // Tự động fix particles trong explosion (sửa vấn đề làm mờ)
            ParticleSystemAutoFix autoFix = explosion.GetComponent<ParticleSystemAutoFix>();
            if (autoFix == null)
            {
                autoFix = explosion.AddComponent<ParticleSystemAutoFix>();
            }
        }
    }
}
