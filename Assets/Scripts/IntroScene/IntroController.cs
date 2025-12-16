using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject welcomePanel; // Màn hình "Tap to Start"
    public GameObject authPanel;    // Màn hình Đăng nhập

    [Header("Auth UI Inputs")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;         

    private void Start()
    {
        welcomePanel.SetActive(true);
        authPanel.SetActive(false);
        
        // Kiểm tra nếu đã từng đăng nhập (lưu token) thì vào luôn MainMenu (Nâng cao)
        //...
    }

    private void OnEnable()
    {
        // Lắng nghe sự kiện từ AuthManager trả về
        AuthManager.OnAuthActionFinished += UpdateMessage;
    }

    private void OnDisable()
    {
        AuthManager.OnAuthActionFinished -= UpdateMessage;
    }

    // Hàm nhận tin nhắn từ AuthManager để hiển thị lên màn hình
    private void UpdateMessage(bool isLoginSuccess, string msg)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = msg;
        messageText.color = isLoginSuccess ? Color.green : (msg.Contains("thành công") ? Color.green : Color.red);
    }

    public void OnTapToStart()
    {
        welcomePanel.SetActive(true);
        authPanel.SetActive(true);
        messageText.gameObject.SetActive(false);
    }

    // --- CÁC HÀM GẮN VÀO NÚT BẤM (BUTTON) ---

    public void OnClickLogin()
    {
        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            messageText.text = "Vui lòng nhập đủ thông tin!";
            messageText.color = Color.red;
            return;
        }
        
        messageText.text = "Đang xử lý...";
        messageText.color = Color.yellow;
        
        // Gọi sang Logic Manager
        AuthManager.Instance.RequestLogin(usernameInput.text, passwordInput.text);
    }

    public void OnClickRegister()
    {
        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            messageText.text = "Vui lòng nhập đủ thông tin!";
            return;
        }

        messageText.text = "Đang xử lý...";
        
        // Gọi sang Logic Manager
        AuthManager.Instance.RequestRegister(usernameInput.text, passwordInput.text);
    }

    public void OnClickGoogleLogin()
    {
        messageText.text = "Đang kết nối Google...";
        AuthManager.Instance.OnGoogleLoginBtnClicked();
    }
}