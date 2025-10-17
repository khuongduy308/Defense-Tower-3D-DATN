using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private TowerData _data;
    private Vector3 _shootDirection;
    private float _projectileDuration;

    // Update is called once per frame
    void Update()
    {
        if (_projectileDuration <= 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            transform.position += new Vector3(_shootDirection.x, _shootDirection.y, 0) * _data.projectileSpeed * Time.deltaTime;
            _projectileDuration -= Time.deltaTime;
        }

    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(_data.damage);
                gameObject.SetActive(false);
            }
        }
    }

    public void Shoot(TowerData data, Vector2 shootDirection)
    {
        _data = data;
        _shootDirection = shootDirection;
        _projectileDuration = data.projectileDuration;

    }

}
