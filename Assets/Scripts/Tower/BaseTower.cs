using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Đổi tên "Tower" thành "BaseTower"
public abstract class BaseTower : MonoBehaviour
{
    [SerializeField] protected TowerData data; // Dùng 'protected' để lớp con có thể truy cập
    [SerializeField] private GameObject rangeCirclePrefab;
    protected List<Enemy> _enemiesInRange; // Dùng 'protected'
    protected Platform _parentPlatform;
    private GameObject _rangeObject;

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
        CreateRangeSensor();
        _enemiesInRange = new List<Enemy>();

        if (rangeCirclePrefab != null)
        {
            _rangeObject = Instantiate(rangeCirclePrefab, transform.position, Quaternion.identity, transform);

            //Scale = Range * 2
            float diameter = data.range * 2f;
            _rangeObject.transform.localScale = new Vector3(diameter, diameter, 1f);

            // Mặc định là ẩn
            _rangeObject.SetActive(false);
        }
    }

    public void ToggleRangeVisual(bool isActive)
    {
        if (_rangeObject != null)
        {
            _rangeObject.SetActive(isActive);
        }
    }

    private void CreateRangeSensor()
    {
        // 1. Tạo GameObject con
        GameObject rangeObj = new GameObject("RangeSensor");
        rangeObj.transform.SetParent(this.transform);
        rangeObj.transform.localPosition = Vector3.zero;

        // 2. Thêm Collider và setup bán kính
        CircleCollider2D rangeCol = rangeObj.AddComponent<CircleCollider2D>();
        rangeCol.isTrigger = true;
        rangeCol.radius = data.range;

        // 3. QUAN TRỌNG: Set Layer để không chặn click chuột
        rangeObj.layer = LayerMask.NameToLayer("Ignore Raycast");

        // 4. Gắn script chuyển tiếp tín hiệu
        TowerRangeSensor sensor = rangeObj.AddComponent<TowerRangeSensor>();
        sensor.Initialize(this);
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
    public virtual void OnEnemyEnterRange(Collider2D collision)
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
    public virtual void OnEnemyExitRange(Collider2D collision)
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

        if (_rangeObject != null)
        {
            float diameter = data.range * 2f;
            _rangeObject.transform.localScale = new Vector3(diameter, diameter, 1f);
        }
        
        // 2. Cập nhật hình ảnh (hủy prefab cũ, tạo prefab mới)
        // (Giả sử hình ảnh/prefab là con đầu tiên của tháp)
        if (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
        Instantiate(upgradeData.prefab, transform.position, transform.rotation, transform);

    //     _circleCollider.radius = data.range;)
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