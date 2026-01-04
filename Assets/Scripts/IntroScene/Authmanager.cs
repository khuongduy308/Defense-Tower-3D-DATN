using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore; // Thư viện lưu trữ dữ liệu
using Firebase.Extensions;
using System.Collections.Generic; // Để dùng Dictionary
using UnityEngine.SceneManagement;
using System.Collections;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [Header("User Info")]
    public bool IsLoggedIn = false;
    public string CurrentUserId;
    public string CurrentUsername;
    public int MaxLevelReached = 1; // Mặc định level 1

    public static event System.Action<bool, string> OnAuthActionFinished;

    FirebaseAuth auth;
    FirebaseFirestore db; // Biến quản lý Database

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else Destroy(gameObject);
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance; // Khởi tạo Firestore

                // Kiểm tra đăng nhập tự động
                if (auth.CurrentUser != null)
                {
                    auth.CurrentUser.ReloadAsync().ContinueWithOnMainThread(reloadTask => {
                        if (auth.CurrentUser.IsEmailVerified)
                        {
                            Debug.Log("Auto Login thành công!");
                            OnLoginSuccess(auth.CurrentUser);
                        }
                    });
                }
            }
            else Debug.LogError("Lỗi Firebase: " + task.Result);
        });
    }

    // --- CÁC HÀM XỬ LÝ AUTH (Đăng ký/Đăng nhập) ---

    public void Register(string email, string password, string username)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                OnAuthActionFinished?.Invoke(false, "Lỗi ĐK: " + task.Exception?.InnerException?.Message);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            UserProfile profile = new UserProfile { DisplayName = username };
            
            newUser.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(updateTask => {
                newUser.SendEmailVerificationAsync();
                OnAuthActionFinished?.Invoke(true, "Đăng ký thành công! Hãy kiểm tra Email để kích hoạt.");
                auth.SignOut(); // Bắt đăng nhập lại
            });
        });
    }

    public void Login(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                OnAuthActionFinished?.Invoke(false, "Sai email hoặc mật khẩu!");
                return;
            }

            FirebaseUser user = task.Result.User;
            if (user.IsEmailVerified)
            {
                OnLoginSuccess(user);
            }
            else
            {
                OnAuthActionFinished?.Invoke(false, "Email chưa kích hoạt! Vui lòng kiểm tra hòm thư.");
                auth.SignOut();
            }
        });
    }

    // Hàm chung xử lý khi Login thành công (để đỡ viết lặp lại)
    void OnLoginSuccess(FirebaseUser user)
    {
        IsLoggedIn = true;
        CurrentUserId = user.UserId;
        CurrentUsername = user.DisplayName;
        
        // TẢI DATA VỀ NGAY LẬP TỨC
        LoadUserData(); 
    }

    public void ResetPassword(string email)
    {
        if (string.IsNullOrEmpty(email)) return;
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task => {
            OnAuthActionFinished?.Invoke(!task.IsFaulted, task.IsFaulted ? "Lỗi gửi mail" : "Đã gửi mail reset!");
        });
    }

    public void Logout()
    {
        if (auth != null) auth.SignOut();
        IsLoggedIn = false;
        MaxLevelReached = 1; // Reset data tạm
        SceneManager.LoadScene("IntroScene");
    }

    // --- CÁC HÀM LƯU TRỮ DỮ LIỆU (FIRESTORE) ---

    public void LoadUserData()
    {
        if (auth.CurrentUser == null) return;
        
        // Tìm file dữ liệu của user này
        DocumentReference docRef = db.Collection("users").Document(CurrentUserId);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted) return;

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists && snapshot.ContainsField("maxLevel"))
            {
                // Dữ liệu Firestore trả về dạng Long, cần ép kiểu về Int
                MaxLevelReached = System.Convert.ToInt32(snapshot.GetValue<long>("maxLevel"));
                Debug.Log($"Đã tải data: Level {MaxLevelReached}");
            }
            else
            {
                // User mới chưa có file save -> Tạo file mới level 1
                SaveLevelData(1);
            }

            // Báo cho UI biết là xong xuôi hết rồi -> Vào game thôi
            OnAuthActionFinished?.Invoke(true, $"Xin chào {CurrentUsername}!");
            StartCoroutine(LoadGameDelay());
        });
    }

    // Hàm này gọi khi thắng game
    public void SaveLevelData(int level)
    {
        if (auth.CurrentUser == null) return;

        // Chỉ lưu nếu kỷ lục mới cao hơn kỷ lục cũ
        if (level >= MaxLevelReached)
        {
            MaxLevelReached = level; // Cập nhật RAM

            // Đóng gói dữ liệu gửi lên mây
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "username", CurrentUsername },
                { "maxLevel", level },
                { "lastLogin", FieldValue.ServerTimestamp }
            };

            // Ghi đè (Merge) dữ liệu
            db.Collection("users").Document(CurrentUserId).SetAsync(data, SetOptions.MergeAll);
            Debug.Log("Đã lưu Level lên mây: " + level);
        }
    }

    IEnumerator LoadGameDelay()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("MainMenu"); 
    }
}