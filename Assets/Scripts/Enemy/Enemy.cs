using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    public EnemyData Data => data;
    public static event Action<EnemyData> OnEnemyReachedEnd;
    public static event Action<Enemy> OnEnemyDestroyed;

    private Path _currentPath;

    private Vector3 _targetPosition;
    private int _currentWaypoint = 0;
    private float _lives;
    private float _maxLives;

    [SerializeField] private Transform healthBar;
    private Vector3 _healthBarOriginalScale;
    private bool _hasBeenCounted = false;

    private Transform _spriteTransform; // Tham chiếu đến object con chứa sprite
    private Vector3 _spriteOriginalScale; // Scale gốc của sprite

    private List<SoldierAI> _attackers = new List<SoldierAI>();

    private void Awake()
    {
        _healthBarOriginalScale = healthBar.localScale;

        _spriteTransform = transform.GetChild(0); // Giả sử sprite là child đầu tiên
        if (_spriteTransform != null)
        {
            _spriteOriginalScale = _spriteTransform.localScale;
        }
        else
        {
            Debug.LogError("Enemy không tìm thấy sprite child (GetChild(0))!", this);
        }
    }

    private void OnEnable()
    {
        // _currentWaypoint = 0;
        // _targetPosition = _currentPath.GetPosition(_currentWaypoint);
        transform.GetChild(0).GetComponent<Animator>().Play("Run");

        if (healthBar != null && _healthBarOriginalScale != Vector3.zero)
        {
            healthBar.localScale = _healthBarOriginalScale;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_hasBeenCounted) return;

        if (_currentPath == null) 
        {
            // Nếu đường đi đã mất (do reset level), thì quái này cũng nên biến mất
            gameObject.SetActive(false); 
            return;
        }

        _attackers.RemoveAll(s => s == null);

        //Kiểm tra xem có bị chặn không
        if (IsEngaged)
        {
            // BỊ CHẶN: Dừng lại
            // (Bạn có thể thêm logic cho quái đánh trả lính ở đây)
            // Ví dụ: Set animator "IsWalking" = false
            return; // Không di chuyển
        }
        
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, data.speed * Time.deltaTime);

        float relativeDistance = (transform.position - _targetPosition).magnitude;
        if (relativeDistance < 0.1f)
        {
            _currentWaypoint++;
            if (_currentWaypoint < _currentPath.waypoints.Length)
            {
                _targetPosition = _currentPath.GetPosition(_currentWaypoint);
                FlipSprite(_targetPosition);
            }
            else
            {
                _hasBeenCounted = true;
                OnEnemyReachedEnd?.Invoke(data);
                gameObject.SetActive(false);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        _lives -= damage;
        _lives = Mathf.Max(_lives, 0);
        UpdateHealthBar();
        if (_lives <= 0)
        {
            if (!_hasBeenCounted)
            {
                _hasBeenCounted = true;
            }
            OnEnemyDestroyed?.Invoke(this);
            gameObject.SetActive(false);
        }
    }

    private void UpdateHealthBar()
    {
        float healthPercent = _lives / _maxLives;
        Vector3 scale = _healthBarOriginalScale;
        scale.x = _healthBarOriginalScale.x * healthPercent;
        healthBar.localScale = scale;
    }

    public void Initialize(Path path, float healthMultiplier)
    {
        if (path == null)
            {
                Debug.LogError("Enemy được khởi tạo với Path NULL! Hủy Object này.");
                gameObject.SetActive(false);
                return;
            }
        _currentPath = path; // Gán path được truyền vào

        _currentWaypoint = 0;
        _targetPosition = _currentPath.GetPosition(_currentWaypoint);
        // Đặt vị trí của Enemy tại điểm bắt đầu của path
        transform.position = _currentPath.GetPosition(0);

        FlipSprite(_targetPosition);

        _hasBeenCounted = false;
        _maxLives = data.lives * healthMultiplier;
        _lives = _maxLives;
        UpdateHealthBar();
    }

    private void FlipSprite(Vector3 targetPosition)
    {
        if (_spriteTransform == null) return;

        // Tính toán hướng di chuyển ngang
        float directionX = targetPosition.x - transform.position.x;

        if (directionX > 0.01f) //khoảng đệm nhỏ để tránh rung lắc
        {
            // Đang đi sang PHẢI thì localScale.x về giá trị gốc (dương)
            _spriteTransform.localScale = new Vector3(_spriteOriginalScale.x, _spriteOriginalScale.y, _spriteOriginalScale.z);
        }
        else if (directionX < -0.01f)
        {
            // Đang đi sang TRÁI localScale.x về giá trị âm (lật ngược)
            _spriteTransform.localScale = new Vector3(-_spriteOriginalScale.x, _spriteOriginalScale.y, _spriteOriginalScale.z);
        }
    }

    public void SetEngaged(bool engaged, SoldierAI soldier)
    {
        if (engaged)
        {
            // Lính bắt đầu chặn
            if (!_attackers.Contains(soldier))
            {
                _attackers.Add(soldier);
            }
        }
        else
        {
            // Lính hết chặn (do nó chết, hoặc quái chạy xa)
            if (_attackers.Contains(soldier))
            {
                _attackers.Remove(soldier);
            }
        }
    }
    
    public bool IsEngaged => _attackers.Count > 0;
}
