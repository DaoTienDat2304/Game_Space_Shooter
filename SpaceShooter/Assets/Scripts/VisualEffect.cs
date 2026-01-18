using System.Collections;
using UnityEngine;

/// <summary>
/// Script quản lý Visual Effect: tự động hủy sau một khoảng thời gian
/// Dùng cho explosion, hit effect, và các VFX khác
/// ĐẢM BẢO đợi particles chạy xong trước khi hủy
/// </summary>
public class VisualEffect : MonoBehaviour
{
    [Header("Destruction Settings")]
    [Tooltip("Thời gian (giây) trước khi object bị hủy")]
    public float destructionTime = 1f;

    [Tooltip("Tự động hủy khi bật GameObject lên?")]
    public bool destroyOnEnable = true;

    [Tooltip("Đợi particles chạy xong trước khi hủy?")]
    public bool waitForParticles = true;

    private ParticleSystem[] particleSystems;

    private void OnEnable()
    {
        // Tìm tất cả Particle Systems trong object
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        
        // Đảm bảo tất cả particles có Guard và RenderFix
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null)
            {
                if (ps.gameObject.GetComponent<ParticleSystemGuard>() == null)
                {
                    ps.gameObject.AddComponent<ParticleSystemGuard>();
                }

                if (ps.gameObject.GetComponent<ParticleSystemRenderFix>() == null)
                {
                    ps.gameObject.AddComponent<ParticleSystemRenderFix>();
                }

                // Đảm bảo sorting order cao
                ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = 10;
                }
            }
        }

        // Thêm AutoFix component để đảm bảo particles được fix
        if (GetComponent<ParticleSystemAutoFix>() == null)
        {
            gameObject.AddComponent<ParticleSystemAutoFix>();
        }

        // Tự động hủy khi GameObject được kích hoạt
        if (destroyOnEnable)
        {
            StartCoroutine(AutoDestroy());
        }
    }

    /// <summary>
    /// Coroutine tự động hủy sau destructionTime hoặc khi particles chạy xong
    /// </summary>
    IEnumerator AutoDestroy()
    {
        // Đợi destructionTime hoặc khi particles chạy xong (lấy giá trị lớn hơn)
        if (waitForParticles && particleSystems != null && particleSystems.Length > 0)
        {
            // Tính thời gian particles cần để chạy xong
            float maxParticleLifetime = 0f;
            foreach (ParticleSystem ps in particleSystems)
            {
                if (ps != null && ps.isPlaying)
                {
                    var main = ps.main;
                    float lifetime = main.startLifetime.constant;
                    if (lifetime > maxParticleLifetime)
                    {
                        maxParticleLifetime = lifetime;
                    }
                }
            }

            // Đợi destructionTime hoặc particle lifetime + 0.5s (đảm bảo particles chạy xong)
            float waitTime = Mathf.Max(destructionTime, maxParticleLifetime + 0.5f);
            yield return new WaitForSeconds(waitTime);

            // Đợi thêm một chút nếu vẫn còn particles active
            while (AreParticlesStillActive())
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            yield return new WaitForSeconds(destructionTime);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Kiểm tra xem còn particles active không
    /// </summary>
    bool AreParticlesStillActive()
    {
        if (particleSystems == null) return false;

        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null && (ps.isPlaying || ps.particleCount > 0))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Hủy ngay lập tức (có thể gọi từ bên ngoài)
    /// </summary>
    public void DestroyImmediately()
    {
        Destroy(gameObject);
    }
}
