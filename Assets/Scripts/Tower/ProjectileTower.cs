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
        GameObject projectile = _projectilePool.GetPooledObject();
        projectile.transform.position = firePoint.position;
        projectile.SetActive(true);
        
        // _enemiesInRange[0] lấy từ BaseTower
        projectile.GetComponent<Projectile>().Shoot(data, _enemiesInRange[0], useArcing);

        if (!string.IsNullOrEmpty(data.shootSoundName))
        {
            AudioManager.Instance.PlaySFX(data.shootSoundName);
        }
    }
}