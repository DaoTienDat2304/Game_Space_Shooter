using UnityEngine;

/// <summary>
/// Script xử lý việc bắn súng của Player
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [Tooltip("Tần suất bắn (số viên/giây). Số cao = bắn nhanh hơn")]
    public float fireRate = 5f;

    [Tooltip("Prefab đạn của Player")]
    public GameObject projectilePrefab;

    [Header("Weapon Power")]
    [Tooltip("Sức mạnh vũ khí hiện tại (1-4)")]
    [Range(1, 4)]
    public int weaponPower = 1;

    [Tooltip("Sức mạnh vũ khí tối đa")]
    public int maxWeaponPower = 4;

    [Header("Gun Positions")]
    [Tooltip("Vị trí các súng (có thể là child objects hoặc transform riêng)")]
    public Transform[] gunPositions;

    [Header("Muzzle Flash (Optional)")]
    [Tooltip("Prefab muzzle flash effect khi bắn")]
    public GameObject muzzleFlashPrefab;

    // Thời gian lần bắn tiếp theo
    private float nextFireTime;

    // Singleton instance
    public static PlayerShooting instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        // Nếu chưa có gun positions, tự động tìm hoặc tạo
        if (gunPositions == null || gunPositions.Length == 0)
        {
            SetupGunPositions();
        }
    }

    private void Update()
    {
        // Tự động bắn liên tục
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + (1f / fireRate);
        }
    }

    /// <summary>
    /// Tự động setup gun positions nếu chưa có
    /// Tạo các empty objects làm vị trí bắn
    /// </summary>
    void SetupGunPositions()
    {
        // Tạo 3 vị trí bắn: giữa, trái, phải
        gunPositions = new Transform[3];

        // Súng giữa (tại vị trí Player)
        GameObject centerGun = new GameObject("CenterGun");
        centerGun.transform.SetParent(transform);
        centerGun.transform.localPosition = Vector3.zero;
        gunPositions[0] = centerGun.transform;

        // Súng trái
        GameObject leftGun = new GameObject("LeftGun");
        leftGun.transform.SetParent(transform);
        leftGun.transform.localPosition = new Vector3(-0.3f, 0.3f, 0);
        gunPositions[1] = leftGun.transform;

        // Súng phải
        GameObject rightGun = new GameObject("RightGun");
        rightGun.transform.SetParent(transform);
        rightGun.transform.localPosition = new Vector3(0.3f, 0.3f, 0);
        gunPositions[2] = rightGun.transform;
    }

    /// <summary>
    /// Thực hiện bắn dựa trên weapon power
    /// </summary>
    void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Chưa gán Projectile Prefab trong PlayerShooting!");
            return;
        }

        switch (weaponPower)
        {
            case 1:
                // Chỉ bắn 1 viên ở giữa
                CreateProjectile(gunPositions[0].position, 0f);
                break;

            case 2:
                // Bắn 2 viên: trái và phải
                CreateProjectile(gunPositions[1].position, 0f);
                CreateProjectile(gunPositions[2].position, 0f);
                break;

            case 3:
                // Bắn 3 viên: giữa, trái (chéo), phải (chéo)
                CreateProjectile(gunPositions[0].position, 0f);
                CreateProjectile(gunPositions[1].position, -15f); // Chéo trái
                CreateProjectile(gunPositions[2].position, 15f);  // Chéo phải
                break;

            case 4:
                // Bắn 5 viên: giữa, 2 trái, 2 phải
                CreateProjectile(gunPositions[0].position, 0f);
                CreateProjectile(gunPositions[1].position, -15f);
                CreateProjectile(gunPositions[1].position, -30f);
                CreateProjectile(gunPositions[2].position, 15f);
                CreateProjectile(gunPositions[2].position, 30f);
                break;
        }
    }

    /// <summary>
    /// Tạo một viên đạn tại vị trí và góc xoay
    /// </summary>
    void CreateProjectile(Vector3 position, float rotationZ)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, rotationZ);
        GameObject projectile = Instantiate(projectilePrefab, position, rotation);
        
        // Tự động fix particles trong projectile (nếu có)
        if (projectile != null)
        {
            ParticleSystemAutoFix autoFix = projectile.GetComponent<ParticleSystemAutoFix>();
            if (autoFix == null)
            {
                autoFix = projectile.AddComponent<ParticleSystemAutoFix>();
            }
        }

        // Tạo muzzle flash nếu có
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, position, rotation);
            // Tự động fix particles trong muzzle flash (sửa vấn đề làm mờ)
            ParticleSystemAutoFix flashFix = flash.GetComponent<ParticleSystemAutoFix>();
            if (flashFix == null)
            {
                flashFix = flash.AddComponent<ParticleSystemAutoFix>();
            }
            // Tự hủy sau 0.1 giây
            Destroy(flash, 0.1f);
        }
    }

    /// <summary>
    /// Tăng sức mạnh vũ khí (gọi khi nhặt Power Up)
    /// </summary>
    public void IncreaseWeaponPower()
    {
        if (weaponPower < maxWeaponPower)
        {
            weaponPower++;
            Debug.Log($"Vũ khí tăng lên cấp {weaponPower}!");
        }
        else
        {
            Debug.Log("Vũ khí đã ở mức tối đa!");
        }
    }
}
