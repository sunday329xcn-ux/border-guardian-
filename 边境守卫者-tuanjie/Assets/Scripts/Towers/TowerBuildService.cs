using UnityEngine;

public static class TowerBuildService
{
    public static bool TryBuild(TowerType type, BuildSlot slot)
    {
        if (!TowerBuildCatalog.IsImplemented(type))
            return false;

        if (slot == null || !slot.CanAcceptBuild())
        {
            TowerBuildFeedback.ShowSelectPlatformFirst();
            return false;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return false;

        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return false;

        if (!PlatformTerrainCatalog.CanBuild(slot.TerrainType, type, out var restrictionReason))
        {
            TowerBuildFeedback.Show(restrictionReason);
            return false;
        }

        var cost = TowerBuildCatalog.GetBuildCost(type);
        if (GameManager.Instance == null || GameManager.Instance.Gold < cost)
        {
            TowerBuildFeedback.ShowInsufficientGold(type);
            return false;
        }

        var builtTower = TowerFactory.Build(type, slot);
        if (builtTower == null)
        {
            TowerBuildFeedback.ShowInsufficientGold(type);
            return false;
        }

        BuildSlotSelectionController.Deselect();
        TowerSelectionController.Select(builtTower);
        EnemySelectionController.Deselect();
        EasterEggController.Instance?.RegisterTowerBuilt(type);
        return true;
    }
}
