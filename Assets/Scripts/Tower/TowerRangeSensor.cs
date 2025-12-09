using UnityEngine;

public class TowerRangeSensor : MonoBehaviour
{
    private BaseTower _parentTower;

    public void Initialize(BaseTower tower)
    {
        _parentTower = tower;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_parentTower != null)
        {
            // Gọi hàm xử lý bên tháp cha
            _parentTower.OnEnemyEnterRange(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_parentTower != null)
        {
            _parentTower.OnEnemyExitRange(collision);
        }
    }
}