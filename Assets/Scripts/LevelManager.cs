using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Vẫn giữ nếu cần quay về MainMenu

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Data")]
    public LevelData[] allLevels;
    public LevelData CurrentLevel { get; private set; }
    
    public int CurrentLevelIndex { get; private set; }

    [Header("Scene References")]
    public Transform mapParent; // Kéo một object rỗng (VD: "Environment") vào đây để chứa Map
    private GameObject _currentMapInstance; // Biến lưu trữ map đang chạy thực tế

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // Nếu bạn muốn test ngay Level 1 khi ấn Play:
            // LoadLevel(allLevels[0]); 
        }
    }

    public void LoadLevel(int index)
    {
        if (index < 0 || index >= allLevels.Length) return;

        CurrentLevelIndex = index;
        CurrentLevel = allLevels[index];

        // 1. Xử lý Map (Single Scene)
        if (_currentMapInstance != null) Destroy(_currentMapInstance);
        if (CurrentLevel.mapPrefab != null)
        {
            _currentMapInstance = Instantiate(CurrentLevel.mapPrefab, mapParent);
        }

        // 2. Cài đặt tài nguyên cho Player (Ví dụ)
        // PlayerStats.Instance.SetMoney(CurrentLevel.startingResources);
        // PlayerStats.Instance.SetLives(CurrentLevel.startingLives);

        // 3. Truyền Wave Data sang Spawner
        if (Spawner.Instance != null)
        {
            // Truyền đúng mảng WaveData[] vào
            Spawner.Instance.SetupLevel(CurrentLevel.wavesInThisLevel);
        }
    }

    // Nút "Next Level" sẽ gọi hàm này
    public void LoadNextLevel()
    {
        int nextIndex = CurrentLevelIndex + 1;

        if (nextIndex < allLevels.Length)
        {
            LoadLevel(nextIndex);
        }
        else
        {
            Debug.Log("Đã phá đảo! Quay về Main Menu.");
            // Riêng về MainMenu thì nên LoadScene thật vì UI MainMenu thường khác hoàn toàn Game
            SceneManager.LoadScene("MainMenu"); 
            
            // Lưu ý: Khi về MainMenu, LevelManager này có thể vẫn tồn tại do DontDestroyOnLoad.
            // Bạn cần xử lý nó (Destroy) hoặc tái sử dụng cẩn thận.
        }
    }
    
    // Hàm tiện ích để load lại level hiện tại (Replay)
    public void ReloadCurrentLevel()
    {
        LoadLevel(CurrentLevelIndex);
    }
}