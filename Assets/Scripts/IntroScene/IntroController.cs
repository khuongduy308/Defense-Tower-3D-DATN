using UnityEngine;
using TMPro;

public class IntroController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject welcomePanel; 
    public GameObject authPanel;    

    [Header("Inputs")]
    public TMP_InputField emailInput;
    public TMP_InputField passInput;
    public TMP_InputField nameInput;
    public TMP_Text messageText;         

    [Header("Buttons")]
    public GameObject loginBtn;      
    public GameObject registerBtn;   
    public GameObject toggleText;    

    private bool isLoginMode = true; 

    private void Start()
    {
        welcomePanel.SetActive(true);
        authPanel.SetActive(false);
        if(messageText) messageText.gameObject.SetActive(false);
    }

    private void OnEnable() => AuthManager.OnAuthActionFinished += UpdateMessage;
    private void OnDisable() => AuthManager.OnAuthActionFinished -= UpdateMessage;

    private void UpdateMessage(bool isSuccess, string msg)
    {
        if(messageText == null) return;
        messageText.gameObject.SetActive(true);
        messageText.text = msg;
        messageText.color = isSuccess ? Color.green : Color.red;
    }

    public void OnTapToStart()
    {
        welcomePanel.SetActive(false);
        authPanel.SetActive(true);
        SwitchMode(true); 
    }

    public void OnToggleModeClick()
    {
        isLoginMode = !isLoginMode;
        SwitchMode(isLoginMode);
    }

    void SwitchMode(bool isLogin)
    {
        isLoginMode = isLogin;
        nameInput.gameObject.SetActive(!isLogin); // Ẩn hiện ô nhập tên
        loginBtn.SetActive(isLogin);
        registerBtn.SetActive(!isLogin);
        
        TMP_Text txt = toggleText.GetComponentInChildren<TMP_Text>();
        if(txt) txt.text = isLogin ? "Chưa có tài khoản? <b>Đăng ký</b>" : "Đã có tài khoản? <b>Đăng nhập</b>";
        
        messageText.gameObject.SetActive(false);
    }

    // --- SỰ KIỆN NÚT BẤM ---

    public void OnRegisterBtnClick()
    {
        string name = nameInput.text;
        if(string.IsNullOrEmpty(name) && !isLoginMode) 
        {
            UpdateMessage(false, "Vui lòng nhập tên hiển thị!");
            return;
        }
        AuthManager.Instance.Register(emailInput.text, passInput.text, name);
    }

    public void OnLoginBtnClick()
    {
        AuthManager.Instance.Login(emailInput.text, passInput.text);
    }

    public void OnForgotPasswordClick()
    {
        AuthManager.Instance.ResetPassword(emailInput.text);
    }
}