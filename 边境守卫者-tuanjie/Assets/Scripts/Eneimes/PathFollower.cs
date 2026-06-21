using System;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
    WaypointPath path;
    int waypointIndex;
    float moveSpeed;
    bool isMoving;
    bool isPaused;

    public bool IsMoving => isMoving && !isPaused;
    public bool IsPaused => isPaused;
    public float PathProgress => CalculatePathProgress();
    public event Action OnPathComplete;

    public void Begin(WaypointPath waypointPath, float speed, bool preservePosition = false)
    {
        path = waypointPath;
        moveSpeed = speed;
        waypointIndex = 0;
        isPaused = false;
        isMoving = path != null && path.Waypoints.Count > 0;

        if (isMoving && !preservePosition)
            transform.position = path.StartPoint;
    }

    public void SyncToPathProgress(float progress)
    {
        if (path == null || path.Waypoints.Count == 0)
            return;

        var clamped = Mathf.Clamp(progress, 0f, path.Waypoints.Count - 0.01f);
        waypointIndex = Mathf.Min(Mathf.FloorToInt(clamped), path.Waypoints.Count - 1);
        var fraction = clamped - waypointIndex;
        var previous = waypointIndex > 0 ? path.Waypoints[waypointIndex - 1] : path.StartPoint;
        var current = path.Waypoints[waypointIndex];
        transform.position = Vector3.Lerp(previous, current, fraction);
    }

    public Vector3 GetMovementDirection()
    {
        if (path == null || path.Waypoints.Count == 0)
            return Vector3.right;

        var target = path.Waypoints[Mathf.Min(waypointIndex, path.Waypoints.Count - 1)];
        var direction = target - transform.position;
        if (direction.sqrMagnitude < 0.0001f && waypointIndex > 0)
            direction = target - path.Waypoints[waypointIndex - 1];

        return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.right;
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Resume()
    {
        isPaused = false;
    }

    void Update()
    {
        if (!isMoving || isPaused || path == null || path.Waypoints.Count == 0)
            return;

        var enemy = GetComponent<EnemyBase>();
        if (enemy != null)
        {
            if (enemy.IsRooted || enemy.IsBlocked || enemy.IsStunned)
                return;

            enemy.SetBlocked(false);
        }

        var target = path.Waypoints[waypointIndex];
        var currentSpeed = moveSpeed;

        var slowEffect = GetComponent<EnemySlowEffect>();
        if (slowEffect != null)
            currentSpeed *= slowEffect.SpeedMultiplier;

        transform.position = Vector3.MoveTowards(transform.position, target, currentSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) > 0.02f)
            return;

        waypointIndex++;
        if (waypointIndex >= path.Waypoints.Count)
            CompletePath();
    }

    void CompletePath()
    {
        isMoving = false;
        OnPathComplete?.Invoke();
    }

    float CalculatePathProgress()
    {
        if (path == null || path.Waypoints.Count == 0)
            return 0f;

        if (waypointIndex >= path.Waypoints.Count)
            return path.Waypoints.Count;

        var previous = waypointIndex > 0 ? path.Waypoints[waypointIndex - 1] : path.StartPoint;
        var currentTarget = path.Waypoints[waypointIndex];
        var segmentLength = Vector3.Distance(previous, currentTarget);

        if (segmentLength <= 0.001f)
            return waypointIndex;

        var traveledOnSegment = 1f - Vector3.Distance(transform.position, currentTarget) / segmentLength;
        return waypointIndex + Mathf.Clamp01(traveledOnSegment);
    }
}
