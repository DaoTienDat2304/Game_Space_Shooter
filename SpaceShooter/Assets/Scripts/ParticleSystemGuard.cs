using UnityEngine;

/// <summary>
/// Script đảm bảo Particle System LUÔN hoạt động đúng, không bị làm mờ hoặc pause
/// Tự động thêm vào tất cả Particle System để đảm bảo chúng luôn hiển thị đúng
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemGuard : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystemRenderer psRenderer;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        psRenderer = GetComponent<ParticleSystemRenderer>();
    }

    private void Start()
    {
        // Đảm bảo Particle System play khi Start
        if (ps != null)
        {
            EnsureActive();
        }
    }

    private void Update()
    {
        // CHỈ kiểm tra pause/play/disable mỗi frame (nhẹ)
        if (ps != null)
        {
            if (ps.isPaused) ps.Play();
            if (!ps.isPlaying && !ps.isStopped) ps.Play();
            if (psRenderer != null && !psRenderer.enabled) psRenderer.enabled = true;
        }
    }

    private void OnEnable()
    {
        // Khi GameObject được enable, đảm bảo Particle System play
        if (ps != null)
        {
            EnsureActive();
        }
    }

    /// <summary>
    /// Đảm bảo Particle System active và play (chỉ gọi 1 lần trong Start/OnEnable)
    /// </summary>
    void EnsureActive()
    {
        if (ps == null) return;

        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        
        if (ps.isPaused || !ps.isPlaying) ps.Play();

        if (psRenderer != null)
        {
            psRenderer.enabled = true;
            if (psRenderer.sortingOrder < 10) psRenderer.sortingOrder = 10;

            // Fix material color
            if (psRenderer.sharedMaterial == null)
            {
                Material mat = new Material(Shader.Find("Sprites/Default"));
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
                psRenderer.material = mat;
            }
            else
            {
                Material mat = psRenderer.material;
                if (mat != null && mat.HasProperty("_Color"))
                {
                    Color c = mat.GetColor("_Color");
                    if (c.r < 0.95f || c.g < 0.95f || c.b < 0.95f || c.a < 0.95f)
                        mat.SetColor("_Color", Color.white);
                }
            }
        }
    }
}
