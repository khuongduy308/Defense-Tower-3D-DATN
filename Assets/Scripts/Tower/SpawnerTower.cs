using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerTower : BaseTower
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int maxSoldiers = 3;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 0.5f; // Bán kính random quanh điểm chốt

    private float _spawnTimer;
    private List<SoldierAI> _spawnedSoldiersAI = new List<SoldierAI>();
    
    // Biến lưu điểm tập kết (Chính là vị trí của Waypoint gần nhất)
    private Vector2 _rallyPoint;
    private bool _hasFoundPath = false;

    protected override void Start()
    {
        base.Start();
        _spawnTimer = 0;

        // Tìm điểm waypoint gần nhất ngay khi bắt đầu
        FindClosestWaypoint();
    }

    private void Update()
    {
        // 1. Dọn dẹp lính chết
        int deadCount = _spawnedSoldiersAI.RemoveAll(ai => ai == null || ai.gameObject == null);
        
        // Nếu có lính chết -> Cập nhật lại vị trí cho những đứa còn sống (để nó đổi chỗ cho tự nhiên)
        if (deadCount > 0) UpdatePositions();

        // 2. Logic Spawn
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0)
        {
            _spawnTimer = spawnInterval;
            if (_spawnedSoldiersAI.Count < maxSoldiers)
            {
                SpawnSoldier();
            }
        }
    }

    private void SpawnSoldier()
    {
        GameObject soldierObj = Instantiate(soldierPrefab, spawnPoint.position, spawnPoint.rotation);
        SoldierAI ai = soldierObj.GetComponent<SoldierAI>();

        if (ai != null)
        {
            ai.Initialize(data.damage);
            _spawnedSoldiersAI.Add(ai);

            // Ra lệnh cho lính đi đến chỗ nấp
            UpdatePositions();
        }
    }

    // --- LOGIC TÌM ĐIỂM & GIAO NHIỆM VỤ ---

    // Hàm 1: Tìm Waypoint gần tháp nhất trong tất cả các Path
    public void FindClosestWaypoint()
    {
        if (LevelManager.Instance == null || LevelManager.Instance.CurrentMapPaths == null) return;

        List<Path> allPaths = LevelManager.Instance.CurrentMapPaths;
        if (allPaths.Count == 0) return;

        float minDistance = float.MaxValue;
        Vector2 towerPos = transform.position;
        _hasFoundPath = false;

        foreach (var path in allPaths)
        {
            if (path == null || path.waypoints == null) continue;

            // Duyệt qua từng waypoint trong path
            foreach (var wp in path.waypoints)
            {
                if (wp == null) continue;

                float dist = Vector2.Distance(towerPos, wp.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    _rallyPoint = wp.transform.position; // Lấy luôn vị trí Waypoint
                    _hasFoundPath = true;
                }
            }
        }
    }

    // Hàm 2: Gán vị trí Random cho lính
    private void UpdatePositions()
    {
        if (_spawnedSoldiersAI.Count == 0) return;

        // Nếu chưa tìm thấy đường (lần đầu có thể chưa load kịp), tìm lại
        if (!_hasFoundPath) FindClosestWaypoint();

        // Nếu vẫn không tìm thấy -> Cho đứng quanh cửa tháp
        Vector2 centerPoint = _hasFoundPath ? _rallyPoint : (Vector2)spawnPoint.position;

        foreach (var soldier in _spawnedSoldiersAI)
        {
            // Random một điểm trong vòng tròn bán kính 0.5f (hoặc patrolRadius)
            Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
            
            // Giao vị trí = Điểm trung tâm + Độ lệch ngẫu nhiên
            soldier.SetGuardPost(centerPoint + randomOffset);
        }
    }

    // Vẽ Debug để xem nó chọn Waypoint nào
    private void OnDrawGizmosSelected()
    {
        if (_hasFoundPath)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_rallyPoint, patrolRadius); // Vẽ vùng tuần tra
            Gizmos.DrawLine(transform.position, _rallyPoint);
        }
    }
}