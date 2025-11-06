using UnityEngine;
using System;
using UnityEngine.UI; // Cần cho "Action"

public class Platform : MonoBehaviour
{

    public static event Action<Platform> OnEmptyPlatformClicked;

    public static event Action<BaseTower> OnTowerClicked;

    private BaseTower _towerOnPlatform;
    private SpriteRenderer _spriteRenderer;

    public static bool IsModalPanelOpen = false; 
    
    private void Awake()
    {
        // Lấy SpriteRenderer một lần và lưu vào biến
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            Debug.LogError("Lỗi: Platform này không có SpriteRenderer!", this);
        }
    }

    public void PlaceTower(TowerData towerData)
    {
        GameObject towerObj = Instantiate(towerData.prefab, transform.position, Quaternion.identity, transform);
        _towerOnPlatform = towerObj.GetComponent<BaseTower>();

        if (_towerOnPlatform != null)
        {
            _towerOnPlatform.SetPlatform(this); 
        }

        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = false;
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("Click detected on: " + gameObject.name + ", panelOpen = " + IsModalPanelOpen);
        if (IsModalPanelOpen) return; 

        if (_towerOnPlatform != null)
        {
            OnTowerClicked?.Invoke(_towerOnPlatform);
        }
        else
        {
            // Trống -> Gửi sự kiện OnEmptyPlatformClicked
            OnEmptyPlatformClicked?.Invoke(this);
        }
    }

    // Hàm để xóa tháp khi bán
    public void ClearTower()
    {
        _towerOnPlatform = null;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = true; // Bật lại sprite khi bán tháp
        }
    }

    public void ClearTower_ForUpgrade()
    {
        _towerOnPlatform = null;
        // KHÔNG bật lại sprite, vì tháp mới sẽ được đặt lên ngay
    }
}