using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerupButton : MonoBehaviour
{
    [SerializeField] private string powerupName; // Gõ "Bomb" vào đây trong Inspector
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button button;

    private void OnEnable()
    {
        // Đăng ký lắng nghe sự kiện từ GameManager
        GameManager.OnPowerupCountChanged += UpdateButton;
    }

    private void OnDisable()
    {
        GameManager.OnPowerupCountChanged -= UpdateButton;
    }

    private void Start()
    {
        // Lấy số lượng hiện tại khi game bắt đầu
        int currentCount = GameManager.Instance.GetPowerupCount(powerupName);
        UpdateButton(powerupName, currentCount);

        // Gán sự kiện cho nút
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        // Báo cho PowerupManager (Bước 4) là chúng ta muốn dùng bom
        PowerupManager.Instance.SelectPowerup(powerupName);
    }

    // Cập nhật số lượng và trạng thái nút
    private void UpdateButton(string name, int newCount)
    {
        // Chỉ cập nhật nếu đúng là vật phẩm này
        if (name == powerupName)
        {
            countText.text = "x" + newCount;
            // Tắt nút nếu hết bom
            button.interactable = (newCount > 0);
        }
    }
}