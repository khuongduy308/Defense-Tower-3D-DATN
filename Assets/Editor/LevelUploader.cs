using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using Firebase.Firestore;
using Firebase.Extensions;

public class LevelUploader : MonoBehaviour { } // Giữ nguyên class rỗng để không lỗi file

public class LevelUploadMenu
{
    // Tên Collection trên Firestore
    static string collectionName = "levels";

    [MenuItem("Assets/TowerDefense/Upload Level To Firestore")]
    public static void UploadLevel()
    {
        // 1. Lấy file đang được chọn
        LevelData selectedLevel = Selection.activeObject as LevelData;

        if (selectedLevel == null)
        {
            Debug.LogError("Hãy chọn một file LevelData (ScriptableObject)!");
            return;
        }

        Debug.Log($"Đang chuẩn bị upload Level {selectedLevel.levelIndex}...");

        // 2. Chuyển đổi dữ liệu sang Dictionary (Format Firestore thích nhất)
        Dictionary<string, object> firestoreData = ConvertToFirestoreData(selectedLevel);

        // 3. Gửi lên Firestore
        UploadToFirestore(selectedLevel.levelIndex.ToString(), firestoreData);
    }

    static void UploadToFirestore(string docId, Dictionary<string, object> data)
    {
        // Lấy instance database
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        // Ghi dữ liệu vào Collection "levels", Document ID = "1", "2"...
        // SetAsync sẽ ghi đè nếu đã tồn tại, hoặc tạo mới nếu chưa có
        db.Collection(collectionName).Document(docId).SetAsync(data).ContinueWithOnMainThread(task => {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log($"<color=green>UPLOAD THÀNH CÔNG!</color> Level {docId} đã lên mây.");
            }
            else
            {
                Debug.LogError($"UPLOAD THẤT BẠI: {task.Exception}");
            }
        });
    }

    // Hàm chuyển đổi: Unity Object -> Dictionary<string, object>
    // Firestore lưu mảng dưới dạng List<object>, và map dưới dạng Dictionary<string, object>
    static Dictionary<string, object> ConvertToFirestoreData(LevelData data)
    {
        // 1. Xử lý Map ID (Lấy tên Prefab)
        string mapId = "UnknownMap";
        if (data.mapPrefab != null) mapId = data.mapPrefab.name;

        // 2. Xử lý Waves (Mảng lồng nhau)
        List<object> wavesList = new List<object>();

        foreach (var wave in data.wavesInThisLevel)
        {
            // Tạo Dictionary cho từng Wave
            var waveDict = new Dictionary<string, object>();
            waveDict["timeBetweenGroups"] = wave.timeBetweenGroups;

            // Xử lý Groups trong Wave
            List<object> groupsList = new List<object>();
            foreach (var group in wave.groupsInWave)
            {
                var groupDict = new Dictionary<string, object>();
                groupDict["count"] = group.count;
                groupDict["spawnInterval"] = group.spawnInterval;
                groupDict["enemyType"] = group.enemyType.ToString(); // Enum -> String

                groupsList.Add(groupDict);
            }

            waveDict["groups"] = groupsList; // Gán list group vào wave
            wavesList.Add(waveDict);         // Gán wave vào list waves
        }

        // 3. Đóng gói Level hoàn chỉnh
        Dictionary<string, object> levelData = new Dictionary<string, object>
        {
            { "levelIndex", data.levelIndex },
            { "levelName", data.levelName },
            { "startingResources", data.startingResources },
            { "startingLives", data.startingLives },
            { "mapId", mapId },
            { "waves", wavesList } // Mảng waves đã xử lý
        };

        return levelData;
    }
}
#endif