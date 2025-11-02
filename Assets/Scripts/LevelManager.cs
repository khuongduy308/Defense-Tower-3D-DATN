using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public LevelData[] allLevels;
    public LevelData CurrentLevel { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        CurrentLevel = allLevels[0];
    }

    public void LoadLevel(LevelData levelData)
    {
        CurrentLevel = levelData;
        SceneManager.LoadScene(levelData.levelName);
    }

    public void LoadNextLevel()
    {
        // 1. Tìm index của level hiện tại
        int currentIndex = -1;
        for (int i = 0; i < allLevels.Length; i++)
        {
            if (allLevels[i] == CurrentLevel)
            {
                currentIndex = i;
                break;
            }
        }

        // 2. Kiểm tra xem có level tiếp theo không
        if (currentIndex != -1 && currentIndex + 1 < allLevels.Length)
        {
            // Có level tiếp theo! Tải nó.
            LevelData nextLevel = allLevels[currentIndex + 1];
            LoadLevel(nextLevel);
        }
        else
        {
            // Đây là level cuối cùng, hoặc có lỗi
            Debug.Log("Bạn đã hoàn thành level cuối cùng! Quay về Main Menu.");
            // Giả sử bạn có Scene tên là "MainMenu"
            SceneManager.LoadScene("MainMenu");
        }
    }

}
