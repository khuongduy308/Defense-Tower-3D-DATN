using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public float lives;
    public int damage;
    public float attackDamage;
    public float attackInterval = 1.5f; // Tốc độ đánh của quái
    public float speed;
    public float resourceReward;
}
