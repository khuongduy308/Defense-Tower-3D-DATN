using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("General Info")]
    public string levelName; // Tên hiển thị (VD: "Vùng Đất Chết"), không cần trùng tên Scene nữa
    public int levelIndex;

    [Header("Resources")]
    public int startingResources;
    public int startingLives;

    [Header("Map Setup")]
    public GameObject mapPrefab; 
    
    [Header("Waves")]
    public WaveData[] wavesInThisLevel; // Mảng chứa các Wave
    
    // Tối ưu: Không cần nhập tay, tự động đếm số phần tử trong mảng
    public int WavesToWin => wavesInThisLevel.Length; 

    [Header("Optimization")]
    // List này dùng để Pre-load pool object, tránh giật lag khi game đang chạy
    public List<EnemyType> enemyTypesAllowed;
}