using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Data")]
    public LevelData[] allLevels;
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameSceneName)
        {
            GameObject envObj = GameObject.Find(mapContainerName);
            if (envObj != null)
            {
                _mapParent = envObj.transform;
            }
            else
            {
                _mapParent = new GameObject(mapContainerName).transform;
            }

            StartCoroutine(SetupLevelRoutine(CurrentLevelIndex));
        }
    }

    public void LoadLevel(int index)
    {
        if (index < 0 || index >= allLevels.Length) return;

        CurrentLevelIndex = index;
        CurrentLevel = allLevels[index];

        if (SceneManager.GetActiveScene().name != gameSceneName)
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            StartCoroutine(SetupLevelRoutine(index));
        }
    }

    private IEnumerator SetupLevelRoutine(int index)
    {
        yield return null; 

        if (_currentMapInstance != null) Destroy(_currentMapInstance);

        if (UIController.Instance != null)
        {
            UIController.Instance.ResetGameUI(); // Tắt bảng Win đi
            Debug.Log("UIController: Reset UI khi load level mới.");
        }

        if (CurrentLevel.mapPrefab != null)
        {
            _currentMapInstance = Instantiate(CurrentLevel.mapPrefab, _mapParent);
        }

        List<Path> pathsInNewMap = new List<Path>();
        if (_currentMapInstance != null)
        {
            Path[] foundPaths = _currentMapInstance.GetComponentsInChildren<Path>();
            pathsInNewMap.AddRange(foundPaths);
        }

        if (Spawner.Instance != null)
        {
            Spawner.Instance.SetupLevel(CurrentLevel.wavesInThisLevel, pathsInNewMap);
        }
        else
        {
            Debug.LogError("Không tìm thấy Spawner instance!");
        }
        
        Time.timeScale = 1f;
    }

    public void LoadNextLevel()
    {
        int nextIndex = CurrentLevelIndex + 1;

        Debug.Log($"Check Next Level: Hiện tại={CurrentLevelIndex}, Tiếp theo={nextIndex}, Tổng số Level={allLevels.Length}");

        if (nextIndex < allLevels.Length)
        {
            LoadLevel(nextIndex);
        }
        else
        {
            Debug.Log("Về Main Menu");
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void ReloadCurrentLevel()
    {
        LoadLevel(CurrentLevelIndex);
    }
}