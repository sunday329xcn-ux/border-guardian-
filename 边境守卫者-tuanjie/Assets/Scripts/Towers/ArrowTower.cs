using UnityEngine;

public class ArrowTower : CombatTowerBase
{
    public const int BuildCost = 50;

    public static ArrowTower Build(BuildSlot slot)
    {
        if (!TowerVisualFactory.TryPayForBuild(slot, BuildCost))
        {
            Debug.Log("Not enough gold for Arrow Tower.");
            return null;
        }

        var towerObject = TowerVisualFactory.CreateTowerObject("ArrowTower", slot, new Color(0.82f, 0.72f, 0.28f));
        var tower = towerObject.AddComponent<ArrowTower>();
        tower.normalColor = new Color(0.82f, 0.72f, 0.28f);
        tower.Setup(slot, BuildCost, TowerType.Arrow);
        if (tower == null)
        {
            TowerVisualFactory.RefundBuild(slot, BuildCost);
            return null;
        }

        return tower;
    }

    protected override void ApplyLevelStats()
    {
        ResetCombatModifiers();
        canTargetFlying = true;

        switch (level)
        {
            case 1:
                minDamage = 5; maxDamage = 8; attackInterval = 0.6f;
                break;
            case 2:
                minDamage = 10; maxDamage = 14; attackInterval = 0.4f;
                break;
            case 3:
                minDamage = 14; maxDamage = 19; attackInterval = 0.35f; critChance = 0.1f;
                break;
            case 4:
                minDamage = 16; maxDamage = 22; attackInterval = 0.32f; critChance = 0.18f;
                break;
            case 5 when branch == TowerBranch.BranchA:
                minDamage = 20; maxDamage = 27; attackInterval = 0.28f; critChance = 0.15f;
                prioritizeLowestHealth = true;
                break;
            case 5 when branch == TowerBranch.BranchB:
                minDamage = 30; maxDamage = 42; attackInterval = 1.35f;
                pierceLine = true; pierceArmorBonus = 0.45f; canTargetFlying = false;
                break;
        }

        transform.localScale = Vector3.one * (0.72f + level * 0.03f);
        TowerRangeScaling.ApplyTo(this);
    }

    void ResetCombatModifiers()
    {
        critChance = armorPenetration = splashRadius = stunDuration = groundZoneRadius = groundZoneDuration = groundZoneSlow = groundZoneDps = pierceArmorBonus = freezeChance = freezeDuration = 0f;
        splashMaxTargets = 1; prioritizeLowestHealth = prioritizeHighestHealth = pierceLine = false; armorStealPerHit = 0;
        ConfigureSynergyDefaults();
    }

    protected override int GetUpgradeGoldCost(int targetLevel) => targetLevel switch { 2 => 80, 3 => 120, _ => 0 };
    protected override int GetUpgradeDiamondCost(int targetLevel) => targetLevel switch { 4 => 5, 5 => 10, _ => 0 };
}
