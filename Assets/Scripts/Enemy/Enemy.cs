using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData data;

    private Path _currentPath;

    private Vector3 _targetPosition;
    private int _currentWaypoint = 0;

    private void Awake()
    {
        if (_currentPath == null)
        {
            // _currentPath = FindObjectOfType<Path>();
            _currentPath = GameObject.Find("Path1").GetComponent<Path>();
        }
    }

    private void OnEnable()
    {
        _currentWaypoint = 0;
        _targetPosition = _currentPath.GetPosition(_currentWaypoint);
        transform.GetChild(0).GetComponent<Animator>().Play("Run");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, data.speed * Time.deltaTime);

        float relativeDistance = (transform.position - _targetPosition).magnitude;
        if (relativeDistance < 0.1f)
        {
            _currentWaypoint++;
            if (_currentWaypoint < _currentPath.waypoints.Length)
            {
                _targetPosition = _currentPath.GetPosition(_currentWaypoint);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
