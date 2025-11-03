using UnityEngine;
using System;
using UnityEngine.UI; // Cần cho "Action"

public class Platform : MonoBehaviour
{
    // --- SỰ KIỆN MỚI ---
    // Gửi đi platform TRỐNG
    public static event Action<Platform> OnEmptyPlatformClicked;
    // Gửi đi tháp ĐÃ CÓ
    public static event Action<BaseTower> OnTowerClicked;
    
    // Biến để lưu tháp đang đứng trên nó
    private BaseTower _towerOnPlatform;
    
    // (Biến static này là từ code UIController của bạn)
    public static bool towerPanelOpen = false; 

    // Hàm này được gọi từ UIController
    public void PlaceTower(TowerData towerData)
    {
        GameObject towerObj = Instantiate(towerData.prefab, transform.position, Quaternion.identity, transform);

        // transform.GetComponent<SpriteRenderer>().enabled(false);
        _towerOnPlatform = towerObj.GetComponent<BaseTower>();
    }

    private void OnMouseDown()
    {
        // Nếu đang mở panel (tower hoặc upgrade), không làm gì cả
        if (towerPanelOpen) return; 

        if (_towerOnPlatform != null)
        {
            // Đã có tháp -> Gửi sự kiện OnTowerClicked
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
        // (GameObject của tháp sẽ tự bị hủy bởi BaseTower)
    }
}