using UnityEngine;
using System.Collections;

/// <summary>
/// Enum định nghĩa các loại đội hình enemies
/// </summary>
public enum EnemyFormation
{
    Straight,    // Thẳng hàng
    V,           // Hình chữ V
    Heart,       // Hình trái tim
    Bow,         // Hình cánh cung (parabola)
    Diamond,     // Hình kim cương
    Circle,      // Hình tròn
    MultipleRows,// Nhiều hàng
    Rhombus,     // Hình thoi
    ZigZag       // Hình zic-zac
}

/// <summary>
/// Script spawn enemies theo đội hình (formation pattern) - HÌNH DẠNG CHUẨN
/// </summary>
public class EnemyFormationSpawner : MonoBehaviour
{
    [Header("Formation Settings")]
    [Tooltip("Loại đội hình enemies")]
    public EnemyFormation formationType = EnemyFormation.Straight;

    [Tooltip("Prefab Enemy sẽ spawn")]
    public GameObject enemyPrefab;

    [Tooltip("Số lượng enemies trong đội hình")]
    public int enemyCount = 5;

    [Tooltip("Khoảng cách giữa các enemies (đơn vị Unity)")]
    public float spacing = 1f;

    [Tooltip("Tốc độ di chuyển của enemies")]
    public float enemySpeed = 3f;

    [Tooltip("Thời gian giữa mỗi enemy spawn (0 = spawn đồng thời)")]
    public float spawnInterval = 0.1f;

    [Header("Formation Parameters")]
    [Tooltip("Bán kính đội hình (cho Circle)")]
    public float radius = 2f;

    [Tooltip("Số hàng (cho MultipleRows, ZigZag)")]
    public int rows = 3;

    [Tooltip("Độ rộng đội hình (cho V, Bow)")]
    public float width = 4f;

    [Tooltip("Độ cao đội hình (cho V, Bow)")]
    public float height = 2f;

    private void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"EnemyFormationSpawner '{gameObject.name}': enemyPrefab = null!");
            return;
        }

        if (enemyCount <= 0)
        {
            Debug.LogWarning($"EnemyFormationSpawner '{gameObject.name}': enemyCount = {enemyCount}");
            return;
        }

        SpawnFormation();
    }

    /// <summary>
    /// Spawn enemies theo đội hình đã chọn
    /// </summary>
    public void SpawnFormation()
    {
        Vector3[] positions = GetFormationPositions();

        if (positions == null || positions.Length == 0)
        {
            Debug.LogError($"Không thể tính toán vị trí đội hình {formationType}!");
            return;
        }

        StartCoroutine(SpawnEnemies(positions));
    }

    /// <summary>
    /// Lấy vị trí spawn cơ bản (phía TRÊN màn hình, X ngẫu nhiên - KHÔNG ở giữa, tính toán để formation vừa màn hình)
    /// </summary>
    Vector3 GetBaseSpawnPosition()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Tính viewport size để đảm bảo formation vừa màn hình
            Vector3 bottomLeft = mainCam.ViewportToWorldPoint(Vector3.zero);
            Vector3 topRight = mainCam.ViewportToWorldPoint(Vector3.one);
            float viewportWidth = topRight.x - bottomLeft.x;
            
            // X ngẫu nhiên: trái (0.2-0.35) hoặc phải (0.65-0.8) - KHÔNG BAO GIỜ giữa
            // Điều chỉnh để formation không ra ngoài màn hình
            float randomX = Random.value < 0.5f 
                ? Random.Range(0.2f, 0.35f) 
                : Random.Range(0.65f, 0.8f);
            
            Vector3 topPos = mainCam.ViewportToWorldPoint(new Vector3(randomX, 1.15f, mainCam.nearClipPlane));
            topPos.z = 0;
            return topPos;
        }
        return new Vector3(Random.value < 0.5f ? -2.5f : 2.5f, 6.5f, 0);
    }

    /// <summary>
    /// Lấy mảng vị trí spawn dựa trên đội hình
    /// </summary>
    Vector3[] GetFormationPositions()
    {
        Vector3 basePosition = GetBaseSpawnPosition();
        Vector3[] positions;

        switch (formationType)
        {
            case EnemyFormation.Straight:
                positions = GetStraightFormation(basePosition);
                break;
            case EnemyFormation.V:
                positions = GetVFormation(basePosition);
                break;
            case EnemyFormation.Heart:
                positions = GetHeartFormation(basePosition);
                break;
            case EnemyFormation.Bow:
                positions = GetBowFormation(basePosition);
                break;
            case EnemyFormation.Diamond:
                positions = GetDiamondFormation(basePosition);
                break;
            case EnemyFormation.Circle:
                positions = GetCircleFormation(basePosition);
                break;
            case EnemyFormation.MultipleRows:
                positions = GetMultipleRowsFormation(basePosition);
                break;
            case EnemyFormation.Rhombus:
                positions = GetRhombusFormation(basePosition);
                break;
            case EnemyFormation.ZigZag:
                positions = GetZigZagFormation(basePosition);
                break;
            default:
                positions = GetStraightFormation(basePosition);
                break;
        }

        // Đảm bảo: 1) Không ở giữa màn hình, 2) Tất cả trong viewport bounds
        // Scale formations dựa trên viewport size để vừa màn hình
        positions = ScaleFormationToFitViewport(positions);
        positions = EnsureNoCenterSpawn(positions);
        positions = ClampToViewportBounds(positions);
        return positions;
    }

    /// <summary>
    /// Scale formation để vừa màn hình (giữ nguyên hình dạng)
    /// </summary>
    Vector3[] ScaleFormationToFitViewport(Vector3[] positions)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null || positions == null || positions.Length == 0) return positions;

        // Tính viewport bounds
        Vector3 bottomLeft = mainCam.ViewportToWorldPoint(Vector3.zero);
        Vector3 topRight = mainCam.ViewportToWorldPoint(Vector3.one);
        float viewportWidth = topRight.x - bottomLeft.x;
        float viewportHeight = topRight.y - bottomLeft.y;

        // Tính bounds hiện tại của formation
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        
        foreach (Vector3 pos in positions)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        float formationWidth = maxX - minX;
        float formationHeight = maxY - minY;

        // Scale nếu formation quá lớn (chỉ scale X để giữ hình dạng, Y tự điều chỉnh)
        float maxAllowedWidth = viewportWidth * 0.6f; // 60% viewport width
        float scaleX = 1f;

        if (formationWidth > maxAllowedWidth)
        {
            scaleX = maxAllowedWidth / formationWidth;
            
            // Scale toàn bộ formation từ center
            Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0);
            
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 offset = positions[i] - center;
                positions[i] = center + offset * scaleX;
            }
        }

        return positions;
    }

    /// <summary>
    /// Đảm bảo không có enemy nào spawn ở giữa màn hình (CHỈ di chuyển cả formation nếu cần)
    /// </summary>
    Vector3[] EnsureNoCenterSpawn(Vector3[] positions)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null || positions == null || positions.Length == 0) return positions;

        const float minCenterX = 0.4f;
        const float maxCenterX = 0.6f;

        // Kiểm tra xem có enemy nào ở giữa màn hình không
        bool hasCenterEnemy = false;
        foreach (Vector3 pos in positions)
        {
            Vector3 viewport = mainCam.WorldToViewportPoint(pos);
            if (viewport.x >= minCenterX && viewport.x <= maxCenterX)
            {
                hasCenterEnemy = true;
                break;
            }
        }

        // Nếu có enemy ở giữa, di chuyển CẢ formation sang trái hoặc phải
        if (hasCenterEnemy)
        {
            Vector3 baseViewport = mainCam.WorldToViewportPoint(positions[0]);
            float offsetX = (baseViewport.x < 0.5f) ? (minCenterX - 0.15f) : (maxCenterX + 0.15f);
            float offsetWorldX = mainCam.ViewportToWorldPoint(new Vector3(offsetX, 0.5f, mainCam.nearClipPlane)).x - 
                                 mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, mainCam.nearClipPlane)).x;
            
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i].x += offsetWorldX;
            }
        }

        return positions;
    }

    /// <summary>
    /// Clamp tất cả positions vào viewport bounds (di chuyển/scale CẢ formation, KHÔNG làm sai hình dạng)
    /// </summary>
    Vector3[] ClampToViewportBounds(Vector3[] positions)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null || positions == null || positions.Length == 0) return positions;

        const float marginX = 0.15f; // Margin ngang (15%)
        const float marginY = 0.1f;  // Margin dọc (10%)

        // Tính bounds của formation
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (Vector3 pos in positions)
        {
            Vector3 viewport = mainCam.WorldToViewportPoint(pos);
            if (viewport.x < minX) minX = viewport.x;
            if (viewport.x > maxX) maxX = viewport.x;
            if (viewport.y < minY) minY = viewport.y;
            if (viewport.y > maxY) maxY = viewport.y;
        }

        // Nếu formation ra ngoài màn hình, di chuyển/scale CẢ formation
        float offsetX = 0f, offsetY = 0f;
        float scaleX = 1f, scaleY = 1f;

        // Kiểm tra X bounds
        if (minX < marginX)
        {
            offsetX = marginX - minX; // Đẩy sang phải
        }
        else if (maxX > 1f - marginX)
        {
            offsetX = (1f - marginX) - maxX; // Đẩy sang trái
        }

        // Kiểm tra Y bounds (chỉ đảm bảo ở trên màn hình)
        if (minY < 1.0f)
        {
            offsetY = 1.0f - minY + 0.1f; // Đẩy lên trên
        }

        // Áp dụng offset/scale cho TẤT CẢ positions (giữ nguyên hình dạng)
        if (offsetX != 0f || offsetY != 0f)
        {
            Vector3 centerWorld = mainCam.ViewportToWorldPoint(new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, mainCam.nearClipPlane));
            
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 viewport = mainCam.WorldToViewportPoint(positions[i]);
                viewport.x += offsetX;
                viewport.y += offsetY;
                
                // Convert lại sang world position
                positions[i] = mainCam.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, mainCam.nearClipPlane));
                positions[i].z = 0;
            }
        }

        return positions;
    }

    // ========== CÁC ĐỘI HÌNH - LOGIC CHUẨN ==========

    /// <summary>
    /// Đội hình thẳng hàng (Straight)
    /// </summary>
    Vector3[] GetStraightFormation(Vector3 center)
    {
        Vector3[] positions = new Vector3[enemyCount];
        float totalWidth = (enemyCount - 1) * spacing;
        float startX = center.x - totalWidth / 2f;

        for (int i = 0; i < enemyCount; i++)
        {
            positions[i] = new Vector3(startX + i * spacing, center.y, 0);
        }

        return positions;
    }

    /// <summary>
    /// Đội hình chữ V (V Formation) - CHUẨN - Đỉnh ở giữa, 2 cánh đối xứng
    /// </summary>
    Vector3[] GetVFormation(Vector3 center)
    {
        Vector3[] positions = new Vector3[enemyCount];
        
        // Đảm bảo số lẻ để có đỉnh V ở giữa
        int midIndex = enemyCount / 2; // Index của đỉnh V (giữa)
        
        for (int i = 0; i < enemyCount; i++)
        {
            int distanceFromCenter = Mathf.Abs(i - midIndex);
            float x = center.x + (i - midIndex) * spacing;
            // Đỉnh ở trên, 2 cánh xuống dần
            float y = center.y - distanceFromCenter * (height / Mathf.Max(1, midIndex));
            
            positions[i] = new Vector3(x, y, 0);
        }

        return positions;
    }

    /// <summary>
    /// Đội hình trái tim (Heart Formation) - CHUẨN - 2 đỉnh, giữa rộng, dưới nhọn
    /// </summary>
    Vector3[] GetHeartFormation(Vector3 center)
    {
        Vector3[] positions = new Vector3[enemyCount];
        int index = 0;
        
        // Row 0: 2 đỉnh trái tim (bắt buộc)
        if (index < enemyCount)
            positions[index++] = new Vector3(center.x - spacing * 0.7f, center.y + spacing * 1.2f, 0);
        if (index < enemyCount)
            positions[index++] = new Vector3(center.x + spacing * 0.7f, center.y + spacing * 1.2f, 0);
        
        // Row 1: Phần giữa rộng nhất (3-5 enemies)
        int row1Count = Mathf.Min(enemyCount - index, Mathf.Max(3, enemyCount / 3));
        for (int i = 0; i < row1Count && index < enemyCount; i++)
        {
            float x = center.x + (i - (row1Count - 1) / 2f) * spacing * 0.8f;
            positions[index++] = new Vector3(x, center.y + spacing * 0.3f, 0);
        }
        
        // Row 2: Phần dưới (enemies còn lại, giảm dần để tạo nhọn)
        int remaining = enemyCount - index;
        int row2Count = Mathf.Max(2, remaining - 1);
        for (int i = 0; i < row2Count && index < enemyCount; i++)
        {
            float x = center.x + (i - (row2Count - 1) / 2f) * spacing * 0.7f;
            positions[index++] = new Vector3(x, center.y - spacing * 0.5f, 0);
        }
        
        // Row 3+: Phần dưới cùng (nhọn nhất - 1-2 enemies)
        while (index < enemyCount)
        {
            positions[index++] = new Vector3(center.x, center.y - spacing * 1.2f, 0);
        }
        
        return positions;
    }

    /// <summary>
    /// Đội hình cánh cung (Bow Formation - Parabola) - CHUẨN
    /// </summary>
    Vector3[] GetBowFormation(Vector3 center)
    {
        Vector3[] positions = new Vector3[enemyCount];

        for (int i = 0; i < enemyCount; i++)
        {
            float t = (float)i / Mathf.Max(1, enemyCount - 1); // 0 đến 1
            float normalizedX = (t - 0.5f) * 2f; // -1 đến 1
            
            float x = center.x + normalizedX * width / 2f;
            float y = center.y - (normalizedX * normalizedX) * height; // Parabola
            
            positions[i] = new Vector3(x, y, 0);
        }

        return positions;
    }

    /// <summary>
    /// Đội hình kim cương (Diamond Formation) - CHUẨN - Đối xứng hoàn hảo
    /// </summary>
    Vector3[] GetDiamondFormation(Vector3 center)
    {
        Vector3[] positions = new Vector3[enemyCount];
        
        // Tính số hàng: Diamond có pattern 1-3-5-7-5-3-1 (hoặc tương tự)
        // Với enemyCount, tính số hàng cần thiết
        int totalRows = 1;
        int enemiesUsed = 1;
        while (enemiesUsed < enemyCount)
        {
            totalRows++;
            if (totalRows <= (totalRows + 1) / 2)
                enemiesUsed += totalRows * 2 - 1;
            else
                enemiesUsed += (totalRows - (totalRows - 1) / 2) * 2 - 1;
        }
        
        int midRow = (totalRows - 1) / 2;
        int index = 0;

        // Top half: Tăng width từ 1 lên max
        for (int row = 0; row <= midRow && index < enemyCount; row++)
        {
            int enemiesInRow = row * 2 + 1;
            for (int col = 0; col < enemiesInRow && index < enemyCount; col++)
            {
                float x = center.x + (col - enemiesInRow / 2f) * spacing;
                float y = center.y + (midRow - row) * spacing;
                positions[index++] = new Vector3(x, y, 0);
            }
        }

        // Bottom half: Giảm width từ max-2 xuống 1 (đối xứng)
        for (int row = midRow + 1; index < enemyCount; row++)
        {
            int enemiesInRow = (midRow * 2 + 1) - (row - midRow) * 2;
            if (enemiesInRow <= 0) break;
            
            for (int col = 0; col < enemiesInRow && index < enemyCount; col++)
            {
                float x = center.x + (col - enemiesInRow / 2f) * spacing;
                float y = center.y - (row - midRow) * spacing;
                positions[index++] = new Vector3(x, y, 0);
            }
        }

        return positions;
    }

    /// <summary>
    /// Đội hình tròn (Circle Formation) - CHUẨN
    /// </summary>
    Vector3[] GetCircleFormation(Vector3 center)
    {
        Vector3[] positions = new Vector3[enemyCount];

        for (int i = 0; i < enemyCount; i++)
        {
            float angle = (float)i / enemyCount * Mathf.PI * 2f;
            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;
            
            positions[i] = new Vector3(x, y, 0);
        }

        return positions;
    }

    /// <summary>
    /// Đội hình nhiều hàng (Multiple Rows Formation) - CHUẨN
    /// </summary>
    Vector3[] GetMultipleRowsFormation(Vector3 center)
    {
        Vector3[] positions = new Vector3[enemyCount];
        int enemiesPerRow = Mathf.CeilToInt((float)enemyCount / rows);
        int index = 0;

        for (int row = 0; row < rows && index < enemyCount; row++)
        {
            for (int col = 0; col < enemiesPerRow && index < enemyCount; col++)
            {
                float x = center.x + (col - enemiesPerRow / 2f) * spacing;
                float y = center.y - row * spacing;
                positions[index++] = new Vector3(x, y, 0);
            }
        }

        return positions;
    }

    /// <summary>
    /// Đội hình thoi (Rhombus Formation) - CHUẨN - Đối xứng hoàn hảo
    /// </summary>
    Vector3[] GetRhombusFormation(Vector3 center)
    {
        Vector3[] positions = new Vector3[enemyCount];
        
        // Tính số hàng: Rhombus có pattern đối xứng
        int sideLength = Mathf.CeilToInt(Mathf.Sqrt(enemyCount));
        int midPoint = (sideLength - 1) / 2;
        int index = 0;

        // Tạo hình thoi đối xứng: tăng từ 1 lên max, rồi giảm về 1
        for (int row = 0; row < sideLength && index < enemyCount; row++)
        {
            int offsetFromCenter = Mathf.Abs(row - midPoint);
            int enemiesInRow = sideLength - offsetFromCenter;
            
            // Đảm bảo ít nhất 1 enemy mỗi hàng
            if (enemiesInRow <= 0) enemiesInRow = 1;
            
            for (int col = 0; col < enemiesInRow && index < enemyCount; col++)
            {
                float x = center.x + (col - (enemiesInRow - 1) / 2f) * spacing;
                float y = center.y + (midPoint - row) * spacing;
                positions[index++] = new Vector3(x, y, 0);
            }
        }

        return positions;
    }

    /// <summary>
    /// Đội hình zic-zac (ZigZag Formation) - CHUẨN
    /// </summary>
    Vector3[] GetZigZagFormation(Vector3 center)
    {
        Vector3[] positions = new Vector3[enemyCount];
        int enemiesPerRow = Mathf.CeilToInt((float)enemyCount / rows);
        int index = 0;

        for (int row = 0; row < rows && index < enemyCount; row++)
        {
            for (int col = 0; col < enemiesPerRow && index < enemyCount; col++)
            {
                float x = center.x + (col - enemiesPerRow / 2f) * spacing;
                float y = center.y - row * spacing;
                
                // Zig-zag: hàng chẵn offset sang phải
                if (row % 2 == 1)
                {
                    x += spacing * 0.5f;
                }
                
                positions[index++] = new Vector3(x, y, 0);
            }
        }

        return positions;
    }

    /// <summary>
    /// Coroutine spawn enemies tại các vị trí đã tính toán
    /// </summary>
    IEnumerator SpawnEnemies(Vector3[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, positions[i], Quaternion.identity);

            // Thêm component di chuyển thẳng xuống
            EnemyStraightMover mover = enemy.GetComponent<EnemyStraightMover>();
            if (mover == null)
            {
                mover = enemy.AddComponent<EnemyStraightMover>();
            }
            mover.speed = enemySpeed;

            if (spawnInterval > 0)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }
}
