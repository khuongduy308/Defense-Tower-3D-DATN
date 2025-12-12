using UnityEngine.SceneManagement;

// Script 1: Cầu nối tĩnh (Gọi từ MainMenu hoặc Game)
public static class Loader 
{
    // Biến lưu tên Scene muốn đến
    private static string targetSceneName;

    public static void Load(string sceneName)
    {
        targetSceneName = sceneName;
        // Chuyển ngay sang màn hình chờ
        SceneManager.LoadScene("LoadingScene");
    }

    // Hàm để LoadingCallback lấy tên Scene cần load
    public static string GetTargetScene()
    {
        return targetSceneName;
    }
}