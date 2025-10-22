using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private TowerData _data;
    // private Vector3 _shootDirection;
    private float _projectileDuration;
    private Enemy _target;

    // Update is called once per frame
    void Update()
    {
        // if (_projectileDuration <= 0)
        // {
        //     gameObject.SetActive(false);
        // }
        // else
        // {
        //     transform.position += new Vector3(_shootDirection.x, _shootDirection.y, 0) * _data.projectileSpeed * Time.deltaTime;
        //     _projectileDuration -= Time.deltaTime;
        // }

        _projectileDuration -= Time.deltaTime; // Luôn giảm thời gian

        // Ktra nếu mục tiêu đã chết, hoặc hết thời gian
        if (_target == null || !_target.gameObject.activeInHierarchy || _projectileDuration <= 0)
        {
            gameObject.SetActive(false);
            return; // Dừng hàm Update
        }

        // Tính hướng từ vị trí hiện tại của đạn đến vị trí hiện tại của mục tiêu ở mỗi frame
        Vector3 direction = (_target.transform.position - transform.position).normalized;

        // Xoay mũi tên (trục X) theo hướng bay mới
        transform.right = direction;

        // Bay theo hướng vừa tính
        transform.position += direction * _data.projectileSpeed * Time.deltaTime;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && enemy == _target)
            {
                enemy.TakeDamage(_data.damage);
                gameObject.SetActive(false);
            }
        }
    }

    // public void Shoot(TowerData data, Vector2 shootDirection)
    // {
    //     _data = data;
    //     _shootDirection = shootDirection;
    //     _projectileDuration = data.projectileDuration;
    //     transform.right = _shootDirection;
    // }

    public void Shoot(TowerData data, Enemy target) 
    {
        _data = data;
        _target = target; // <-- Lưu mục tiêu
        _projectileDuration = data.projectileDuration;
    }

}
