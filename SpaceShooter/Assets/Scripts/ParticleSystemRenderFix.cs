using UnityEngine;

/// <summary>
/// Script sửa vấn đề particles bị làm mờ (đặc biệt gần Player)
/// Đảm bảo particles render đúng với sorting layer và material
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemRenderFix : MonoBehaviour
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
        FixParticleRendering();
    }

    private void OnEnable()
    {
        // Mỗi khi enable, đảm bảo render đúng
        FixParticleRendering();
    }

    /// <summary>
    /// Sửa particle rendering để không bị làm mờ
    /// </summary>
    void FixParticleRendering()
    {
        if (psRenderer == null) return;

        // Đảm bảo renderer enable
        psRenderer.enabled = true;

        // Đảm bảo sorting layer cao (render trước Player)
        psRenderer.sortingLayerName = "Default";
        psRenderer.sortingOrder = 10; // Cao hơn Player (Player thường là 0-5)

        // Đảm bảo material không null và đúng - Dùng shader particles tốt hơn
        if (psRenderer.sharedMaterial == null || psRenderer.sharedMaterial.shader.name == "Sprites/Default")
        {
            // Thử dùng shader particles chuyên dụng
            Shader particleShader = Shader.Find("Sprites/Default");
            
            // Nếu có URP, thử dùng shader particles URP
            if (particleShader == null || particleShader.name == "Hidden/InternalErrorShader")
            {
                // Fallback: Tạo material mới với shader mặc định
                particleShader = Shader.Find("Sprites/Default");
            }
            
            if (particleShader != null && particleShader.name != "Hidden/InternalErrorShader")
            {
                Material particleMaterial = new Material(particleShader);
                
                // Đảm bảo material color không bị tối (tint = white)
                if (particleMaterial.HasProperty("_Color"))
                {
                    particleMaterial.SetColor("_Color", Color.white);
                }
                if (particleMaterial.HasProperty("_TintColor"))
                {
                    particleMaterial.SetColor("_TintColor", Color.white);
                }
                
                psRenderer.material = particleMaterial;
            }
        }
        else
        {
            // Nếu đã có material, đảm bảo color tint = white (không bị tối)
            Material mat = psRenderer.material;
            if (mat != null)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color currentColor = mat.GetColor("_Color");
                    if (currentColor.r < 0.9f || currentColor.g < 0.9f || currentColor.b < 0.9f || currentColor.a < 0.9f)
                    {
                        mat.SetColor("_Color", Color.white);
                    }
                }
                if (mat.HasProperty("_TintColor"))
                {
                    Color tintColor = mat.GetColor("_TintColor");
                    if (tintColor.r < 0.9f || tintColor.g < 0.9f || tintColor.b < 0.9f || tintColor.a < 0.9f)
                    {
                        mat.SetColor("_TintColor", Color.white);
                    }
                }
            }
        }

        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        if (ps != null)
        {
            var main = ps.main;
            if (main.startSize.constant < 0.1f)
            {
                var size = main.startSize;
                size.constant = Mathf.Max(0.1f, size.constant);
                main.startSize = size;
            }
        }
    }
}
