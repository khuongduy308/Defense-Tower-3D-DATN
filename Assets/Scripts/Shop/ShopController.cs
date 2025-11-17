using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopController : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject shopItemCardPrefab;
    [SerializeField] private Transform shopItemCardContainer;
    [SerializeField] private TMP_Text warningText; // Gán WarningText của bạn vào đây

    [Header("Shop Items")]
    [SerializeField] private ShopItemData[] shopItems; // Kéo các ScriptableObject ShopItemData vào đây

    private List<GameObject> activeCards = new List<GameObject>();

    // (Tùy chọn) Thêm một sự kiện để xử lý đóng băng (sạch sẽ hơn)
    public static event System.Action<float> OnFreezeAllEnemies;

    private void OnEnable()
    {
        // Lắng nghe sự kiện từ ShopItemCard
        ShopItemCard.OnShopItemSelected += HandleShopItemSelected;
    }

    private void OnDisable()
    {
        ShopItemCard.OnShopItemSelected -= HandleShopItemSelected;
    }

    // --- Các hàm Public để gọi từ Button (Nút Mở/Đóng Shop) ---

    public void OpenShopPanel()
    {
        shopPanel.SetActive(true);
        // Tạm dừng game, giống hệt Tower Panel
        Platform.IsModalPanelOpen = true;
        GameManager.Instance.setTimeScale(0f); 
        PopulateShopCards();
    }

    public void CloseShopPanel()
    {
        shopPanel.SetActive(false);
        // Tiếp tục game, dùng tốc độ đã lưu
        Platform.IsModalPanelOpen = false;
        GameManager.Instance.setTimeScale(GameManager.Instance.GameSpeed);
    }

    // --- Logic chính ---

    private void PopulateShopCards()
    {
        // 1. Xóa các card cũ
        foreach (var card in activeCards)
        {
            Destroy(card);
        }
        activeCards.Clear();

        // 2. Tạo card mới
        foreach (var item in shopItems)
        {
            GameObject cardGameObject = Instantiate(shopItemCardPrefab, shopItemCardContainer);
            ShopItemCard card = cardGameObject.GetComponent<ShopItemCard>();
            card.Initialize(item);
            activeCards.Add(cardGameObject);
        }
    }

    private void HandleShopItemSelected(ShopItemData itemData)
    {
        // 1. Luôn đóng panel sau khi chọn
        CloseShopPanel(); 

        // 2. Kiểm tra tiền
        if (GameManager.Instance.Resources >= itemData.cost)
        {
            // 3. Trừ tiền
            GameManager.Instance.SpendResources(itemData.cost);
            
            // 4. Kích hoạt hiệu ứng
            ApplyItemEffect(itemData);
        }
        else
        {
            // 5. Báo lỗi không đủ tiền
            StartCoroutine(ShowWarningMessage("No Resource Enough!"));
        }
    }

    private void ApplyItemEffect(ShopItemData itemData)
    {
        if (itemData.isConsumable)
        {
            GameManager.Instance.AddPowerup(itemData.powerupName, 1);
            return; // neu mua vat pham dung sau thi luu lai roi thoat
        }
        switch (itemData.itemType)
        {
            case ShopItemType.HealPlayer:
                GameManager.Instance.AddLives(itemData.value); 
                break;

            // case ShopItemType.GrantResources:
            //     // Bạn sẽ cần thêm hàm `AddResources` trong GameManager
            //     GameManager.Instance.AddResources(itemData.value); 
            //     break;

            case ShopItemType.GlobalDamage:
                // Tìm tất cả quái vật và gây sát thương
                foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    if (enemy != null && enemy.gameObject.activeInHierarchy)
                    {
                        enemy.TakeDamage(itemData.value); 
                    }
                }
                break;

            case ShopItemType.FreezeEnemies:
                // Gửi sự kiện đóng băng. Script Enemy của bạn nên lắng nghe sự kiện này
                // và tự "đóng băng" trong `itemData.value` giây.
                OnFreezeAllEnemies?.Invoke(itemData.value); 
                break;
        }
    }

    // Copy y hệt hàm ShowWarningMessage từ UIController của bạn
    private IEnumerator ShowWarningMessage(string message)
    {
        if (warningText == null)
        {
            Debug.LogError("LỖI: Biến warningText CHƯA ĐƯỢC GÁN trong Inspector của ShopController!");
            yield break;
        }
    
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        // Phải dùng Realtime vì game đang bị pause (Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(2f); 
        warningText.gameObject.SetActive(false);
    }
}
