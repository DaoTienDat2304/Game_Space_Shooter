using System.Collections;
using UnityEngine;

/// <summary>
/// Script tạo wave (làn sóng) enemies
/// </summary>
[System.Serializable]
public class WaveData
{
    [Tooltip("Prefab Enemy sẽ spawn")]
    public GameObject enemyPrefab;

    [Tooltip("Số lượng enemies trong wave")]
    public int enemyCount = 5;

    [Tooltip("Tốc độ di chuyển của enemies")]
    public float enemySpeed = 3f;

    [Tooltip("Thời gian giữa mỗi enemy spawn")]
    public float spawnInterval = 0.5f;

    [Tooltip("Các điểm đường đi (path points) cho enemies")]
    public Transform[] pathPoints;
}

public class Wave : MonoBehaviour
{
    [Header("Wave Settings")]
    [Tooltip("Thông tin của wave này")]
    public WaveData waveData;

    [Tooltip("Enemy có xoay theo đường đi không?")]
    public bool rotateByPath = false;

    [Tooltip("Wave có lặp lại không?")]
    public bool loop = false;

    [Tooltip("Màu hiển thị đường đi trong Editor")]
    public Color pathColor = Color.yellow;

    private void Start()
    {
        if (waveData == null)
        {
            Debug.LogError($"Wave '{gameObject.name}' không có waveData! Không thể spawn enemies!");
            return;
        }

        if (waveData.enemyPrefab == null)
        {
            Debug.LogError($"Wave '{gameObject.name}' có waveData nhưng enemyPrefab = null! " +
                $"EnemyCount: {waveData.enemyCount}, SpawnInterval: {waveData.spawnInterval}. " +
                "Hãy gán enemyPrefab trong Inspector!");
            return;
        }

        if (waveData.enemyCount <= 0)
        {
            Debug.LogWarning($"Wave '{gameObject.name}' có enemyCount = {waveData.enemyCount}. Không spawn enemies.");
            return;
        }

        StartCoroutine(SpawnWave());
    }

    /// <summary>
    /// Spawn các enemies trong wave
    /// </summary>
    IEnumerator SpawnWave()
    {
        for (int i = 0; i < waveData.enemyCount; i++)
        {
            // Tạo enemy
            GameObject newEnemy = Instantiate(
                waveData.enemyPrefab,
                GetSpawnPosition(),
                Quaternion.identity
            );

            // Nếu có path points, thêm component di chuyển theo đường
            if (waveData.pathPoints != null && waveData.pathPoints.Length > 0)
            {
                AddPathMovement(newEnemy);
            }
            else
            {
                // Nếu không có path, enemy di chuyển thẳng xuống
                AddStraightMovement(newEnemy);
            }

            // Đợi trước khi spawn enemy tiếp theo
            yield return new WaitForSeconds(waveData.spawnInterval);
        }

        // Nếu loop = true, spawn lại wave sau 3 giây
        if (loop)
        {
            yield return new WaitForSeconds(3f);
            StartCoroutine(SpawnWave());
        }
    }

    /// <summary>
    /// Lấy vị trí spawn (phía TRÊN màn hình, X ngẫu nhiên - CHẮC CHẮN không ở giữa)
    /// </summary>
    Vector3 GetSpawnPosition()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Spawn ở phía TRÊN màn hình (y = 1.15), X ngẫu nhiên (CHẮC CHẮN tránh giữa 0.4-0.6)
            float randomX;
            // Chọn ngẫu nhiên trái HOẶC phải (không bao giờ chọn giữa)
            if (Random.value < 0.5f)
            {
                randomX = Random.Range(0.15f, 0.38f); // Bên trái (tránh giữa)
            }
            else
            {
                randomX = Random.Range(0.62f, 0.85f); // Bên phải (tránh giữa)
            }
            
            Vector3 topRandom = mainCam.ViewportToWorldPoint(new Vector3(randomX, 1.15f, mainCam.nearClipPlane));
            topRandom.z = 0;
            return topRandom;
        }
        // Fallback: random X (chắc chắn tránh giữa)
        float fallbackX = Random.value < 0.5f ? Random.Range(-3f, -0.5f) : Random.Range(0.5f, 3f);
        return new Vector3(fallbackX, 6.5f, 0);
    }

    /// <summary>
    /// Thêm component di chuyển theo đường cho enemy
    /// </summary>
    void AddPathMovement(GameObject enemy)
    {
        EnemyPathMover mover = enemy.GetComponent<EnemyPathMover>();
        if (mover == null)
        {
            mover = enemy.AddComponent<EnemyPathMover>();
        }

        mover.pathPoints = waveData.pathPoints;
        mover.speed = waveData.enemySpeed;
        mover.rotateByPath = rotateByPath;
    }

    /// <summary>
    /// Thêm component di chuyển thẳng xuống cho enemy
    /// </summary>
    void AddStraightMovement(GameObject enemy)
    {
        EnemyStraightMover mover = enemy.GetComponent<EnemyStraightMover>();
        if (mover == null)
        {
            mover = enemy.AddComponent<EnemyStraightMover>();
        }

        mover.speed = waveData.enemySpeed;
    }

    // Vẽ đường đi trong Editor (chỉ hiển thị khi chọn Wave)
    private void OnDrawGizmosSelected()
    {
        if (waveData != null && waveData.pathPoints != null && waveData.pathPoints.Length > 1)
        {
            Gizmos.color = pathColor;
            for (int i = 0; i < waveData.pathPoints.Length - 1; i++)
            {
                if (waveData.pathPoints[i] != null && waveData.pathPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(
                        waveData.pathPoints[i].position,
                        waveData.pathPoints[i + 1].position
                    );
                }
            }
        }
    }
}
