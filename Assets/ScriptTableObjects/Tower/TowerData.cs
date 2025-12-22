using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData", order = 1)]
public class TowerData : ScriptableObject
{
    public float range;  //tầm bắn
    public float shootInterval; // khoảng thời gian bắn
    public float projectileSpeed;  //Tốc độ viên đạn
    public float projectileDuration;  // Thời gian tồn tại của viên đạn
    public float projectileSize; // độ lớn viên đạn
    public float damage;  //sát thương

    public int cost;
    public Sprite sprite;

    public GameObject prefab;

    [Header("Audio")]
    public string shootSoundName;
    public string impactSoundName;

    public TowerData nextUpgrade;
}