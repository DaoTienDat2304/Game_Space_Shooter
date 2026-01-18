using UnityEngine;
using UnityEditor;

/// <summary>
/// Script tự động thêm ParticleSystemGuard vào tất cả Particle Systems
/// Đảm bảo particles không bị làm mờ hoặc pause
/// </summary>
public class AutoAddParticleGuard : Editor
{
    [MenuItem("Tools/Auto Fix Particle Systems - Thêm Guard Cho Tất Cả")]
    public static void AddParticleGuardToAll()
    {
        // Tìm tất cả Particle Systems
        ParticleSystem[] allParticles = FindObjectsOfType<ParticleSystem>(true);
        
        int addedGuardCount = 0;
        int addedInitializerCount = 0;
        
        foreach (ParticleSystem ps in allParticles)
        {
            if (ps != null)
            {
                if (ps.gameObject.GetComponent<ParticleSystemGuard>() == null)
                {
                    ps.gameObject.AddComponent<ParticleSystemGuard>();
                    EditorUtility.SetDirty(ps.gameObject);
                    addedGuardCount++;
                }

                if (ps.gameObject.GetComponent<ParticleSystemInitializer>() == null)
                {
                    ps.gameObject.AddComponent<ParticleSystemInitializer>();
                    EditorUtility.SetDirty(ps.gameObject);
                    addedInitializerCount++;
                }
            }
        }

        // Tìm trong prefabs
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                ParticleSystem[] particles = prefab.GetComponentsInChildren<ParticleSystem>(true);
                foreach (ParticleSystem ps in particles)
                {
                    if (ps != null)
                    {
                        if (ps.gameObject.GetComponent<ParticleSystemGuard>() == null)
                        {
                            ps.gameObject.AddComponent<ParticleSystemGuard>();
                            EditorUtility.SetDirty(prefab);
                            addedGuardCount++;
                        }

                        if (ps.gameObject.GetComponent<ParticleSystemInitializer>() == null)
                        {
                            ps.gameObject.AddComponent<ParticleSystemInitializer>();
                            EditorUtility.SetDirty(prefab);
                            addedInitializerCount++;
                        }
                    }
                }
            }
        }
        
        AssetDatabase.SaveAssets();

        int totalCount = addedGuardCount + addedInitializerCount;
        EditorUtility.DisplayDialog("Hoàn thành!", 
            $"Đã thêm vào {totalCount} Particle System(s):\n" +
            $"- ParticleSystemGuard: {addedGuardCount}\n" +
            $"- ParticleSystemInitializer: {addedInitializerCount}\n\n" +
            "Particle Systems giờ sẽ không bị làm mờ hoặc pause nữa!", 
            "OK");
        
        Debug.Log($"✅ Đã thêm ParticleSystemGuard vào {addedGuardCount} và ParticleSystemInitializer vào {addedInitializerCount} Particle System(s)!");
    }
}
