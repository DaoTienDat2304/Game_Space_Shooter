using UnityEngine;
using UnityEditor;

/// <summary>
/// Script tự động sửa TẤT CẢ Particle System trong scene/prefab với màu sắc đẹp
/// Dùng khi màu bị "kẹt" ở 1 màu tím hoặc không đổi màu
/// </summary>
public class ParticleSystemFixAll : EditorWindow
{
    [MenuItem("Tools/Fix All Particle Systems - Sửa Tất Cả Màu")]
    public static void FixAllParticleSystems()
    {
        // Tìm tất cả Particle System trong scene VÀ prefabs
        ParticleSystem[] sceneParticles = FindObjectsOfType<ParticleSystem>(true);
        
        // Tìm trong prefabs
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        
        int fixedCount = 0;
        
        // Sửa trong scene
        foreach (ParticleSystem ps in sceneParticles)
        {
            if (FixParticleSystem(ps))
            {
                fixedCount++;
            }
        }
        
        // Sửa trong prefabs
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                ParticleSystem[] particles = prefab.GetComponentsInChildren<ParticleSystem>(true);
                foreach (ParticleSystem ps in particles)
                {
                    if (FixParticleSystem(ps))
                    {
                        fixedCount++;
                        EditorUtility.SetDirty(prefab);
                    }
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Hoàn thành!", 
            $"Đã sửa {fixedCount} Particle System(s)!\n\n" +
            "Tất cả Particle Systems (cả scene và prefabs) giờ sẽ có nhiều màu sắc đẹp mắt.", 
            "OK");
        
        Debug.Log($"✅ Đã sửa {fixedCount} Particle System(s)!");
    }

    /// <summary>
    /// Sửa một Particle System cụ thể
    /// </summary>
    private static bool FixParticleSystem(ParticleSystem ps)
    {
        if (ps == null) return false;

        var main = ps.main;
        var colorOverLifetime = ps.colorOverLifetime;

        // BẮT BUỘC bật Color over Lifetime
        colorOverLifetime.enabled = true;

        // Tạo gradient Explosion đẹp mắt (Đỏ → Vàng → Cam → Trong suốt)
        Gradient gradient = new Gradient();
        
        // Color keys: Nhiều màu khác nhau
        GradientColorKey[] colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(1f, 0f, 0f), 0.0f),      // Đỏ ở 0%
            new GradientColorKey(new Color(1f, 1f, 0f), 0.25f),     // Vàng ở 25%
            new GradientColorKey(new Color(1f, 0.8f, 0f), 0.4f),    // Vàng cam ở 40%
            new GradientColorKey(new Color(1f, 0.5f, 0f), 0.6f),    // Cam ở 60%
            new GradientColorKey(new Color(1f, 0.3f, 0f), 0.8f),    // Cam nhạt ở 80%
            new GradientColorKey(new Color(0.8f, 0.2f, 0f), 1.0f)   // Cam đậm ở 100%
        };

        // Alpha keys: Giữ sáng lâu, chỉ fade ở cuối (KHÔNG NHỢT NHẠT)
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1.0f, 0.0f),   // Đậm ở đầu (100%)
            new GradientAlphaKey(1.0f, 0.5f),   // Đậm ở 50% (GIỮ SÁNG)
            new GradientAlphaKey(1.0f, 0.7f),   // Đậm ở 70% (GIỮ SÁNG)
            new GradientAlphaKey(0.8f, 0.85f),  // Hơi trong suốt ở 85%
            new GradientAlphaKey(0.5f, 0.95f),  // Trong suốt hơn ở 95%
            new GradientAlphaKey(0.0f, 1.0f)    // Hoàn toàn trong suốt ở 100%
        };

        gradient.SetKeys(colorKeys, alphaKeys);
        
        // ÁP DỤNG gradient
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        // Đảm bảo Start Color là Gradient SÁNG (mỗi particle có màu khác nhau, KHÔNG NHỢT NHẠT)
        Gradient startGradient = new Gradient();
        startGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 1f, 0.8f), 0f),    // Vàng sáng (KHÔNG TỐI)
                new GradientColorKey(new Color(1f, 0.8f, 0f), 0.5f),  // Cam sáng
                new GradientColorKey(new Color(1f, 0.6f, 0f), 1f)     // Cam đậm (KHÔNG TỐI)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),   // Alpha = 1 (KHÔNG TRONG SUỐT)
                new GradientAlphaKey(1f, 1f)    // Alpha = 1 (KHÔNG TRONG SUỐT)
            }
        );
        main.startColor = new ParticleSystem.MinMaxGradient(startGradient);

        // Đảm bảo Start Lifetime đủ dài
        if (main.startLifetime.constant < 0.5f)
        {
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f);
        }

        // FORCE REFRESH - Bắt buộc phải stop trước khi clear
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear();
        
        // Đảm bảo Simulation Space đúng
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        
        // Đảm bảo không bị pause
        if (ps.isPaused) ps.Play();

        // Thêm ParticleSystemGuard nếu chưa có (đảm bảo không bị pause/làm mờ)
        if (ps.gameObject.GetComponent<ParticleSystemGuard>() == null)
        {
            ps.gameObject.AddComponent<ParticleSystemGuard>();
        }

        // Thêm ParticleSystemInitializer nếu chưa có (đảm bảo setup đúng khi instantiate)
        if (ps.gameObject.GetComponent<ParticleSystemInitializer>() == null)
        {
            ps.gameObject.AddComponent<ParticleSystemInitializer>();
        }

        // Thêm ParticleSystemRenderFix nếu chưa có (sửa vấn đề làm mờ)
        if (ps.gameObject.GetComponent<ParticleSystemRenderFix>() == null)
        {
            ps.gameObject.AddComponent<ParticleSystemRenderFix>();
        }

        // Đảm bảo renderer có sorting order cao và enable
        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 10; // Cao hơn Player
            renderer.enabled = true;
            
            // Đảm bảo material không null
            if (renderer.sharedMaterial == null)
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
            }
        }

        // Đánh dấu đã thay đổi
        EditorUtility.SetDirty(ps);
        EditorUtility.SetDirty(ps.gameObject);
        
        // FORCE UPDATE ngay lập tức
        ps.Simulate(0.1f, true, true);

        return true;
    }

    /// <summary>
    /// Sửa Particle System trong Prefabs
    /// </summary>
    [MenuItem("Tools/Fix Particle Systems in Prefabs")]
    public static void FixParticleSystemsInPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:ParticleSystem", new[] { "Assets/Prefabs" });
        
        int fixedCount = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                ParticleSystem[] particles = prefab.GetComponentsInChildren<ParticleSystem>(true);
                foreach (ParticleSystem ps in particles)
                {
                    if (FixParticleSystem(ps))
                    {
                        fixedCount++;
                    }
                }
                
                if (particles.Length > 0)
                {
                    EditorUtility.SetDirty(prefab);
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Hoàn thành!", 
            $"Đã sửa {fixedCount} Particle System(s) trong Prefabs!", 
            "OK");
    }
}
