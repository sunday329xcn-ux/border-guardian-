public static class TowerBuildCatalog
{
    public static int GetBuildCost(TowerType type)
    {
        return type switch
        {
            TowerType.Arrow => ArrowTower.BuildCost,
            TowerType.Frost => FrostTower.BuildCost,
            TowerType.Cannon => CannonTower.BuildCost,
            TowerType.Arcane => ArcaneTower.BuildCost,
            TowerType.Barracks => BarracksTower.BuildCost,
            TowerType.DiamondMine => DiamondMineTower.BuildCost,
            _ => 0
        };
    }

    public static string GetDisplayName(TowerType type)
    {
        return type switch
        {
            TowerType.Arrow => "Arrow",
            TowerType.Frost => "Frost",
            TowerType.Cannon => "Cannon",
            TowerType.Arcane => "Arcane",
            TowerType.Barracks => "Barracks",
            TowerType.DiamondMine => "Mine",
            _ => type.ToString()
        };
    }

    public static string GetBranchName(TowerType type, TowerBranch branch)
    {
        if (branch == TowerBranch.BranchA)
        {
            return type switch
            {
                TowerType.Arrow => "Ranger",
                TowerType.Frost => "Permafrost",
                TowerType.Cannon => "Incendiary",
                TowerType.Arcane => "Destroyer",
                TowerType.Barracks => "Paladin",
                _ => "Branch A"
            };
        }

        return type switch
        {
            TowerType.Arrow => "Siege",
            TowerType.Frost => "Blizzard",
            TowerType.Cannon => "Thunder",
            TowerType.Arcane => "Void Rift",
            TowerType.Barracks => "Assassin",
            _ => "Branch B"
        };
    }

    public static bool IsImplemented(TowerType type)
    {
        return type switch
        {
            TowerType.Arrow or TowerType.Frost or TowerType.Cannon or TowerType.Arcane or TowerType.Barracks or TowerType.DiamondMine => true,
            _ => false
        };
    }

    public static bool SupportsUpgrade(TowerType type)
    {
        return type != TowerType.DiamondMine;
    }

    public static float GetPreviewRange(TowerType type) => TowerRangeScaling.GetPreviewAttackRange(type);

    public static bool ShowsCombatRangeRing(TowerType type)
    {
        return type != TowerType.DiamondMine;
    }

    public static string GetPreviewRingLabel(TowerType type)
    {
        return type == TowerType.Barracks ? "Rally" : "Range";
    }
}
