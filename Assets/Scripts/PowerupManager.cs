using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Cần để kiểm tra click trúng UI

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager Instance { get; private set; }

    [Header("Bomb Settings")]
    [SerializeField] private float bombRadius = 2.5f;
    [SerializeField] private int bombDamage = 150;
    [SerializeField] private GameObject explosionVFX; // Kéo Prefab hiệu ứng nổ vào đây

    [Header("Targeting")]
    [SerializeField] private Texture2D targetingCursor; // (Tùy chọn) Kéo ảnh con trỏ "nhắm"
    
    private string _selectedPowerup = null;
    private bool _isAiming = false;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    // Được gọi từ PowerupButton.cs
    public void SelectPowerup(string powerupName)
    {
        // Kiểm tra xem có còn hàng không
        if (GameManager.Instance.GetPowerupCount(powerupName) > 0)
        {
            _isAiming = true;
            _selectedPowerup = powerupName;
            
            // (Tùy chọn) Đổi con trỏ chuột
            if (targetingCursor != null)
            {
                Cursor.SetCursor(targetingCursor, Vector2.zero, CursorMode.Auto);
            }
        }
    }

    private void Update()
    {
        // Nếu không ở trạng thái nhắm, thì không làm gì
        if (!_isAiming) return;

        // Xử lý click chuột
        if (Input.GetMouseButtonDown(0)) // Click chuột trái
        {
            // Kiểm tra xem có click trúng UI không (ví dụ: nút Pause)
            if (EventSystem.current.IsPointerOverGameObject())
            {
                CancelAiming(); // Hủy nếu click trúng UI
                return;
            }

            // Lấy vị trí click trên thế giới
            Vector2 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            // Sử dụng vật phẩm
            if (GameManager.Instance.UsePowerup(_selectedPowerup))
            {
                // Kích hoạt hiệu ứng bom
                if (_selectedPowerup == "Bomb")
                {
                    Explode(clickPosition);
                }
            }
            
            // Dù thành công hay không, hủy trạng thái nhắm
            CancelAiming();
        }
        else if (Input.GetMouseButtonDown(1)) // Click chuột phải để hủy
        {
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

    private void CancelAiming()
    {
        _isAiming = false;
        _selectedPowerup = null;
        // (Tùy chọn) Reset con trỏ chuột
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 
    }
}