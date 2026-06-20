using UnityEngine;

public static class BuildSlotPlacementUtility
{
    public const float DefaultPickRadius = 0.55f;

    public static BuildSlot FindBuildSlotAt(MapGridController map, Vector3 worldPoint, float radius = DefaultPickRadius)
    {
        if (map?.BuildSlots == null)
            return null;

        BuildSlot closestSlot = null;
        var closestDistanceSqr = radius * radius;

        foreach (var slot in map.BuildSlots)
        {
            if (slot == null || !slot.CanAcceptBuild())
                continue;

            var offset = slot.transform.position - worldPoint;
            offset.z = 0f;
            var distanceSqr = offset.sqrMagnitude;
            if (distanceSqr > closestDistanceSqr)
                continue;

            closestDistanceSqr = distanceSqr;
            closestSlot = slot;
        }

        return closestSlot;
    }

    public static BuildSlot FindBuildSlotAtScreen(MapGridController map, Camera camera, Vector2 screenPoint,
        float radius = DefaultPickRadius)
    {
        if (map == null || camera == null)
            return null;

        var screenZ = Mathf.Abs(camera.transform.position.z);
        var worldPoint = camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, screenZ));
        worldPoint.z = 0f;
        return FindBuildSlotAt(map, worldPoint, radius);
    }
}
