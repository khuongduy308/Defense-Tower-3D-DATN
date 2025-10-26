using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]

[System.Serializable]
public class EnemyGroup
{
    public EnemyType enemyType;     // Loại quái trong nhóm này
    public int count;               // Số lượng
    public float spawnInterval;     // Thời gian giãn cách *trong* nhóm này
}

[CreateAssetMenu(fileName = "NewWaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    // public EnemyType enemyType;
    // public float spawnInterval;
    // public int enemiesPerWave;

    public EnemyGroup[] groupsInWave;

    // Thêm thời gian chờ giữa các nhóm
    public float timeBetweenGroups;

    // Một thuộc tính (property) tiện ích để Spawner biết
    // tổng số quái trong wave này là bao nhiêu
    public int TotalEnemiesInWave
    {
        get
        {
            int total = 0;
            foreach (var group in groupsInWave)
            {
                total += group.count;
            }
            return total;
        }
    }
}