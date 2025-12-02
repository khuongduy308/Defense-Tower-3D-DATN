using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Level Buttons")]
    public Button[] levelButtons; 

    private void Start()
    {
        UpdateLevelButtons();
    }

    private void UpdateLevelButtons()
    {
        // Lấy level cao nhất từ PlayerPrefs (đã được AuthManager cập nhật khi login)
        int maxLevel = PlayerPrefs.GetInt("MaxLevelReached", 0);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (i <= maxLevel)
            {
                // Level đã mở
                levelButtons[i].interactable = true;
                // Có thể đổi màu icon để báo hiệu sáng lên
                levelButtons[i].image.color = Color.white; 
            }
            else
            {
                // Level chưa mở
                levelButtons[i].interactable = false;
                // Làm tối nút đi
                levelButtons[i].image.color = Color.gray; 
            }
        }
    }

    // Gắn vào sự kiện OnClick của từng nút Level: Nút 1 truyền 0, Nút 2 truyền 1...
    public void StartGame(int levelIndex)
    {
        // Gọi LevelManager (đang là Singleton DontDestroyOnLoad)
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevelFromCloud(levelIndex);
        }
        else
        {
            Debug.LogError("Chưa có LevelManager! Hãy chạy game từ Scene đầu tiên.");
        }
    }

    // Gắn vào nút Logout
    public void QuitGame() // Hoặc đặt tên là Logout
    {
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.Logout();
        }
        else
        {
            // Fallback nếu test trực tiếp
            SceneManager.LoadScene("IntroScene");
        }
    }
}
