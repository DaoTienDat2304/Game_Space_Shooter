using UnityEngine;

/// <summary>
/// Script tự động setup Particle System khi được instantiate
/// Đảm bảo particles luôn có màu sắc đúng và không bị làm mờ
/// Tự động thêm vào tất cả GameObject có Particle System
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemInitializer : MonoBehaviour
{
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        EnsureParticleSystemSetup();
    }

    private void OnEnable()
    {
        // Mỗi khi object được enable (kể cả khi pool/reuse), đảm bảo setup đúng
        EnsureParticleSystemSetup();
    }

    /// <summary>
    /// Đảm bảo Particle System được setup đúng
    /// </summary>
    void EnsureParticleSystemSetup()
    {
        if (ps == null) return;

        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
            renderer.sortingOrder = 10;
            if (renderer.sharedMaterial == null)
                renderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        if (GetComponent<ParticleSystemGuard>() == null)
            gameObject.AddComponent<ParticleSystemGuard>();

        if (ps.isPaused || !ps.isPlaying) ps.Play();
    }
}
