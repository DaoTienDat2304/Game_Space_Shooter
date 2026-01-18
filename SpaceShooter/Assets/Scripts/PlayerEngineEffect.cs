using UnityEngine;

/// <summary>
/// Script quản lý hiệu ứng động cơ của Player
/// Tạo particle effect phía sau Player
/// </summary>
public class PlayerEngineEffect : MonoBehaviour
{
    [Header("Engine Effect Settings")]
    [Tooltip("Prefab particle effect của động cơ")]
    public GameObject engineParticlePrefab;

    [Tooltip("Vị trí offset phía sau Player")]
    public Vector3 offset = new Vector3(0, -0.5f, 0);

    private GameObject currentEffect;
    private PlayerMovement player;

    private void Start()
    {
        player = GetComponentInParent<PlayerMovement>();

        // Tạo particle effect nếu có prefab
        if (engineParticlePrefab != null)
        {
            CreateEngineEffect();
        }
    }

    private void Update()
    {
        // Cập nhật vị trí effect theo Player
        if (currentEffect != null && player != null)
        {
            currentEffect.transform.position = player.transform.position + offset;
        }
    }

    /// <summary>
    /// Tạo particle effect động cơ
    /// </summary>
    void CreateEngineEffect()
    {
        if (engineParticlePrefab != null)
        {
            currentEffect = Instantiate(
                engineParticlePrefab,
                transform.position + offset,
                Quaternion.identity
            );
            currentEffect.transform.SetParent(transform);
        }
    }

    /// <summary>
    /// Hủy effect khi Player chết
    /// </summary>
    private void OnDestroy()
    {
        if (currentEffect != null)
        {
            Destroy(currentEffect);
        }
    }
}
