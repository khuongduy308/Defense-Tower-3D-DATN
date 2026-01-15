using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Path : MonoBehaviour
{
    public GameObject[] waypoints;

    public Vector3 GetPosition(int index)
    {
        if (index < waypoints.Length)
        {
            return waypoints[index].transform.position;
        }
        return Vector3.zero;
    }

}
