using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject explosionPrefab; // Prefab vụ nổ (đã có script tự hủy)
    [SerializeField] private float arcHeight = 2.0f; 
    [SerializeField] private float targetOffset = 0.5f; // Bắn vào ngực

    private TowerData _data;
    private Enemy _target;
    private float _projectileDuration;

    private bool _isArcing; 
    private Vector3 _startPosition; 
    private float _travelTime; 
    private float _totalTravelTime;

    // Update is called once per frame
    void Update()
    {
        if (_target == null || !_target.gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            return; 
        }

        if (_isArcing)
        {
            MoveArc();
        }
        else
        {
            MoveStraight();
        }
    }

    private void MoveStraight()
    {
        _projectileDuration -= Time.deltaTime;
        if (_projectileDuration <= 0) 
        {
            gameObject.SetActive(false);
            return;
        }

        Vector3 targetPos = _target.transform.position + Vector3.up * targetOffset;
        Vector3 direction = (targetPos - transform.position).normalized; // Hướng về ngực
        float distanceToTarget = Vector3.Distance(transform.position, targetPos);
        float step = _data.projectileSpeed * Time.deltaTime;

        transform.right = direction; 

        // Kiểm tra trúng đích sớm
        if (distanceToTarget <= step)
        {
            transform.position = targetPos; // Dịch chuyển tới đích cho đẹp
            HandleHitEnemy(_target);
            return;
        }

        transform.position += direction * step;
    }

    private void MoveArc()
    {
        _travelTime += Time.deltaTime;
        float progress = _travelTime / _totalTravelTime;
        if (progress >= 1.0f) progress = 1.0f;

        Vector3 targetPos = _target.transform.position + Vector3.up * targetOffset;
        Vector3 currentPos = Vector3.Lerp(_startPosition, targetPos, progress);
        currentPos.y += arcHeight * Mathf.Sin(progress * Mathf.PI);

        Vector3 direction = currentPos - transform.position;
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        transform.position = currentPos;

        if (progress >= 1.0f || Vector3.Distance(currentPos, targetPos) < 0.1f)
        {
            HandleHitEnemy(_target);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && enemy == _target)
            {
                HandleHitEnemy(enemy);
            }
        }
    }

    public void Shoot(TowerData data, Enemy target, bool useArcing) 
    {
        _data = data;
        _target = target;
        _projectileDuration = data.projectileDuration;
        _isArcing = useArcing;

        if (_isArcing)
        {
            _startPosition = transform.position;
            _travelTime = 0;
            
            // Tính toán lại thời gian bay dựa trên khoảng cách và tốc độ
            float distance = Vector3.Distance(_startPosition, target.transform.position);
            _totalTravelTime = distance / data.projectileSpeed;
            if(_totalTravelTime < 0.1f) _totalTravelTime = 0.1f; 
        }
    }

    private void HandleHitEnemy(Enemy enemy)
    {
        enemy.TakeDamage(_data.damage);
        gameObject.SetActive(false);
        // StartCoroutine(ExplosionProcess());
    }

}
