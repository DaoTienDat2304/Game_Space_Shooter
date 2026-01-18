using UnityEngine;

/// <summary>
/// Script tự động sửa particles khi GameObject được instantiate
/// Áp dụng cho tất cả particles (explosion, hit effect, etc.)
/// </summary>
public class ParticleSystemAutoFix : MonoBehaviour
{
    private void Start()
    {
        FixAllParticlesInChildren();
    }

    private void OnEnable()
    {
        // Mỗi khi enable, đảm bảo particles được fix
        FixAllParticlesInChildren();
    }

    /// <summary>
    /// Sửa tất cả particles trong GameObject và children
    /// </summary>
    void FixAllParticlesInChildren()
    {
        ParticleSystem[] allParticles = GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in allParticles)
        {
            if (ps != null)
            {
                FixParticleSystem(ps);
            }
        }
    }

    /// <summary>
    /// Sửa một Particle System cụ thể
    /// </summary>
    void FixParticleSystem(ParticleSystem ps)
    {
        if (ps == null) return;

        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            // Đảm bảo renderer enable
            renderer.enabled = true;

            // Đảm bảo sorting order cao (render trước Player, không bị làm mờ)
            renderer.sortingOrder = 10;

            // Đảm bảo material không null
            if (renderer.sharedMaterial == null || renderer.material == null)
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
            }
        }

        var main = ps.main;

        // Đảm bảo simulation space
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        if (ps.isPaused || !ps.isPlaying) ps.Play();

        // Thêm RenderFix component nếu chưa có
        if (ps.gameObject.GetComponent<ParticleSystemRenderFix>() == null)
        {
            ps.gameObject.AddComponent<ParticleSystemRenderFix>();
        }

        // Thêm Guard component nếu chưa có
        if (ps.gameObject.GetComponent<ParticleSystemGuard>() == null)
        {
            ps.gameObject.AddComponent<ParticleSystemGuard>();
        }
    }
}
