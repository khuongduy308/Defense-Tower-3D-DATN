using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialPopup : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image contentImage; // Cái ảnh hiển thị hướng dẫn
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button prevBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI pageText; // (Tùy chọn) Hiển thị trang 1/3

    [Header("Data")]
    [SerializeField] private Sprite[] tutorialPages; // Kéo 3 ảnh hướng dẫn vào đây

    private int _currentPageIndex = 0;
    private System.Action _onCloseCallback; // Hành động làm sau khi tắt (ví dụ: thả quái)

    private void Start()
    {
        // Gán sự kiện cho nút
        nextBtn.onClick.AddListener(NextPage);
        prevBtn.onClick.AddListener(PrevPage);
        closeBtn.onClick.AddListener(CloseTutorial);
        
        // Mặc định ẩn panel khi game bắt đầu
        gameObject.SetActive(false);
    }

    // Hàm gọi mở Tutorial
    public void ShowTutorial(System.Action onClosed = null)
    {
        _onCloseCallback = onClosed;
        _currentPageIndex = 0;
        
        gameObject.SetActive(true);
        UpdateUI();

        // PAUSE GAME khi đang xem hướng dẫn
        Time.timeScale = 0f; 
    }

    public void CloseTutorial()
    {
        gameObject.SetActive(false);
        
        // RESUME GAME
        Time.timeScale = 1f;

        // Gọi callback (ví dụ: Bắt đầu spawn quái)
        _onCloseCallback?.Invoke();
        _onCloseCallback = null; // Reset callback để lần sau mở bằng nút Hint không bị gọi lại
    }

    private void NextPage()
    {
        if (_currentPageIndex < tutorialPages.Length - 1)
        {
            _currentPageIndex++;
            UpdateUI();
        }
    }

    private void PrevPage()
    {
        if (_currentPageIndex > 0)
        {
            _currentPageIndex--;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        // Cập nhật ảnh
        if (tutorialPages.Length > 0)
            contentImage.sprite = tutorialPages[_currentPageIndex];

        // Ẩn/Hiện nút Next/Prev tùy trang
        prevBtn.gameObject.SetActive(_currentPageIndex > 0);
        nextBtn.gameObject.SetActive(_currentPageIndex < tutorialPages.Length - 1);

        // Cập nhật text số trang (nếu có)
        if (pageText != null) 
            pageText.text = $"{_currentPageIndex + 1} / {tutorialPages.Length}";
    }
}