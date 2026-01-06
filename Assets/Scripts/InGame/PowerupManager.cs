using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Cần để kiểm tra click trúng UI

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager Instance { get; private set; }

    [Header("Bomb Settings")]
    [SerializeField] private float bombRadius = 1f;
    [SerializeField] private int bombDamage = 100;
    [SerializeField] private GameObject explosionVFX; // Kéo Prefab hiệu ứng nổ vào đây

    [Header("Targeting")]
    [SerializeField] private Texture2D targetingCursor; // (Tùy chọn) Kéo ảnh con trỏ "nhắm"
    
    // private string _selectedPowerup = null;
    private bool _isAiming = false;

    private string _currentPowerupName;
    private PowerupButton _activeButton; // Lưu tham chiếu nút đang được chọn

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    // Được gọi từ PowerupButton.cs
    public bool SelectPowerup(string name, PowerupButton buttonRef)
    {
        // 1. Nếu đang chọn cái khác, phải hủy cái cũ đi đã
        if (_isAiming)
        {
            CancelAiming();
        }

        // 2. Kiểm tra số lượng
        if (GameManager.Instance.GetPowerupCount(name) > 0)
        {
            _currentPowerupName = name;
            _activeButton = buttonRef; // Lưu lại nút nào đang gọi
            _isAiming = true;
            
            Debug.Log($"Đã chọn {name}. Hãy chạm vào màn hình để sử dụng.");
            return true;
        }
        
        return false;
    }

    private void Update()
    {
        // Nếu không ở trạng thái nhắm, thì không làm gì
        if (!_isAiming) return;

        // Xử lý click chuột
        if (Input.GetMouseButtonDown(0)) // Click chuột trái
        {
            if (IsPointerOverUI()) return;

            // Lấy vị trí click/touch trong thế giới game
            Vector3 inputPos = Input.mousePosition;
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(inputPos);

            // Dùng Item
            if (GameManager.Instance.UsePowerup(_currentPowerupName))
            {
                if (_currentPowerupName == "Bomb")
                {
                    Explode(worldPos);
                }
            }

            // Sau khi dùng xong -> Hủy chế độ nhắm -> Nút UI tự thu nhỏ lại
            CancelAiming();
        }
    }

    private void Explode(Vector2 position)
    {
        Debug.Log("BOOM!");
        
        // 1. Hiển thị hiệu ứng nổ
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, position, Quaternion.identity);
            AudioManager.Instance.PlaySFX("No");
        }

        // 2. Tìm tất cả quái vật trong tầm ảnh hưởng
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, bombRadius);

        foreach (Collider2D hit in hits)
        {
            // 3. Gây sát thương nếu là quái
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(bombDamage);
                }
            }
        }
    }

    public void CancelAiming()
    {
        _isAiming = false;
        _currentPowerupName = null;

        // Báo cho nút UI biết để nó thu nhỏ lại và tắt dấu X
        if (_activeButton != null)
        {
            _activeButton.ResetState();
            _activeButton = null;
        }
    }

    private bool IsPointerOverUI()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            // Chỉ kiểm tra khi ngón tay bắt đầu chạm (Began) để chặn ngay lập tức
            if (touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return true;
            }
        }

        // 2. Kiểm tra chuột (Editor / PC)
        // Lưu ý: Trên mobile Unity đôi khi vẫn hiểu touch là mouse, nên dòng này vẫn cần thiết
        if (EventSystem.current.IsPointerOverGameObject()) return true;

        return false;
    }
}