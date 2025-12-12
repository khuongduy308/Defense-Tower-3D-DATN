using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking; // MỚI: Dùng để gọi Server
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Cloud Config (MỚI)")]
    public string serverUrl = "http://localhost:3000/api/level/"; // Link server
    public GameResourceConfig resourceConfig; // Kéo file config Resources vào đây

    [Header("Offline Data")]
    public LevelData[] allLevels; // Dữ liệu offline dự phòng
    
    // Runtime State
    public LevelData CurrentLevel { get; private set; }
    public int CurrentLevelIndex { get; private set; }

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private string mapContainerName = "Environment";
    private Transform _mapParent; 
    private GameObject _currentMapInstance;

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
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameSceneName)
        {
            GameObject envObj = GameObject.Find(mapContainerName);
            if (envObj != null) _mapParent = envObj.transform;
            else _mapParent = new GameObject(mapContainerName).transform;

            // Khi Scene load xong, tiếp tục cài đặt với LevelData đang có
            if (CurrentLevel != null)
            {
                StartCoroutine(SetupLevelRoutine(CurrentLevel));
            }
        }
    }

    // --- PHẦN 1: LOAD LEVEL (ENTRY POINTS) ---

    // Cách 1: Load từ Cloud (Khuyên dùng)
    public void LoadLevelFromCloud(int index)
    {
        CurrentLevelIndex = index;
        
        // Nếu chưa ở trong Game Scene -> Load Scene trước
        if (SceneManager.GetActiveScene().name != gameSceneName)
        {
            Loader.Load(gameSceneName);
            StartCoroutine(DownloadAndPlayLevel(index, true)); // true = đợi scene load
        }
        else
        {
            // Đang ở trong game rồi -> Tải và chơi luôn
            StartCoroutine(DownloadAndPlayLevel(index, false));
        }
    }

    // Cách 2: Load Offline (Dự phòng)
    public void LoadLevelOffline(int index)
    {
        if (index < 0 || index >= allLevels.Length) return;
        CurrentLevelIndex = index;
        CurrentLevel = allLevels[index]; // Lấy từ mảng có sẵn

        if (SceneManager.GetActiveScene().name != gameSceneName)
        {
            SceneManager.LoadScene(gameSceneName);
            // OnSceneLoaded sẽ lo phần còn lại vì CurrentLevel đã có dữ liệu
        }
        else
        {
            StartCoroutine(SetupLevelRoutine(CurrentLevel));
        }
    }

    // --- PHẦN 2: TẢI VÀ XỬ LÝ DATA CLOUD ---

    private IEnumerator DownloadAndPlayLevel(int index, bool waitForScene)
    {
        if (waitForScene) 
        {
            // Vòng lặp: Chừng nào chưa sang Scene "Game" thì đứng đợi ở đây
            while (SceneManager.GetActiveScene().name != gameSceneName)
            {
                yield return null; // Đợi sang frame tiếp theo kiểm tra lại
            }

            // Đã sang Scene Game rồi, nhưng đợi thêm 1 frame để Spawner kịp chạy hàm Awake()
            yield return null; 
        }

        Debug.Log($"Đang tải Level {index} từ: {serverUrl + index}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(serverUrl + index))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Lỗi mạng: " + request.error + " -> Chuyển sang load Offline.");
                LoadLevelOffline(index); // Fallback về offline nếu mất mạng
            }
            else
            {
                string json = request.downloadHandler.text;
                Debug.Log("Đã nhận JSON: " + json);
                
                // Parse JSON sang DTO
                LevelDataDTO dto = JsonUtility.FromJson<LevelDataDTO>(json);
                
                // Convert DTO sang LevelData xịn
                SetupGameFromCloudData(dto);
            }
        }
    }

    private void SetupGameFromCloudData(LevelDataDTO dto)
    {
        // Tạo LevelData ảo
        LevelData runtimeLevelData = ScriptableObject.CreateInstance<LevelData>();
        runtimeLevelData.levelIndex = dto.levelIndex;
        runtimeLevelData.levelName = dto.levelName;
        runtimeLevelData.startingResources = dto.startingResources;
        runtimeLevelData.startingLives = dto.startingLives;

        // Map String -> Prefab
        runtimeLevelData.mapPrefab = resourceConfig.GetMap(dto.mapId);
        if (runtimeLevelData.mapPrefab == null) Debug.LogError($"Thiếu Map Prefab: {dto.mapId}");

        // Map String -> Enum cho Waves
        List<WaveData> realWaves = new List<WaveData>();
        foreach (var waveDto in dto.wavesInThisLevel)
        {
            WaveData w = ScriptableObject.CreateInstance<WaveData>();
            w.timeBetweenGroups = waveDto.timeBetweenGroups;

            List<EnemyGroup> realGroups = new List<EnemyGroup>();
            foreach (var groupDto in waveDto.groupsInWave)
            {
                EnemyGroup g = new EnemyGroup();
                g.count = groupDto.count;
                g.spawnInterval = groupDto.spawnInterval;
                
                // Chuyển đổi Enum an toàn
                if (Enum.TryParse(groupDto.enemyType, out EnemyType parsedType))
                    g.enemyType = parsedType;
                else
                    g.enemyType = EnemyType.EnemyBase; // Default

                realGroups.Add(g);
            }
            w.groupsInWave = realGroups.ToArray();
            realWaves.Add(w);
        }
        runtimeLevelData.wavesInThisLevel = realWaves.ToArray();

        // Gán vào biến chính và chạy game
        CurrentLevel = runtimeLevelData;
        StartCoroutine(SetupLevelRoutine(CurrentLevel));
    }

    // --- PHẦN 3: SETUP GAME LOGIC ---

    // Nhận LevelData trực tiếp thay vì Index
    private IEnumerator SetupLevelRoutine(LevelData data)
    {
        yield return null; 
        
        // 1. Chờ UI Controller (Fix lỗi Null)
        float waitTime = 0f;
        while (UIController.Instance == null && waitTime < 1f)
        {
            yield return null;
            waitTime += Time.deltaTime;
        }

        if (UIController.Instance != null) UIController.Instance.ResetGameUI();

        // 2. Reset Map
        if (_currentMapInstance != null) Destroy(_currentMapInstance);
        if (data.mapPrefab != null)
        {
            _currentMapInstance = Instantiate(data.mapPrefab, _mapParent);
        }

        // 3. Reset Stats
        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitLevelStats(data.startingResources, data.startingLives);
        }

        // 4. Setup Spawner
        List<Path> pathsInNewMap = new List<Path>();
        if (_currentMapInstance != null)
            pathsInNewMap.AddRange(_currentMapInstance.GetComponentsInChildren<Path>());

        if (Spawner.Instance != null)
        {
            Spawner.Instance.SetupLevel(data.wavesInThisLevel, pathsInNewMap);
        }
        else
        {
            Debug.LogError("Không tìm thấy Spawner instance!");
        }
        
        Time.timeScale = 1f;
        Debug.Log($"Đã Setup xong Level: {data.levelName} (Index: {data.levelIndex})");
    }

    public void LoadNextLevel()
    {
        int nextIndex = CurrentLevelIndex + 1;
        LoadLevelFromCloud(nextIndex); 
    }

    public void ReloadCurrentLevel()
    {
        // Load lại đúng level hiện tại
        LoadLevelFromCloud(CurrentLevelIndex);
    }

    public void SaveProgress()
    {
        int currentLevel = CurrentLevelIndex;

        // 1. Lưu Offline (PlayerPrefs) - Vẫn giữ để backup
        int maxLevelLocal = PlayerPrefs.GetInt("MaxLevelReached", 0);
        if (currentLevel >= maxLevelLocal)
        {
            PlayerPrefs.SetInt("MaxLevelReached", currentLevel + 1);
            PlayerPrefs.Save();
        }

        // 2. Lưu Online (Gọi AuthManager) - Lưu level tiếp theo đã mở khóa
        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            // Lưu ý: currentLevel là level vừa thắng -> mở khóa currentLevel + 1
            AuthManager.Instance.SaveProgress(currentLevel + 1);
        }
    }
}

// --- DTO CLASSES ---

[Serializable]
public class LevelDataDTO
{
    public int levelIndex;
    public string levelName;
    public int startingResources;
    public int startingLives;
    public string mapId;
    public List<WaveDTO> wavesInThisLevel;
}

[Serializable]
public class WaveDTO
{
    public float timeBetweenGroups;
    public List<EnemyGroupDTO> groupsInWave;
}

[Serializable]
public class EnemyGroupDTO
{
    public string enemyType;
    public int count;
    public float spawnInterval;
}