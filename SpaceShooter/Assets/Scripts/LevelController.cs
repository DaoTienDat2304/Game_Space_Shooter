using System.Collections;
using UnityEngine;

/// <summary>
/// Script điều khiển level: spawn waves, power ups, planets
/// </summary>
[System.Serializable]
public class EnemyWave
{
    [Tooltip("Prefab Wave sẽ spawn")]
    public GameObject wavePrefab;
    
    [Tooltip("Thời gian delay trước khi spawn wave này (tùy chọn, 0 = dùng delayBetweenWaves mặc định)")]
    public float customDelay = 0f;
}

public class LevelController : MonoBehaviour
{
    [Header("Enemy Waves")]
    [Tooltip("Danh sách các waves sẽ spawn")]
    public EnemyWave[] waves;

    [Header("Power Up Settings")]
    [Tooltip("Prefab Power Up")]
    public GameObject powerUpPrefab;

    [Tooltip("Thời gian giữa mỗi Power Up (giây)")]
    public float powerUpInterval = 15f;

    [Header("Planet Settings (Optional)")]
    [Tooltip("Prefabs các hành tinh")]
    public GameObject[] planetPrefabs;

    [Tooltip("Thời gian giữa mỗi hành tinh")]
    public float planetInterval = 8f;

    [Tooltip("Tốc độ di chuyển của hành tinh")]
    public float planetSpeed = 3f;

    private Camera mainCamera;
    private float gameStartTime;

    private void Start()
    {
        mainCamera = Camera.main;
        gameStartTime = Time.time;

        // Bắt đầu spawn waves
        StartCoroutine(SpawnWaves());

        // Bắt đầu spawn power ups
        if (powerUpPrefab != null)
        {
            StartCoroutine(SpawnPowerUps());
        }

        // Bắt đầu spawn planets nếu có
        if (planetPrefabs != null && planetPrefabs.Length > 0)
        {
            StartCoroutine(SpawnPlanets());
        }
    }

    private GameObject currentWaveInstance;
    
    [Header("Wave Spawn Settings")]
    [Tooltip("Thời gian chờ giữa các wave (giây) - Áp dụng cho tất cả waves")]
    public float delayBetweenWaves = 8f;

    [Tooltip("Thời gian delay cho wave đầu tiên (giây) - Wave 1 sẽ spawn sau bao lâu?")]
    public float firstWaveDelay = 0f;

    [Tooltip("Đợi hết enemies của wave trước mới spawn wave tiếp theo? (false = chỉ đợi delayBetweenWaves)")]
    public bool waitForEnemiesToClear = false;
    
    /// <summary>
    /// Coroutine spawn các waves - Tự động tính delay dựa trên delayBetweenWaves
    /// </summary>
    IEnumerator SpawnWaves()
    {
        if (waves == null || waves.Length == 0)
        {
            Debug.LogWarning("Chưa có waves nào được cấu hình!");
            yield break;
        }

        // Đợi delay cho wave đầu tiên (nếu có)
        if (firstWaveDelay > 0)
        {
            yield return new WaitForSeconds(firstWaveDelay);
        }

        int waveIndex = 0;
        foreach (EnemyWave wave in waves)
        {
            waveIndex++;

            // Đợi delay giữa các wave (trừ wave đầu tiên)
            if (currentWaveInstance != null)
            {
                if (waitForEnemiesToClear)
                {
                    // Đợi hết enemies của wave trước
                    yield return StartCoroutine(WaitForWaveComplete());
                }
                else
                {
                    // Sử dụng customDelay nếu có, không thì dùng delayBetweenWaves
                    float delay = wave.customDelay > 0 ? wave.customDelay : delayBetweenWaves;
                    yield return new WaitForSeconds(delay);
                    
                    // Hủy wave instance cũ (nếu còn)
                    if (currentWaveInstance != null)
                    {
                        Destroy(currentWaveInstance);
                        currentWaveInstance = null;
                    }
                }
            }

            // Spawn wave - Kiểm tra kỹ trước khi spawn
            if (wave.wavePrefab == null)
            {
                Debug.LogError($"Wave {waveIndex} prefab không được gán! Bỏ qua wave này.");
                continue;
            }

            if (PlayerMovement.instance == null)
            {
                Debug.LogWarning("Player không tồn tại! Không thể spawn wave.");
                continue;
            }

            currentWaveInstance = Instantiate(wave.wavePrefab, Vector3.zero, Quaternion.identity);
            
            // Kiểm tra wave instance có component Wave không và waveData có đúng không
            Wave waveComponent = currentWaveInstance.GetComponent<Wave>();
            if (waveComponent != null && waveComponent.waveData != null)
            {
                if (waveComponent.waveData.enemyPrefab == null)
                {
                    Debug.LogError($"Wave {waveIndex} prefab được spawn nhưng enemyPrefab = null! " +
                        "Hãy kiểm tra lại Wave prefab trong Inspector và gán enemyPrefab!");
                }
            }

            Debug.Log($"✅ Spawned Wave {waveIndex}/{waves.Length} - Prefab: {wave.wavePrefab.name}");
        }
    }

    /// <summary>
    /// Đợi đến khi wave hiện tại hoàn thành (hết enemies)
    /// </summary>
    IEnumerator WaitForWaveComplete()
    {
        // Đợi 3 giây đầu (để đảm bảo enemies spawn xong)
        yield return new WaitForSeconds(3f);

        // Kiểm tra liên tục cho đến khi hết enemies
        int consecutiveEmptyChecks = 0;
        const int requiredEmptyChecks = 3; // Phải kiểm tra 3 lần liên tiếp không có enemies
        
        while (true)
        {
            // Đếm số enemies còn lại
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            if (enemies == null || enemies.Length == 0)
            {
                consecutiveEmptyChecks++;
                
                // Nếu kiểm tra 3 lần liên tiếp không có enemies, wave đã hoàn thành
                if (consecutiveEmptyChecks >= requiredEmptyChecks)
                {
                    // Không còn enemies, wave đã hoàn thành
                    Debug.Log("Wave completed! Spawning next wave...");
                    
                    // Đợi 1 giây trước khi spawn wave tiếp theo
                    yield return new WaitForSeconds(1f);
                    
                    // Hủy wave instance cũ SAU KHI đảm bảo không còn enemies (KHÔNG hủy nếu còn enemies)
                    if (currentWaveInstance != null)
                    {
                        Destroy(currentWaveInstance);
                        currentWaveInstance = null;
                    }
                    
                    break;
                }
            }
            else
            {
                // Còn enemies, reset counter
                consecutiveEmptyChecks = 0;
            }

            // Kiểm tra lại sau 0.5 giây
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Coroutine spawn Power Ups định kỳ
    /// </summary>
    IEnumerator SpawnPowerUps()
    {
        while (true)
        {
            yield return new WaitForSeconds(powerUpInterval);

            if (PlayerMovement.instance != null && powerUpPrefab != null)
            {
                Vector3 spawnPosition = GetRandomSpawnPosition();
                Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);
                Debug.Log("Spawned Power Up!");
            }
        }
    }

    /// <summary>
    /// Coroutine spawn Planets định kỳ
    /// </summary>
    IEnumerator SpawnPlanets()
    {
        // Kiểm tra nếu không có planet prefabs thì không chạy coroutine
        if (planetPrefabs == null || planetPrefabs.Length == 0)
        {
            yield break; // Dừng coroutine nếu không có planet prefabs
        }

        yield return new WaitForSeconds(10f); // Đợi 10 giây trước khi spawn hành tinh đầu tiên

        while (true)
        {
            yield return new WaitForSeconds(planetInterval);

            // Kiểm tra kỹ trước khi spawn
            if (PlayerMovement.instance != null && planetPrefabs != null && planetPrefabs.Length > 0)
            {
                // Chọn ngẫu nhiên một hành tinh (đảm bảo prefab không null)
                int randomIndex = Random.Range(0, planetPrefabs.Length);
                GameObject planetPrefab = planetPrefabs[randomIndex];
                
                if (planetPrefab != null)
                {
                    Vector3 spawnPosition = GetRandomSpawnPosition();
                    
                    GameObject planet = Instantiate(planetPrefab, spawnPosition, Quaternion.identity);
                    
                    // Thêm component di chuyển thẳng xuống cho hành tinh
                    PlanetMover mover = planet.GetComponent<PlanetMover>();
                    if (mover == null)
                    {
                        mover = planet.AddComponent<PlanetMover>();
                    }
                    mover.speed = planetSpeed;

                    Debug.Log("Spawned Planet!");
                }
            }
        }
    }

    /// <summary>
    /// Lấy vị trí spawn ngẫu nhiên phía trên màn hình (TRONG màn hình, không ra ngoài)
    /// </summary>
    Vector3 GetRandomSpawnPosition()
    {
        if (mainCamera != null && PlayerMovement.instance != null)
        {
            // Lấy viewport borders
            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

            // Spawn TRONG màn hình, cách viền một chút
            // Power Up spawn Ở GIỮA màn hình (không gần enemies ở trên)
            float minX = bottomLeft.x + 1f;  // Cách viền trái 1 unit
            float maxX = topRight.x - 1f;    // Cách viền phải 1 unit
            float spawnY = (topRight.y + bottomLeft.y) * 0.5f + 1f; // Ở GIỮA màn hình, hơi cao hơn giữa một chút

            float randomX = Random.Range(minX, maxX);
            return new Vector3(randomX, spawnY, 0);
        }

        // Fallback nếu không có camera
        return new Vector3(Random.Range(-3f, 3f), 4f, 0f);
    }
}
