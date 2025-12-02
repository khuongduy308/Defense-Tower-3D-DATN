using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor; // Chỉ chạy trong Editor
#endif

public class LevelUploader : MonoBehaviour
{

}

#if UNITY_EDITOR
public class LevelUploadMenu
{
    // Đường dẫn Server
    static string serverUrl = "http://localhost:3000/api/level";

    // Tạo menu chuột phải vào file LevelData
    [MenuItem("Assets/TowerDefense/Upload Level To Cloud")]
    public static void UploadLevel()
    {
        // 1. Lấy file đang được chọn
        LevelData selectedLevel = Selection.activeObject as LevelData;

        if (selectedLevel == null)
        {
            Debug.LogError("Hãy chọn một file LevelData (ScriptableObject)!");
            return;
        }

        // 2. Chuyển đổi LevelData (Unity) -> LevelDataDTO (JSON)
        LevelDataDTO dto = ConvertToDTO(selectedLevel);
        string json = JsonUtility.ToJson(dto);

        // 3. Gửi lên Server
        // Vì Editor không chạy Coroutine, ta dùng EditorCoroutine hoặc gửi Async đơn giản
        UploadToServer(json);
    }

    static void UploadToServer(string json)
    {
        var request = new UnityWebRequest(serverUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Đang tải lên server...");

        var operation = request.SendWebRequest();
        
        // Chờ request chạy xong (Kiểu thủ công trong Editor)
        operation.completed += (op) => 
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"<color=green>UPLOAD THÀNH CÔNG!</color> Server phản hồi: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"UPLOAD THẤT BẠI: {request.error}\n{request.downloadHandler.text}");
            }
            request.Dispose();
        };
    }

    // Hàm chuyển đổi dữ liệu ngược (Unity -> DTO)
    static LevelDataDTO ConvertToDTO(LevelData data)
    {
        LevelDataDTO dto = new LevelDataDTO();
        dto.levelIndex = data.levelIndex;
        dto.levelName = data.levelName;
        dto.startingResources = data.startingResources;
        dto.startingLives = data.startingLives;
        
        // Lấy tên Map Prefab làm mapId
        if (data.mapPrefab != null)
            dto.mapId = data.mapPrefab.name; 
        else 
            dto.mapId = "UnknownMap";

        dto.wavesInThisLevel = new List<WaveDTO>();

        foreach (var wave in data.wavesInThisLevel)
        {
            WaveDTO wDto = new WaveDTO();
            wDto.timeBetweenGroups = wave.timeBetweenGroups;
            wDto.groupsInWave = new List<EnemyGroupDTO>();

            foreach (var group in wave.groupsInWave)
            {
                EnemyGroupDTO gDto = new EnemyGroupDTO();
                gDto.count = group.count;
                gDto.spawnInterval = group.spawnInterval;
                // Chuyển Enum thành String
                gDto.enemyType = group.enemyType.ToString(); 

                wDto.groupsInWave.Add(gDto);
            }
            dto.wavesInThisLevel.Add(wDto);
        }

        return dto;
    }
}
#endif