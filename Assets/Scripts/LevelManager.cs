using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Firestore; // Thư viện Firestore
using Firebase.Extensions;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    #region CONFIGURATION
    [Header("Firebase Config")]
    [SerializeField] private string collectionName = "levels"; // Tên collection trên Firestore
    public GameResourceConfig resourceConfig; // Config để map string -> Prefab

    [Header("Offline Fallback")]
    public LevelData[] offlineLevels; 

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private string mapContainerName = "Environment";
    #endregion

    #region RUNTIME STATE
    public LevelData CurrentLevel { get; private set; }
    public int CurrentLevelIndex { get; private set; }
    
    private Transform _mapParent;
    private GameObject _currentMapInstance;
    private FirebaseFirestore db;
    #endregion

    #region UNITY LIFECYCLE
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Khởi tạo DB một lần duy nhất
        db = FirebaseFirestore.DefaultInstance;
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameSceneName)
        {
            InitializeMapContainer();
            // Nếu đã tải xong data level, tiến hành setup
            if (CurrentLevel != null)
            {
                StartCoroutine(SetupGameRoutine(CurrentLevel));
            }
        }
    }
    #endregion

    #region PUBLIC API (LOAD LEVEL)

    public void LoadLevel(int index)
    {
        CurrentLevelIndex = index;
        StartCoroutine(LoadLevelFromFirestore(index));
    }

    public void LoadNextLevel() => LoadLevel(CurrentLevelIndex + 1);
    public void ReloadCurrentLevel() => LoadLevel(CurrentLevelIndex);

    public void SaveProgress()
    {
        int nextLevel = CurrentLevelIndex + 1;

        // 1. Lưu Offline (Backup)
        if (CurrentLevelIndex >= PlayerPrefs.GetInt("MaxLevelReached", 0))
        {
            PlayerPrefs.SetInt("MaxLevelReached", nextLevel);
            PlayerPrefs.Save();
        }

        // 2. Lưu Online (Qua AuthManager)
        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            AuthManager.Instance.SaveLevelData(nextLevel);
        }
    }

    #endregion

    #region CORE LOGIC (FIRESTORE LOADING)

    private IEnumerator LoadLevelFromFirestore(int index)
    {
        // 1. Chuyển Scene và chờ
        yield return EnsureGameSceneActive();

        Debug.Log($"[Firestore] Đang tải Level {index}...");

        // 2. Gọi Firestore (Bất đồng bộ)
        var task = db.Collection(collectionName).Document(index.ToString()).GetSnapshotAsync();
        
        // Đợi task hoàn thành mà không chặn main thread
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
        {
            Debug.LogError($"Lỗi tải Level {index}: {task.Exception}");
            Debug.LogWarning("-> Chuyển sang load Offline.");
            LoadLevelOffline(index);
        }
        else
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Debug.Log("Đã tìm thấy Level trên mây!");
                // Parse dữ liệu từ Firestore Dictionary sang LevelData
                LevelData cloudData = ParseFirestoreData(snapshot, index);
                CurrentLevel = cloudData;
                StartCoroutine(SetupGameRoutine(CurrentLevel));
            }
            else
            {
                Debug.LogError($"Level {index} không tồn tại trên Firestore!");
                LoadLevelOffline(index);
            }
        }
    }

    // Hàm load offline dự phòng
    private void LoadLevelOffline(int index)
    {
        // Index mảng bắt đầu từ 0, nên cần trừ 1 nếu level bắt đầu từ 1
        int arrayIndex = index - 1; 

        if (arrayIndex >= 0 && arrayIndex < offlineLevels.Length)
        {
            Debug.Log($"Loading Offline Level {index}");
            CurrentLevel = offlineLevels[arrayIndex];
            StartCoroutine(SetupGameRoutine(CurrentLevel));
        }
        else
        {
            Debug.LogError($"Không tìm thấy Level {index} trong cả Online và Offline!");
        }
    }

    #endregion

    #region DATA PARSING (QUAN TRỌNG)

    // Chuyển đổi cục dữ liệu thô của Firestore thành ScriptableObject game hiểu được
    private LevelData ParseFirestoreData(DocumentSnapshot doc, int index)
    {
        LevelData data = ScriptableObject.CreateInstance<LevelData>();
        
        // 1. Thông tin cơ bản (Dùng TryGetValue cho an toàn)
        data.levelIndex = index;
        data.levelName = doc.GetValue<string>("levelName");
        data.startingLives = doc.ContainsField("startingLives") ? Convert.ToInt32(doc.GetValue<long>("startingLives")) : 20;
        data.startingResources = doc.ContainsField("startingResources") ? Convert.ToInt32(doc.GetValue<long>("startingResources")) : 500;
        
        // 2. Map Prefab
        string mapId = doc.GetValue<string>("mapId");
        data.mapPrefab = resourceConfig.GetMap(mapId);

        // 3. Waves (Phần phức tạp nhất: Xử lý mảng lồng nhau)
        List<WaveData> wavesList = new List<WaveData>();

        if (doc.ContainsField("waves"))
        {
            // Lấy list các object thô
            List<object> wavesRaw = doc.GetValue<List<object>>("waves");

            foreach (var waveObj in wavesRaw)
            {
                // Ép kiểu về Dictionary để đọc
                Dictionary<string, object> waveDict = (Dictionary<string, object>)waveObj;
                
                WaveData wave = ScriptableObject.CreateInstance<WaveData>();
                wave.timeBetweenGroups = Convert.ToSingle(waveDict["timeBetweenGroups"]);

                // Xử lý Groups trong Wave
                List<EnemyGroup> groupsList = new List<EnemyGroup>();
                List<object> groupsRaw = (List<object>)waveDict["groups"];

                foreach (var groupObj in groupsRaw)
                {
                    Dictionary<string, object> groupDict = (Dictionary<string, object>)groupObj;
                    
                    EnemyGroup group = new EnemyGroup();
                    group.count = Convert.ToInt32(groupDict["count"]);
                    group.spawnInterval = Convert.ToSingle(groupDict["spawnInterval"]);
                    
                    // Parse Enum EnemyType
                    string typeStr = groupDict["enemyType"].ToString();
                    if (Enum.TryParse(typeStr, out EnemyType type)) group.enemyType = type;
                    else group.enemyType = EnemyType.EnemyBase;

                    groupsList.Add(group);
                }
                wave.groupsInWave = groupsList.ToArray();
                wavesList.Add(wave);
            }
        }
        
        data.wavesInThisLevel = wavesList.ToArray();
        return data;
    }

    #endregion

    #region GAME SETUP (GIỮ NGUYÊN)

    private IEnumerator EnsureGameSceneActive()
    {
        if (SceneManager.GetActiveScene().name != gameSceneName)
        {
            SceneManager.LoadScene(gameSceneName);
            while (SceneManager.GetActiveScene().name != gameSceneName) yield return null;
            yield return null;
        }
    }

    private IEnumerator SetupGameRoutine(LevelData data)
    {
        // Timeout chờ UI
        float timeout = 2f;
        while (UIController.Instance == null && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (UIController.Instance != null) UIController.Instance.ResetGameUI();

        // Setup Map
        if (_currentMapInstance != null) Destroy(_currentMapInstance);
        InitializeMapContainer();
        if (data.mapPrefab != null) _currentMapInstance = Instantiate(data.mapPrefab, _mapParent);

        // Setup Stats
        if (GameManager.Instance != null)
            GameManager.Instance.InitLevelStats(data.startingResources, data.startingLives);

        // Setup Spawner
        SetupSpawner(data);

        Time.timeScale = 1f;
        Debug.Log($"<color=green>Setup Level {data.levelIndex} Success!</color>");
    }

    private void InitializeMapContainer()
    {
        if (_mapParent == null)
        {
            GameObject envObj = GameObject.Find(mapContainerName);
            if (envObj != null) _mapParent = envObj.transform;
            else _mapParent = new GameObject(mapContainerName).transform;
        }
    }

    private void SetupSpawner(LevelData data)
    {
        if (Spawner.Instance == null) return;
        List<Path> mapPaths = new List<Path>();
        if (_currentMapInstance != null) mapPaths.AddRange(_currentMapInstance.GetComponentsInChildren<Path>());
        Spawner.Instance.SetupLevel(data.wavesInThisLevel, mapPaths);
    }

    #endregion
}