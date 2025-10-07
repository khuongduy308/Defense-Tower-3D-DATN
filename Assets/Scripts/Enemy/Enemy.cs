using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Path currentPath;

    private Vector3 _targetPosition;
    private int _currentWaypoint = 0;

    private void Awake()
    {
        if (currentPath == null)
        {
            // currentPath = FindObjectOfType<Path>();
            currentPath = GameObject.Find("Path1").GetComponent<Path>();
        }
    }

    private void OnEnable()
    {
        _currentWaypoint = 0;
        _targetPosition = currentPath.GetPosition(_currentWaypoint);
        transform.GetChild(0).GetComponent<Animator>().Play("Run");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.deltaTime);

        float relativeDistance = (transform.position - _targetPosition).magnitude;
        if (relativeDistance < 0.1f)
        {
            _currentWaypoint++;
            if (_currentWaypoint < currentPath.waypoints.Length)
            {
                _targetPosition = currentPath.GetPosition(_currentWaypoint);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
