using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Đổi tên "Tower" thành "BaseTower"
public abstract class BaseTower : MonoBehaviour
{
    [SerializeField] protected TowerData data; // Dùng 'protected' để lớp con có thể truy cập
    protected CircleCollider2D _circleCollider;
    protected List<Enemy> _enemiesInRange; // Dùng 'protected'

    // OnEnable / OnDisable để xử lý sự kiện enemy chết
    protected virtual void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    protected virtual void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
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
}