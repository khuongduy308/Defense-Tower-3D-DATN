using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerupButton : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] private string powerupName; // Tên item: "Bomb"
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button mainButton; // Nút chính (hình quả bom)

    [Header("Selection Visuals")]
    [SerializeField] private Button cancelButton; // Nút dấu X (nhỏ)
    [SerializeField] private float scaleMultiplier = 1.2f; // Tỉ lệ phóng to (1.2 lần)

    private Vector3 _originalScale;
    private bool _isSelected = false;

    private void OnEnable()
    {
        GameManager.OnPowerupCountChanged += UpdateCountDisplay;
    }

    private void OnDisable()
    {
        GameManager.OnPowerupCountChanged -= UpdateCountDisplay;
    }

    private void Start()
    {
        _originalScale = transform.localScale;

        // Update số lượng ban đầu
        int currentCount = GameManager.Instance.GetPowerupCount(powerupName);
        UpdateCountDisplay(powerupName, currentCount);

        // Gán sự kiện click
        mainButton.onClick.AddListener(OnMainButtonClicked);
        
        if (cancelButton == null)
        {
            Debug.LogError($"LỖI: Bạn chưa kéo nút X (Cancel Button) vào script của {gameObject.name}!");
        }
        else 
        {
            // Ẩn nút X đi khi bắt đầu
            cancelButton.gameObject.SetActive(false);
            cancelButton.onClick.AddListener(OnCancelClicked);
        }
    }

    private void OnMainButtonClicked()
    {
        // Nếu đang chọn rồi thì không làm gì hoặc có thể coi là hủy (tuỳ logic, ở đây ta giữ nguyên)
        if (_isSelected) return;

        // Gọi Manager kiểm tra xem có chọn được không
        bool success = PowerupManager.Instance.SelectPowerup(powerupName, this);
        
        if (success)
        {
            SetSelectedState(true);
        }
    }

    private void OnCancelClicked()
    {
        // Gọi Manager hủy bỏ việc nhắm
        PowerupManager.Instance.CancelAiming();
    }

    // Hàm này sẽ được gọi bởi Manager khi hoàn thành nổ HOẶC khi bấm nút X
    public void ResetState()
    {
        SetSelectedState(false);
    }

    private void SetSelectedState(bool selected)
    {
        _isSelected = selected;

        if (selected)
        {
            // Phóng to nút
            transform.localScale = _originalScale * scaleMultiplier;
            // Hiện nút X
            if (cancelButton != null) cancelButton.gameObject.SetActive(true);
        }
        else
        {
            // Trả về kích thước gốc
            transform.localScale = _originalScale;
            // Ẩn nút X
            if (cancelButton != null) cancelButton.gameObject.SetActive(false);
        }
    }

    private void UpdateCountDisplay(string name, int newCount)
    {
        if (name == powerupName)
        {
            if (countText != null) countText.text = "x" + newCount;
            mainButton.interactable = (newCount > 0);
        }
    }
}