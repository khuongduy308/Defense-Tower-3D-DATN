using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameResources", menuName = "Manager/GameResources")]
public class GameResourceConfig : ScriptableObject
{
    [Header("Danh sách Map Prefab")]

    public List<GameObject> mapPrefabs;

    public GameObject GetMap(string mapId)
    {
        return mapPrefabs.Find(m => m.name == mapId);
    }
}