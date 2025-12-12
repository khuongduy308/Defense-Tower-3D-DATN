using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [Header("UI References")]
    // public GameObject authPanel;
    // public TMP_InputField usernameInput;
    // public TMP_InputField passwordInput;
    // public TMP_Text messageText;
    
    [Header("Server Config")]
    public string baseUrl = "http://localhost:3000/api"; 

    // --- LƯU TRẠNG THÁI NGƯỜI CHƠI ---
    public bool IsLoggedIn { get; private set; } = false;
    public string CurrentUserId { get; private set; }
    public int MaxLevelReached { get; private set; } = 0;
    public string LastMessage { get; private set; }
    public static event System.Action<bool, string> OnAuthActionFinished;

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

    public void RequestRegister(string user, string pass) => StartCoroutine(AuthRequest("/register", user, pass));
    public void RequestLogin(string user, string pass) => StartCoroutine(AuthRequest("/login", user, pass));

    IEnumerator AuthRequest(string endpoint, string user, string pass)
    {
        AuthData data = new AuthData { username = user, password = pass };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest req = CreateRequest(baseUrl + endpoint, json))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                if (endpoint == "/login")
                {
                    LoginResponse res = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);
                    IsLoggedIn = true;
                    CurrentUserId = res.userId;
                    MaxLevelReached = res.maxLevel;
                    
                    // Logic cập nhật PlayerPrefs
                    if (res.maxLevel > PlayerPrefs.GetInt("MaxLevelReached", 0))
                    {
                        PlayerPrefs.SetInt("MaxLevelReached", res.maxLevel);
                        PlayerPrefs.Save();
                    }

                    // Bắn tin hiệu thành công về cho IntroController
                    OnAuthActionFinished?.Invoke(true, $"Welcome {res.username}!");
                    
                    yield return new WaitForSeconds(1f);
                    Loader.Load("MainMenu");
                }
                else
                {
                    // Đăng ký thành công
                    OnAuthActionFinished?.Invoke(false, "Done Register! Please Login.");
                }
            }
            else
            {
                // Thất bại (Lỗi mạng hoặc sai pass)
                string errorMsg = "Lỗi: " + req.downloadHandler.text;
                OnAuthActionFinished?.Invoke(false, errorMsg);
            }
        }
    }

    // ... (Các hàm SaveProgress, Logout, CreateRequest giữ nguyên) ...
    // Nhớ copy lại các hàm đó vào đây nhé
     public void SaveProgress(int levelIndex)
    {
        if (IsLoggedIn && levelIndex > MaxLevelReached)
        {
            MaxLevelReached = levelIndex; // Cập nhật RAM
            StartCoroutine(SaveProgressRoutine(levelIndex)); // Gửi lên Server
        }
    }

    IEnumerator SaveProgressRoutine(int levelIndex)
    {
        SaveData data = new SaveData { userId = CurrentUserId, levelIndex = levelIndex };
        string json = JsonUtility.ToJson(data);
        using (UnityWebRequest req = CreateRequest(baseUrl + "/save-progress", json))
        {
            yield return req.SendWebRequest();
        }
    }

    public void Logout()
    {
        IsLoggedIn = false;
        CurrentUserId = "";
        MaxLevelReached = 0;
        PlayerPrefs.DeleteKey("MaxLevelReached");
        PlayerPrefs.Save();
        SceneManager.LoadScene("IntroScene");
    }

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

[System.Serializable]
public class AuthData 
{ 
    public string username; 
    public string password; 
}

[System.Serializable]
public class LoginResponse 
{ 
    public string message; 
    public string userId; 
    public string username; 
    public int maxLevel; 
}

[System.Serializable]
public class SaveData 
{ 
    public string userId; 
    public int levelIndex; 
}