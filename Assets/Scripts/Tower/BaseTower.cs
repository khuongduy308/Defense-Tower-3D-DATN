using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Đổi tên "Tower" thành "BaseTower"
public abstract class BaseTower : MonoBehaviour
{
    [SerializeField] protected TowerData data; // Dùng 'protected' để lớp con có thể truy cập
    protected CircleCollider2D _circleCollider;
    protected List<Enemy> _enemiesInRange; // Dùng 'protected'
    protected Platform _parentPlatform;

    // OnEnable / OnDisable để xử lý sự kiện enemy chết
    protected virtual void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    protected virtual void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    public TowerData GetData()
    {
        return data;
    }

    // Start() chỉ khởi tạo những gì chung nhất
    protected virtual void Start()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _circleCollider.radius = data.range;
        _enemiesInRange = new List<Enemy>();
    }
    
    // Vẽ tầm bắn trong Editor
    protected virtual void OnDrawGizmos()
    {
        if (data != null)
        {
            Gizmos.DrawWireSphere(transform.position, data.range);
        }
    }

    // Logic phát hiện kẻ thù vào tầm
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && !_enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Add(enemy);
            }
        }
    }

    // Logic phát hiện kẻ thù ra khỏi tầm
    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && _enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Remove(enemy);
            }
        }
    }

    // Xóa kẻ thù đã chết ra khỏi danh sách
    protected virtual void HandleEnemyDestroyed(Enemy enemy)
    {
        if (_enemiesInRange.Contains(enemy))
        {
            _enemiesInRange.Remove(enemy);
        }
    }

    // Dọn dẹp danh sách trước khi tấn công
    protected void CleanUpEnemyList()
    {
        _enemiesInRange.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);
    }
    
    public void UpgradeTower(TowerData upgradeData)
    {
        // 1. Cập nhật data
        this.data = upgradeData;
        
        // 2. Cập nhật hình ảnh (hủy prefab cũ, tạo prefab mới)
        // (Giả sử hình ảnh/prefab là con đầu tiên của tháp)
        if (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
        Instantiate(upgradeData.prefab, transform.position, transform.rotation, transform);
        
        // 3. Cập nhật các chỉ số khác (nếu cần)
        _circleCollider.radius = data.range;
        // (Bạn có thể cần reset _shootTimer ở đây nếu muốn tháp bắn ngay)
    }

    public void SellTower()
    {
        if (_parentPlatform != null)
        {
            _parentPlatform.ClearTower();
        }

        // 2. Tự hủy
        Destroy(gameObject);
    }

    public Platform GetPlatform()
    {
        return _parentPlatform;
    }

    public void DestroyForUpgrade()
    {
        if (_parentPlatform != null)
        {
            _parentPlatform.ClearTower_ForUpgrade(); // Báo platform là nó đã trống
        }

        Destroy(gameObject);
    }
    
    public void SetPlatform(Platform platform)
    {
        _parentPlatform = platform;
    }
}