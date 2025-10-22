using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private TowerData data;
    private CircleCollider2D _circleCollider;

    private List<Enemy> _enemiesInRange;
    private ObjectPooler _projectilePool;
    private float _shootTimer;

    private void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    private void Start()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _circleCollider.radius = data.range;
        _enemiesInRange = new List<Enemy>();
        _projectilePool = GetComponent<ObjectPooler>();
        transform.GetChild(0).GetComponent<Animator>().Play("Idle");
    }
    
    private void Update()
    {
        _shootTimer -= Time.deltaTime;
        if(_shootTimer <= 0)
        {
            _shootTimer = data.shootInterval;
            Shoot();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, data.range);
    }

    private void OnTriggerEnter2D(Collider2D collision)
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

    private void OnTriggerExit2D(Collider2D collision)
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

    private void Shoot()
    {
        _enemiesInRange.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);

        if (_enemiesInRange.Count > 0)
        {
            GameObject projectile = _projectilePool.GetPooledObject();
            projectile.transform.position = transform.position;
            projectile.SetActive(true);
            // Vector2 _shootDirection = (_enemiesInRange[0].transform.position - transform.position).normalized; //huong cua vien dan
            // projectile.GetComponent<Projectile>().Shoot(data, _shootDirection);

            projectile.GetComponent<Projectile>().Shoot(data, _enemiesInRange[0]);
        }
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        // if (_enemiesInRange.Contains(enemy))
        // {
            _enemiesInRange.Remove(enemy);
        // }
    }
}
