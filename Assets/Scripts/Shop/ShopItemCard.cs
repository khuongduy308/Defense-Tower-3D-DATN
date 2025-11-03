using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemCard : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemCostText;
    [SerializeField] private Button selectButton;

    private ShopItemData _data;

    // Sự kiện public static, giống hệt cách TowerCard hoạt động
    public static event System.Action<ShopItemData> OnShopItemSelected;

    public void Initialize(ShopItemData data)
    {
        _data = data;
        itemIcon.sprite = data.itemIcon;
        itemNameText.text = data.itemName;
        itemCostText.text = data.cost.ToString();
        
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(HandleSelection);
    }

    private void HandleSelection()
    {
        // Gửi sự kiện đi kèm dữ liệu của vật phẩm
        OnShopItemSelected?.Invoke(_data);
    }
}