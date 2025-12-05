using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject authPanel;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;
    
    [Header("Server Config")]
    // Nhớ thay bằng link Render nếu đã deploy
    public string baseUrl = "http://localhost:3000/api"; 

    // --- LƯU TRẠNG THÁI NGƯỜI CHƠI ---
    public bool IsLoggedIn { get; private set; } = false;
    public string CurrentUserId { get; private set; }
    public int MaxLevelReached { get; private set; } = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnRegisterBtnClicked()
    {
        StartCoroutine(Register(usernameInput.text, passwordInput.text));
    }

    public void OnLoginBtnClicked()
    {
        StartCoroutine(Login(usernameInput.text, passwordInput.text));
    }

    IEnumerator Register(string user, string pass)
    {
        AuthData data = new AuthData { username = user, password = pass };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest req = CreateRequest(baseUrl + "/register", json))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                messageText.text = "Đăng ký thành công! Hãy đăng nhập.";
                messageText.color = Color.green;
            }
            else
            {
                messageText.text = "Lỗi: " + req.downloadHandler.text;
                messageText.color = Color.red;
            }
        }
    }

    IEnumerator Login(string user, string pass)
    {
        AuthData data = new AuthData { username = user, password = pass };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest req = CreateRequest(baseUrl + "/login", json))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                // Parse kết quả trả về
                LoginResponse res = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);
                
                // Lưu thông tin vào RAM
                IsLoggedIn = true;
                CurrentUserId = res.userId;
                MaxLevelReached = res.maxLevel;

                messageText.text = $"Đăng nhập thành công!";
                messageText.color = Color.green;

                // Ẩn bảng login đi
                yield return new WaitForSeconds(1f);

                GoToMainMenu();
                
                // Cập nhật lại các nút Level ở MainMenu (nếu đang ở đó)
                // MainMenuController.Instance.UpdateButtons(); 
            }
            else
            {
                messageText.text = "Lỗi: " + req.downloadHandler.text;
                messageText.color = Color.red;
            }
        }
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Logout()
    {
        // 1. Xóa dữ liệu trong RAM
        IsLoggedIn = false;
        CurrentUserId = "";
        MaxLevelReached = 0;

        // 2. Quay về màn hình Intro
        SceneManager.LoadScene("IntroScene");
    }

    // Hàm gọi khi thắng level để lưu
    public void SaveProgress(int levelIndex)
    {
        if (!IsLoggedIn) return; // Không đăng nhập thì không lưu server

        // Cập nhật RAM trước
        if (levelIndex > MaxLevelReached) MaxLevelReached = levelIndex;

        StartCoroutine(SaveProgressRoutine(levelIndex));
    }

    IEnumerator SaveProgressRoutine(int levelIndex)
    {
        SaveData data = new SaveData { userId = CurrentUserId, levelIndex = levelIndex };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest req = CreateRequest(baseUrl + "/save-progress", json))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Đã lưu tiến trình lên Server!");
            }
        }
    }

    // Helper tạo Request JSON
    UnityWebRequest CreateRequest(string url, string json)
    {
        var req = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        return req;
    }
}

// --- DTO CLASSES ---
[System.Serializable]
public class AuthData { public string username; public string password; }

[System.Serializable]
public class LoginResponse { public string message; public string userId; public string username; public int maxLevel; }

[System.Serializable]
public class SaveData { public string userId; public int levelIndex; }
