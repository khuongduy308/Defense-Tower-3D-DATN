using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerTower : BaseTower
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject soldierPrefab; // Prefab của lính
    [SerializeField] private Transform spawnPoint; // Nơi lính xuất hiện
    [SerializeField] private float spawnInterval = 10f; // Thời gian giữa 2 lần cử lính
    [SerializeField] private int maxSoldiers = 3; // Số lính tối đa

    private float _spawnTimer;
    private List<GameObject> _spawnedSoldiers = new List<GameObject>();

    // Khởi tạo cho SpawnerTower
    protected override void Start()
    {
        base.Start(); // <-- Vẫn gọi BaseTower.Start() để lấy tầm bắn, v.v.
        _spawnTimer = 0;
    }

    // Hàm Update() của tháp này hoàn toàn khác
    private void Update()
    {
        // 1. Dọn dẹp danh sách lính (nếu lính đã chết)
        _spawnedSoldiers.RemoveAll(soldier => soldier == null);

        // 2. Đếm ngược timer
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0)
        {
            _spawnTimer = spawnInterval;

            // 3. Chỉ spawn nếu chưa đạt giới hạn
            if (_spawnedSoldiers.Count < maxSoldiers)
            {
                // Logic: Tháp này chỉ cử lính KHI CÓ KẺ THÙ TRONG TẦM
                //gọi CleanUpEnemyList() và kiểm tra _enemiesInRange.Count > 0
                
                // CleanUpEnemyList();
                // if (_enemiesInRange.Count > 0)
                // {
                //     SpawnSoldier();
                // }

                // Cử lính bất kể có kẻ thù hay không:
                SpawnSoldier();
            }
        }
    }

    private void SpawnSoldier()
    {
        GameObject soldierObj = Instantiate(soldierPrefab, spawnPoint.position, spawnPoint.rotation);
        _spawnedSoldiers.Add(soldierObj);
        
        SoldierAI ai = soldierObj.GetComponent<SoldierAI>();
        if (ai != null)
        {
            // Giao "chốt" cho lính. Lính sẽ quay về đây khi không có quái.
            ai.SetGuardPost(spawnPoint.position);
        }
    }
}