using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData", order = 1)]
public class TowerData : ScriptableObject
{
    // public string towerName;
    // public GameObject towerPrefab;
    public float range;
    // public float fireRate;
    // public int cost;
    // public int upgradeCost;
    // public int sellValue;
    public float shootInterval;
    public float projectileSpeed;
    public float projectileDuration;
    public float damage;
}