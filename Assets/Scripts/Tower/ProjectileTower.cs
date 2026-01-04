using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Kế thừa từ BaseTower thay vì MonoBehaviour
public class ProjectileTower : BaseTower
{
    private ObjectPooler _projectilePool;
    private float _shootTimer;

    [Header("Tower Settings")]
    [SerializeField] private Transform firePoint; // Kéo GameObject con (nòng súng)
    [SerializeField] private bool useArcing = false; // Tích vào nếu là tháp pháo bắn đường cong

    // "override" hàm Start() của BaseTower
    protected override void Start()
    {
        base.Start(); // <-- Rất quan trọng: Gọi hàm Start() của BaseTower trước

        // Đây là logic khởi tạo của riêng ProjectileTower
        _projectilePool = GetComponent<ObjectPooler>();
        if (firePoint == null) 
        {
            firePoint = transform;
            Debug.LogWarning($"Chưa gán FirePoint cho tháp {gameObject.name}, đang dùng vị trí mặc định.");
        }

        if (transform.GetChild(0).GetComponent<Animator>())
        {
            transform.GetChild(0).GetComponent<Animator>().Play("Idle");
        }
        
        _shootTimer = 0f;
    }
    
    // Lớp này có hàm Update() riêng
    private void Update()
    {
        _shootTimer -= Time.deltaTime;
        if(_shootTimer <= 0)
        {
            _shootTimer = data.shootInterval;
            
            // Dọn dẹp danh sách (hàm này từ BaseTower)
            CleanUpEnemyList(); 

            // Nếu có kẻ thù trong tầm (danh sách này từ BaseTower)
            if (_enemiesInRange.Count > 0)
            {
                Shoot();
            }
        }
    }

    // Hàm Shoot() là của riêng lớp này
    private void Shoot()
    {
        // 1. Tìm mục tiêu xịn nhất
        Enemy target = GetEnemyClosestToEnd();

        // Nếu không tìm thấy ai (hoặc list rỗng) thì không bắn
        if (target == null) return;

        // 2. Lấy đạn từ Pool
        GameObject projectileObj = _projectilePool.GetPooledObject();
        projectileObj.transform.position = firePoint.position;
        projectileObj.SetActive(true);
        
        // 3. Bắn vào target vừa tìm được
        Projectile projectileScript = projectileObj.GetComponent<Projectile>();
        projectileScript.Shoot(data, target, useArcing);

        // 4. Âm thanh
        if (!string.IsNullOrEmpty(data.shootSoundName))
        {
            AudioManager.Instance.PlaySFX(data.shootSoundName);
        }
    }

    // Hàm tìm quái gần đích nhất
    protected Enemy GetEnemyClosestToEnd()
    {
        // 1. Dọn dẹp danh sách (xóa quái null hoặc đã chết)
        CleanUpEnemyList(); 

        if (_enemiesInRange.Count == 0) return null;

        Enemy bestCandidate = null;
        float minDistanceToWaypoint = float.MaxValue;
        int maxWaypointIndex = -1; // -1 là chưa đi được đâu

        foreach (Enemy enemy in _enemiesInRange)
        {
            // Bỏ qua nếu quái lỗi
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            // Lấy thông tin của quái
            int enemyIndex = enemy.CurrentWaypointIndex;
            float dist = enemy.GetDistanceToTarget();

            // SO SÁNH:
            // Tiêu chí 1: Ai có Waypoint Index cao hơn thì người đó gần đích hơn
            if (enemyIndex > maxWaypointIndex)
            {
                bestCandidate = enemy;
                maxWaypointIndex = enemyIndex;
                minDistanceToWaypoint = dist;
            }
            // Tiêu chí 2: Nếu cùng Waypoint Index, ai gần cái Waypoint đó hơn thì thắng
            else if (enemyIndex == maxWaypointIndex)
            {
                if (dist < minDistanceToWaypoint)
                {
                    bestCandidate = enemy;
                    minDistanceToWaypoint = dist;
                }
            }
        }

        return bestCandidate;
    }
}