using UnityEngine;

// Enum để định nghĩa các loại vật phẩm
public enum ShopItemType
{
    HealPlayer,     // Hồi máu cho nhà chính
    GrantResources, // Tặng thêm tiền
    GlobalDamage,   // Gây sát thương toàn bộ quái
    FreezeEnemies   // Đóng băng toàn bộ quái
}

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Tower Defense/Shop Item")]
public class ShopItemData : ScriptableObject
{
    [Header("Info")]
    public string itemName;
    public Sprite itemIcon;
    [TextArea]
    public string itemDescription;

    [Header("Stats")]
    public int cost;
    public ShopItemType itemType;
    
    [Tooltip("Số lượng (máu hồi, tiền, sát thương, hoặc thời gian đóng băng)")]
    public int value; 

    [Header("Consumable Powerup")]
    [Tooltip("Đánh dấu nếu đây là vật phẩm mua để giữ lai và sử dụng sau")]
    public bool isConsumable;

    [Tooltip("Tên định danh cho vật phẩm, ví dụ: 'Bomb', 'Potion'")]
    public string powerupName;
}