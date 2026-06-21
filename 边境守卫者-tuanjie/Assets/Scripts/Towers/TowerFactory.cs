public static class TowerFactory
{
    public static TowerBase Build(TowerType type, BuildSlot slot)
    {
        if (!TowerBuildCatalog.IsImplemented(type))
        {
            UnityEngine.Debug.Log($"{TowerBuildCatalog.GetDisplayName(type)} is not implemented yet.");
            return null;
        }

        return type switch
        {
            TowerType.Arrow => ArrowTower.Build(slot),
            TowerType.Frost => FrostTower.Build(slot),
            TowerType.Cannon => CannonTower.Build(slot),
            TowerType.Arcane => ArcaneTower.Build(slot),
            TowerType.Barracks => BarracksTower.Build(slot),
            TowerType.DiamondMine => DiamondMineTower.Build(slot),
            TowerType.Spotter => SpotterTower.Build(slot),
            TowerType.Beacon => BeaconTower.Build(slot),
            TowerType.BountyShrine => BountyShrineTower.Build(slot),
            _ => null
        };
    }
}
