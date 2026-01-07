using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoldierAI : MonoBehaviour
{
    [Header("Soldier Stats")]
    [SerializeField] private float moveSpeed;
    private float _damage;
    [SerializeField] private float attackRange; // Khoảng cách lính đứng để đánh
    [SerializeField] private float attackInterval; // Tốc độ đánh
    [SerializeField] private float aggroRange; // Tầm lính phát hiện quái
    [SerializeField] private float maxChaseDistance;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Transform healthBar;
    private Vector3 _healthBarOriginalScale;
    
    private int _currentHealth;
    private float _attackTimer;
    private Vector2 _guardPost; // Vị trí lính được "giao" để bảo vệ

    private Enemy _currentTarget;
    private List<Enemy> _enemiesInRange = new List<Enemy>();
    private CircleCollider2D _aggroCollider;

    // Quản lý trạng thái
    private enum State { Patrolling, Chasing, Attacking }
    private State _currentState;

    void OnEnable()
    {
        // Lắng nghe sự kiện quái chết
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
        _currentHealth = maxHealth;
        if (healthBar != null && _healthBarOriginalScale != Vector3.zero)
        {
            healthBar.localScale = _healthBarOriginalScale;
        }
    }

    void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
        
        // Khi lính bị "tắt" (chết hoặc về pool), 
        // nó phải "thả" quái vật nó đang đánh
        if (_currentTarget != null)
        {
            _currentTarget.SetEngaged(false, this);
        }
    }

    void Awake()
    {
        _healthBarOriginalScale = healthBar.localScale;
        // Tạo collider "phát hiện" quái
        _aggroCollider = gameObject.AddComponent<CircleCollider2D>();
        _aggroCollider.isTrigger = true;
        _aggroCollider.radius = aggroRange;
    }

    void Start()
    {
        
        _currentState = State.Patrolling;
    }

    public void Initialize(float damageFromTower)
    {
        
        // Ép kiểu float về int nếu biến máu của quái là int, hoặc sửa máu quái thành float
        _damage = damageFromTower; 
        
        // Reset máu lính
        _currentHealth = maxHealth;
        // Reset trạng thái
        _currentState = State.Patrolling;
        _currentTarget = null;

        // 1. Xóa danh sách cũ
        _enemiesInRange.Clear();

        // 2. --- QUÉT THỦ CÔNG NGAY LẬP TỨC ---
        // Vì vừa hồi sinh nên Trigger chưa kịp bắt, ta phải tự đi tìm
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aggroRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy e = hit.GetComponent<Enemy>();
                // Chỉ thêm nếu quái còn sống
                if (e != null && e.gameObject.activeInHierarchy)
                {
                    _enemiesInRange.Add(e);
                }
            }
        }

        UpdateHealthBar();
    }

    // Hàm này được gọi bởi SpawnerTower
    public void SetGuardPost(Vector2 post)
    {
        _guardPost = post;
    }

    void Update()
    {
        // Chạy "bộ não" dựa trên trạng thái
        switch (_currentState)
        {
            case State.Patrolling:
                Patrol();
                break;
            case State.Chasing:
                Chase();
                break;
            case State.Attacking:
                Attack();
                break;
        }

        _attackTimer -= Time.deltaTime;
    }

    // --- LOGIC TRẠNG THÁI ---

    private void Patrol()
    {
        // 1. Tìm quái
        FindNewTarget();
        if (_currentTarget != null)
        {
            _currentState = State.Chasing;
            return;
        }

        // 2. Nếu không có quái, quay về vị trí "chốt"
        float distanceToPost = Vector2.Distance(transform.position, _guardPost);
        if (distanceToPost > 0.1f)
        {
            MoveTowards(_guardPost);
        }
    }

    private void Chase()
    {
        if (_currentTarget == null)
        {
            _currentState = State.Patrolling;
            return;
        }

        float distToPost = Vector2.Distance(transform.position, _guardPost);

        // 1. Nếu quái ra khỏi tầm "aggro", bỏ qua nó
        if (distToPost > aggroRange)
        {
            _currentTarget = null;
            _currentState = State.Patrolling;
            return;
        }

        float distToEnemy = Vector2.Distance(transform.position, _currentTarget.transform.position);
        // 2. Nếu quái trong tầm đánh, chuyển sang đánh
        if (distToEnemy > aggroRange)
        {
            _currentTarget = null;
            _currentState = State.Patrolling;
            return;
        }
        if (distToEnemy <= attackRange)
        {
            _currentState = State.Attacking;
            return;
        }
        
        // 3. Nếu không, tiếp tục đuổi
        MoveTowards(_currentTarget.transform.position);
    }

    private void Attack()
    {
        if (_currentTarget == null)
        {
            _currentState = State.Patrolling;
            return;
        }
        
        // Báo cho quái vật "dừng lại"
        _currentTarget.SetEngaged(true, this);

        float distToPost = Vector2.Distance(transform.position, _guardPost);
        if (distToPost > maxChaseDistance)
        {
            _currentTarget.SetEngaged(false, this); // Nhớ thả quái ra cho nó đi
            _currentTarget = null;
            _currentState = State.Patrolling;
            return;
        }

        // Đánh
        if (_attackTimer <= 0)
        {
            _attackTimer = attackInterval;
            
            // (Tùy chọn) Chạy animation đánh
            // transform.GetComponent<Animator>().Play("Chop");

            _currentTarget.TakeDamage(Mathf.RoundToInt(_damage));
            if (_currentTarget == null)
            {
                return; // Quái đã chết, kết thúc hàm Attack() tại đây
            }
            // --- KẾT THÚC GIẢI PHÁP ---

            Debug.Log("Soldier attacks " + _currentTarget.name);
        }
    }

    // --- LOGIC HỖ TRỢ ---

    private void MoveTowards(Vector2 target)
    {
        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    private void FindNewTarget()
    {
        // Dọn dẹp danh sách
        _enemiesInRange.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);

        if (_enemiesInRange.Count == 0)
        {
            _currentTarget = null;
            return;
        }

        Enemy bestCandidate = null;

        foreach (var enemy in _enemiesInRange)
        {
            if (enemy.AttackerCount == 0)
            {
                bestCandidate = enemy;
                break; // Tìm thấy con ngon ăn rồi, chốt luôn!
            }
        }


        if (bestCandidate == null)
        {
            int minAttackers = int.MaxValue;
            foreach (var enemy in _enemiesInRange)
            {
                if (enemy.AttackerCount < minAttackers)
                {
                    minAttackers = enemy.AttackerCount;
                    bestCandidate = enemy;
                }
            }
        }

        // 3. Gán mục tiêu
        _currentTarget = bestCandidate;
    }

    // Nhận sát thương (ví dụ: bị quái đánh lại)
    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // (Tùy chọn) Chạy animation chết
        Destroy(gameObject); // Hoặc trả về pool
    }

    // --- LOGIC VA CHẠM (CHO VIỆC TÌM QUÁI) ---

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null && !_enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Add(enemy);
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null && _enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Remove(enemy);
            }
        }
    }

    // Khi nghe tin quái chết, dọn dẹp danh sách
    private void HandleEnemyDestroyed(Enemy enemy)
    {
        if (_enemiesInRange.Contains(enemy))
        {
            _enemiesInRange.Remove(enemy);
        }
        if (_currentTarget == enemy)
        {
            _currentTarget = null;
            // Tìm mục tiêu mới ngay lập tức
            FindNewTarget();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null) return;
        float healthPercent = (float)_currentHealth / maxHealth;
        healthPercent = Mathf.Clamp01(healthPercent);

        Vector3 scale = _healthBarOriginalScale;
        scale.x = _healthBarOriginalScale.x * healthPercent;
        healthBar.localScale = scale;
    }
}