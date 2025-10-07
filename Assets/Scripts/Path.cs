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

    private void OnDrawGizmos()
    {
        if (waypoints.Length > 0) {
            for (int i = 0; i < waypoints.Length; i++)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.green;
                style.alignment = TextAnchor.MiddleCenter;
                Handles.Label(waypoints[i].transform.position + Vector3.up * 0.7f, waypoints[i].name, style);
                
                if (i < waypoints.Length - 1)
                {
                    Gizmos.color = Color.gray;
                    Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i + 1].transform.position);
                }
            }
        }
    }
}
