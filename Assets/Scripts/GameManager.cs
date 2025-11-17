using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private Dictionary<string, int> _powerupInventory = new Dictionary<string, int>();
    public static event Action<string, int> OnPowerupCountChanged;
    public static event Action<int> OnLivesChanged;
    public static event Action<int> OnResourcesChanged;
    private int _lives = 10;
    private int _resources = 175;
    public int Resources => _resources;
    private float _gameSpeed = 0.5f;
    public float GameSpeed => _gameSpeed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }else{
            Instance = this;
        }
    }

    private void OnEnable()
    {
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        OnLivesChanged?.Invoke(_lives);
        OnResourcesChanged?.Invoke(_resources);
    }

    public void HandleEnemyReachedEnd(EnemyData data)
    {
        _lives = Mathf.Max(0, _lives - data.damage);
        Debug.Log($"Lives: {_lives}");
        OnLivesChanged?.Invoke(_lives);
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        AddResources(Mathf.RoundToInt(enemy.Data.resourceReward));
    }

    public void AddResources(int amount)
    {
        // _resources += Mathf.RoundToInt(amount);
        _resources += amount;
        OnResourcesChanged?.Invoke(_resources);
        Debug.Log($"Resources: {_resources}");
    }

    public void setTimeScale(float scale) //danh cho pause game
    {
        Time.timeScale = scale;
    }

    public void SpendResources(int amount)
    {
        if (_resources >= amount)
        {
            _resources -= amount;
            OnResourcesChanged?.Invoke(_resources);
        }
    }

    public void SetGameSpeed(float newSpeed) //danh cho toc do game
    {
        _gameSpeed = newSpeed;
        setTimeScale(_gameSpeed);
    }

    public void ResetGameState()
    {
        _lives = LevelManager.Instance.CurrentLevel.startingLives;
        OnLivesChanged?.Invoke(_lives);
        _resources = LevelManager.Instance.CurrentLevel.startingResources;
        OnResourcesChanged?.Invoke(_resources);

        _powerupInventory.Clear();
        // OnPowerupCountChanged?.Invoke("Bomb", 0);

        SetGameSpeed(0.5f);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetGameState();
    }
    
    public void AddLives(int amount)
    {
        _lives += amount;
        OnLivesChanged?.Invoke(_lives);
        Debug.Log($"Lives added. Total Lives: {_lives}");
    }

    public void AddPowerup(string powerupName, int amount) //them vat pham vao tui do
    {
        if (!_powerupInventory.ContainsKey(powerupName))
        {
            _powerupInventory[powerupName] = 0;
        }
        _powerupInventory[powerupName] += amount;

        // Báo cho UI (như nút Bom) cập nhật
        OnPowerupCountChanged?.Invoke(powerupName, _powerupInventory[powerupName]);
    }

    public bool UsePowerup(string powerupName)  //co kha nang su dung vat pham khong
    {
        if (_powerupInventory.ContainsKey(powerupName) && _powerupInventory[powerupName] > 0)
        {
            _powerupInventory[powerupName]--;
            OnPowerupCountChanged?.Invoke(powerupName, _powerupInventory[powerupName]);
            return true;
        }
        return false; // Hết vật phẩm
    }

    public int GetPowerupCount(string powerupName) //lay so luong vat pham trong tui do
    {
        if (_powerupInventory.ContainsKey(powerupName))
        {
            return _powerupInventory[powerupName];
        }
        return 0;
    }
}

