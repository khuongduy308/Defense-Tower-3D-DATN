using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static Spawner Instance { get; private set; }

    public static event Action<int> OnWaveChanged;
    public static event Action OnMissionComplete;
    private WaveData[] _wavesForThisLevel;
    private int _currentWaveIndex = 0;
    private int _waveCounter = 0;
    private WaveData CurrentWave => _wavesForThisLevel[_currentWaveIndex];
    private float _spawnTimer;
    private float _spawnCounter = 0;
    private int _enemiesRemoved = 0;

    private float _timeBetweenWaves = 2f;
    private float _waveCooldown = 0f;
    private bool _isBetweenWaves = false;
    private bool _isEndlessMode = false;

    private bool _isSpawningActive = false; //danh dau da co level de chay
    private int _currentGroupIndex = 0;
    private int _enemiesSpawnedInGroup = 0;

    [SerializeField] private List<Path> allPaths;

    [SerializeField] private ObjectPooler basePool;
    [SerializeField] private ObjectPooler bombPool;
    [SerializeField] private ObjectPooler healPool;
    [SerializeField] private ObjectPooler wolfPool;

    private Dictionary<EnemyType, ObjectPooler> _poolDictionary;

    private void Awake()
    {
        _poolDictionary = new Dictionary<EnemyType, ObjectPooler>
        {
            { EnemyType.EnemyBase, basePool },
            { EnemyType.EnemyBomb, bombPool },
            { EnemyType.EnemyHeal, healPool },
            { EnemyType.EnemyWolf, wolfPool }
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
        // Lấy danh sách wave từ LevelManager
        // _wavesForThisLevel = LevelManager.Instance.CurrentLevel.wavesInThisLevel;

        // OnWaveChanged?.Invoke(_waveCounter);
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
                if (_waveCounter + 1 >= LevelManager.Instance.CurrentLevel.WavesToWin && !_isEndlessMode)
                {
                    OnMissionComplete?.Invoke();
                    return;
                }

                _currentWaveIndex = (_currentWaveIndex + 1) % _wavesForThisLevel.Length;
                _waveCounter++;
                OnWaveChanged?.Invoke(_waveCounter);
                _spawnCounter = 0;
                _enemiesRemoved = 0;
                _spawnTimer = 0f;
                _isBetweenWaves = false;

                _currentGroupIndex = 0;
                _enemiesSpawnedInGroup = 0;
            }
            return;
        } else {
            // _spawnTimer -= Time.deltaTime;
            // if (_spawnTimer <= 0f && _spawnCounter < CurrentWave.enemiesPerWave)
            // {
            //     _spawnTimer = CurrentWave.spawnInterval;
            //     _spawnCounter++;
            //     SpawnEnemy();
            // }
            // else if (_spawnCounter >= CurrentWave.enemiesPerWave && _enemiesRemoved >= CurrentWave.enemiesPerWave)
            // {

            //     _isBetweenWaves = true;
            //     _waveCooldown = _timeBetweenWaves;
            // }

            _spawnTimer -= Time.deltaTime;

            // Lấy tổng số quái cần spawn (từ property mới của WaveData)
            int totalEnemiesToSpawn = CurrentWave.TotalEnemiesInWave;

            if (_spawnTimer <= 0f && _spawnCounter < totalEnemiesToSpawn)
            {
                // Lấy nhóm (group) hiện tại
                EnemyGroup currentGroup = CurrentWave.groupsInWave[_currentGroupIndex];
                
                // 1. Spawn quái theo đúng loại của group
                SpawnEnemy(currentGroup.enemyType); 
                _spawnCounter++; // Tăng tổng số quái đã spawn
                _enemiesSpawnedInGroup++; // Tăng số quái trong group
                
                // 2. Đặt hẹn giờ cho quái tiếp theo TRONG group
                _spawnTimer = currentGroup.spawnInterval;

                // 3. Kiểm tra xem group này đã spawn xong chưa
                if (_enemiesSpawnedInGroup >= currentGroup.count)
                {
                    _currentGroupIndex++; // Chuyển sang nhóm tiếp theo
                    _enemiesSpawnedInGroup = 0; // Reset bộ đếm group

                    // 4. Nếu vẫn còn nhóm tiếp theo, thêm thời gian chờ
                    if (_currentGroupIndex < CurrentWave.groupsInWave.Length)
                    {
                        _spawnTimer = CurrentWave.timeBetweenGroups;
                    }
                }
            }
            // Logic kết thúc wave (giữ nguyên, chỉ thay tên biến)
            else if (_spawnCounter >= totalEnemiesToSpawn && _enemiesRemoved >= totalEnemiesToSpawn)
            {
                _isBetweenWaves = true;
                _waveCooldown = _timeBetweenWaves;
            }
        }
        
    }

    public void SetupLevel(WaveData[] levelWaves)
{
    _wavesForThisLevel = levelWaves;

    // 1. Reset chỉ số Wave
    _currentWaveIndex = 0;
    
    // 2. Reset các chỉ số trong Wave
    _currentGroupIndex = 0;
    _enemiesSpawnedInGroup = 0;
    _spawnCounter = 0;
    _enemiesRemoved = 0;
    
    _isBetweenWaves = false;

    // 3. Kiểm tra an toàn và kích hoạt
    if (_wavesForThisLevel != null && _wavesForThisLevel.Length > 0)
    {
        _isSpawningActive = true;
        
        // Báo hiệu UI cập nhật Wave 1
        OnWaveChanged?.Invoke(1); 

        // Lấy thông tin Wave đầu tiên để setup timer ban đầu
        WaveData firstWave = _wavesForThisLevel[0];
        if (firstWave.groupsInWave != null && firstWave.groupsInWave.Length > 0)
        {
            // Timer khởi điểm bằng thời gian giãn cách của nhóm quái đầu tiên
            _spawnTimer = firstWave.groupsInWave[0].spawnInterval;
        }
        else
        {
            // Trường hợp Wave rỗng không có quái
            _spawnTimer = 1f; 
        }
    }
    else
    {
        Debug.LogError("Spawner: List Wave bị rỗng hoặc null!");
        _isSpawningActive = false;
    }
}
    private void SpawnEnemy(EnemyType typeToSpawn)
    {
        // if (_poolDictionary.TryGetValue(CurrentWave.enemyType, out var pool))
        // {
        //     // Kiểm tra xem có path nào không
        //     if (allPaths == null || allPaths.Count == 0)
        //     {
        //         Debug.LogError("Spawner không có Path nào được gán trong Inspector!");
        //         return;
        //     }

        //     Path chosenPath = allPaths[UnityEngine.Random.Range(0, allPaths.Count)];

        //     GameObject spawnedObject = pool.GetPooledObject();
        //     spawnedObject.transform.position = transform.position;

        //     float healthMultiplier = 1f + (_waveCounter * 0.1f); //+10% health per wave
        //     Enemy enemy = spawnedObject.GetComponent<Enemy>();

        //     enemy.Initialize(chosenPath, healthMultiplier);

        //     spawnedObject.SetActive(true);
        // }


        if (_poolDictionary.TryGetValue(typeToSpawn, out var pool))
        {
            // (Phần còn lại của hàm giữ nguyên y hệt)
            if (allPaths == null || allPaths.Count == 0)
            {
                Debug.LogError("Spawner không có Path nào được gán trong Inspector!");
                return;
            }

            Path chosenPath = allPaths[UnityEngine.Random.Range(0, allPaths.Count)];

            GameObject spawnedObject = pool.GetPooledObject();
            // Xóa dòng này: spawnedObject.transform.position = transform.position;
            // Vì hàm Initialize() trong Enemy.cs đã làm việc này rồi

            float healthMultiplier = 1f + (_waveCounter * 0.1f);
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
