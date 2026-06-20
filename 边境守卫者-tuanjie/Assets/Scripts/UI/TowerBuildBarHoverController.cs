using UnityEngine;

public static class TowerBuildBarHoverController
{
    static TowerType? hoveredTowerType;

    public static bool TryGetHovered(out TowerType towerType)
    {
        if (hoveredTowerType.HasValue)
        {
            towerType = hoveredTowerType.Value;
            return true;
        }

        towerType = default;
        return false;
    }

    public static void SetHovered(TowerType towerType)
    {
        hoveredTowerType = towerType;
    }

    public static void ClearIf(TowerType towerType)
    {
        if (hoveredTowerType == towerType)
            hoveredTowerType = null;
    }

    public static void Clear()
    {
        hoveredTowerType = null;
    }
}
