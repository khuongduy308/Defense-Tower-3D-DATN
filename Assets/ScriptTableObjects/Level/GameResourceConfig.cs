using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameResources", menuName = "Manager/GameResources")]
public class GameResourceConfig : ScriptableObject
{
    [Header("Danh sách Map Prefab")]
    // Kéo tất cả Prefab Map vào đây (Map Level 1, Map Level 2...)
    // TÊN PREFAB phải đặt giống hệt mapId trên Mongo (VD: Map_Level_01)
    public List<GameObject> mapPrefabs;

    public GameObject GetMap(string mapId)
    {
        return mapPrefabs.Find(m => m.name == mapId);
    }
}