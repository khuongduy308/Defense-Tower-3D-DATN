using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionDuration = 0.5f;
    [SerializeField] private float arcHeight = 2.0f; // Độ cao của đường đạn cong
    
    private TowerData _data;
    // private Vector3 _shootDirection;
    private float _projectileDuration;
    private Enemy _target;

    private bool _isArcing; // Có bắn cong không?
    private Vector3 _startPosition; // Vị trí bắt đầu (dùng cho bắn cong)
    private float _travelTime; // Thời gian đã bay (dùng cho bắn cong)
    private float _totalTravelTime; // Tổng thời gian dự kiến bay

    private bool _isExploding = false;
    [SerializeField] private Renderer projectileRenderer; 
    [SerializeField] private Collider2D projectileCollider;

    private void OnEnable()
    {
        _isExploding = false;
        if (projectileRenderer != null) projectileRenderer.enabled = true;
        if (projectileCollider != null) projectileCollider.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isExploding) return;

        _projectileDuration -= Time.deltaTime; // Luôn giảm thời gian

        // Ktra nếu mục tiêu đã chết, hoặc hết thời gian
        if (_target == null || !_target.gameObject.activeInHierarchy || _projectileDuration <= 0)
        {
            gameObject.SetActive(false);
            return; // Dừng hàm Update
        }

        if (_isArcing)
        {
            // --- LOGIC BẮN CONG (PARABOL) ---
            MoveArc();
        }
        else
        {
            // --- LOGIC BẮN THẲNG (CŨ) ---
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

        Vector3 direction = (_target.transform.position - transform.position).normalized;
        transform.right = direction; // Xoay mũi tên
        transform.position += direction * _data.projectileSpeed * Time.deltaTime;
    }

    // Logic di chuyển cong
    private void MoveArc()
    {
        _travelTime += Time.deltaTime;
        
        // Tính toán tiến độ bay (từ 0 đến 1)
        float progress = _travelTime / _totalTravelTime;

        if (progress >= 1.0f)
        {
            progress = 1.0f;
        }

        // 1. Nội suy tuyến tính vị trí X, Y gốc (đi thẳng từ A đến B)
        Vector3 currentPos = Vector3.Lerp(_startPosition, _target.transform.position, progress);

        // 2. Cộng thêm độ cao Y theo hình Sin (0 ở đầu, 1 ở giữa, 0 ở cuối)
        // Mathf.Sin(progress * Mathf.PI) trả về giá trị hình vòng cung
        currentPos.y += arcHeight * Mathf.Sin(progress * Mathf.PI);

        // 3. Xoay đầu đạn theo hướng di chuyển
        Vector3 direction = currentPos - transform.position;
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // 4. Cập nhật vị trí
        transform.position = currentPos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isExploding) return;

        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && enemy == _target)
            {
                enemy.TakeDamage(_data.damage);
                StartCoroutine(ExplosionProcess());
            }
        }
    }

    private IEnumerator ExplosionProcess()
    {
        _isExploding = true;
        if (projectileRenderer != null) projectileRenderer.enabled = false;
        if (projectileCollider != null) projectileCollider.enabled = false;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(explosionDuration);
        gameObject.SetActive(false);
    }

    // Hàm Shoot được cập nhật để nhận tham số isArcing
    public void Shoot(TowerData data, Enemy target, bool useArcing) 
    {
        _data = data;
        _target = target;
        _projectileDuration = data.projectileDuration; // Dùng làm timeout cho đạn thẳng
        _isArcing = useArcing;

        if (_isArcing)
        {
            _startPosition = transform.position;
            _travelTime = 0;
            
            // Tính toán khoảng cách để xác định thời gian bay
            // Giả sử tốc độ projectileSpeed là đơn vị/giây
            float distance = Vector3.Distance(_startPosition, target.transform.position);
            _totalTravelTime = distance / data.projectileSpeed;
            
            // Đảm bảo thời gian bay không quá nhỏ để tránh lỗi chia cho 0
            if(_totalTravelTime < 0.1f) _totalTravelTime = 0.1f; 
        }
    }

}
