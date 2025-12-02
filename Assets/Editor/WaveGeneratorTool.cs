using UnityEngine;
using UnityEditor; // Thêm thư viện này
using System.Collections.Generic;

public class WaveGeneratorTool
{
    [MenuItem("My Tools/Generate Level 1 Waves")]
    private static void GenerateLevel1Waves()
    {
        // Nơi bạn muốn lưu các file WaveData
        string path = "Assets/Editor/GameData/"; 

        for (int i = 0; i < 20; i++) // Tạo 20 wave
        {
            // 1. Tạo 1 WaveData instance
            WaveData wave = ScriptableObject.CreateInstance<WaveData>();

            List<EnemyGroup> groups = new List<EnemyGroup>();

            // 2. Thiết lập thông số theo CÔNG THỨC của bạn
            EnemyGroup group1 = new EnemyGroup();
            group1.enemyType = EnemyType.EnemyBase;
            group1.count = 10 + (i * 2); // Wave 0 có 5, wave 1 có 7,...
            group1.spawnInterval = 0.5f;

            groups.Add(group1);

            // Nếu là wave thứ 5 (i == 4), thêm 1 con Bomb
            if ((i + 1) % 5 == 0)
            {
                EnemyGroup group2 = new EnemyGroup();
                group2.enemyType = EnemyType.EnemyBomb;
                group2.count = 5 + (i / 5); // Cứ 5 wave thêm 1 bomb
                group2.spawnInterval = 1f;
                groups.Add(group2);
                wave.timeBetweenGroups = 2f;
            }

            if ((i + 1) % 2 == 0)
            {
                EnemyGroup group3 = new EnemyGroup();
                group3.enemyType = EnemyType.EnemyHeal;
                group3.count = 4 + (i / 5); // Cứ 5 wave thêm 1 bomb
                group3.spawnInterval = 1f;
                groups.Add(group3);
                wave.timeBetweenGroups = 2f;
            }

            if ((i + 1) % 4 == 0)
            {
                EnemyGroup group4 = new EnemyGroup();
                group4.enemyType = EnemyType.EnemyWolf;
                group4.count = 10 + (i / 5); // Cứ 5 wave thêm 1 bomb
                group4.spawnInterval = 1f;
                groups.Add(group4);
                wave.timeBetweenGroups = 2f;
            }

            wave.groupsInWave = groups.ToArray();
            // 3. Lưu file lại
            // Tên file ví dụ: "Assets/GameData/Waves/Level1/Wave_01.asset"
            string fileName = string.Format("Wave_{0:00}.asset", i + 1);
            AssetDatabase.CreateAsset(wave, path + fileName);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("ĐÃ TẠO XONG 20 WAVES!");
    }
}