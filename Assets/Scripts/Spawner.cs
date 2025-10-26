using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static Spawner Instance { get; private set; }

    public static event Action<int> OnWaveChanged;
    public static event Action OnMissionComplete;

    [SerializeField] private WaveData[] waves;
    private int _currentWaveIndex = 0;
    private int _waveCounter = 0;
    private WaveData CurrentWave => waves[_currentWaveIndex];
    private float _spawnTimer;
    private float _spawnCounter = 0;
    private int _enemiesRemoved = 0;

    private float _timeBetweenWaves = 2f;
    private float _waveCooldown = 0f;
    private bool _isBetweenWaves = false;
    private bool _isEndlessMode = false;

    [SerializeField] private List<Path> allPaths;

    [SerializeField] private ObjectPooler basePool;
    [SerializeField] private ObjectPooler bombPool;
    [SerializeField] private ObjectPooler healPool;

    private Dictionary<EnemyType, ObjectPooler> _poolDictionary;

    private void Awake()
    {
        _poolDictionary = new Dictionary<EnemyType, ObjectPooler>
        {
            { EnemyType.EnemyBase, basePool },
            { EnemyType.EnemyBomb, bombPool },
            { EnemyType.EnemyHeal, healPool }
        };

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    void Start()
    {
        OnWaveChanged?.Invoke(_waveCounter);
        // _spawnTimer = CurrentWave.spawnInterval;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isBetweenWaves)
        {
            _waveCooldown -= Time.deltaTime;
            if (_waveCooldown <= 0f)
            {
                if (_waveCounter + 1 >= LevelManager.Instance.CurrentLevel.wavesToWin && !_isEndlessMode)
                {
                    OnMissionComplete?.Invoke();
                    return;
                }

                _currentWaveIndex = (_currentWaveIndex + 1) % waves.Length;
                _waveCounter++;
                OnWaveChanged?.Invoke(_waveCounter);
                _spawnCounter = 0;
                _enemiesRemoved = 0;
                _spawnTimer = 0f;
                _isBetweenWaves = false;
            }
            return;
        } else {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f && _spawnCounter < CurrentWave.enemiesPerWave)
            {
                _spawnTimer = CurrentWave.spawnInterval;
                _spawnCounter++;
                SpawnEnemy();
            }
            else if (_spawnCounter >= CurrentWave.enemiesPerWave && _enemiesRemoved >= CurrentWave.enemiesPerWave)
            {

                _isBetweenWaves = true;
                _waveCooldown = _timeBetweenWaves;
            }
        }
        
    }

    private void SpawnEnemy()
    {
        if (_poolDictionary.TryGetValue(CurrentWave.enemyType, out var pool))
        {
            // Kiểm tra xem có path nào không
            if (allPaths == null || allPaths.Count == 0)
            {
                Debug.LogError("Spawner không có Path nào được gán trong Inspector!");
                return;
            }

            Path chosenPath = allPaths[UnityEngine.Random.Range(0, allPaths.Count)];

            GameObject spawnedObject = pool.GetPooledObject();
            spawnedObject.transform.position = transform.position;

            float healthMultiplier = 1f + (_waveCounter * 0.1f); //+10% health per wave
            Enemy enemy = spawnedObject.GetComponent<Enemy>();

            enemy.Initialize(chosenPath, healthMultiplier);

            spawnedObject.SetActive(true);
        }

    }

    private void HandleEnemyReachedEnd(EnemyData data)
    {
        _enemiesRemoved++;
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        _enemiesRemoved++;
    }
    
    public void EnableEndlessMode()
    {
        _isEndlessMode = true;
    }
}
