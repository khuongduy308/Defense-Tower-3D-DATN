using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject welcomePanel; // Màn hình "Tap to Start"
    public GameObject authPanel;    // Màn hình Đăng nhập

    private void Start()
    {
        welcomePanel.SetActive(true);
        authPanel.SetActive(false);
        
        // Kiểm tra nếu đã từng đăng nhập (lưu token) thì vào luôn MainMenu (Nâng cao)
        //...
    }

    public void OnScreenTapped()
    {
        // welcomePanel.SetActive(false);
        authPanel.SetActive(true);
    }
}