using UnityEngine;
using UnityEditor; // Thêm thư viện này

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

            // 2. Thiết lập thông số theo CÔNG THỨC của bạn
            EnemyGroup group1 = new EnemyGroup();
            group1.enemyType = EnemyType.EnemyBase;
            group1.count = 5 + (i * 2); // Wave 0 có 5, wave 1 có 7,...
            group1.spawnInterval = 0.5f;

            wave.groupsInWave = new EnemyGroup[] { group1 };

            // Nếu là wave thứ 5 (i == 4), thêm 1 con Bomb
            if ((i + 1) % 5 == 0)
            {
                EnemyGroup group2 = new EnemyGroup();
                group2.enemyType = EnemyType.EnemyBomb;
                group2.count = 1 + (i / 5); // Cứ 5 wave thêm 1 bomb
                group2.spawnInterval = 1f;
                wave.groupsInWave = new EnemyGroup[] { group1, group2 };
                wave.timeBetweenGroups = 2f;
            }

            // 3. Lưu file lại
            // Tên file ví dụ: "Assets/GameData/Waves/Level1/Wave_01.asset"
            string fileName = string.Format("Wave_{0:00}.asset", i + 1);
            AssetDatabase.CreateAsset(wave, path + fileName);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("ĐÃ TẠO XONG 20 WAVES!");
    }
}