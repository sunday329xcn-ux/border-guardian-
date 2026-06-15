using System.Collections.Generic;
using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    readonly List<Vector3> waypoints = new();

    public IReadOnlyList<Vector3> Waypoints => waypoints;
    public Vector3 StartPoint => waypoints.Count > 0 ? waypoints[0] : transform.position;
    public Vector3 EndPoint => waypoints.Count > 0 ? waypoints[^1] : transform.position;

    public void SetWaypoints(IEnumerable<Vector3> points)
    {
        waypoints.Clear();
        waypoints.AddRange(points);
    }
}
