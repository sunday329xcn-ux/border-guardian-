using UnityEngine;

public class FrostTower : CombatTowerBase
{
    public const int BuildCost = 75;

    public static FrostTower Build(BuildSlot slot)
    {
        if (!TowerVisualFactory.TryPayForBuild(slot, BuildCost))
        {
            Debug.Log("Not enough gold for Frost Tower.");
            return null;
        }

        var towerObject = TowerVisualFactory.CreateTowerObject("FrostTower", slot, new Color(0.45f, 0.78f, 0.95f));
        var tower = towerObject.AddComponent<FrostTower>();
        tower.normalColor = new Color(0.45f, 0.78f, 0.95f);
        tower.Setup(slot, BuildCost, TowerType.Frost);
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
                minDamage = 2; maxDamage = 4; attackInterval = 1.2f; groundZoneSlow = 0.4f;
                break;
            case 2:
                minDamage = 4; maxDamage = 7; attackInterval = 1.0f; groundZoneSlow = 0.6f; freezeChance = 0.15f; freezeDuration = 1f;
                break;
            case 3:
                minDamage = 7; maxDamage = 10; attackInterval = 0.9f; groundZoneSlow = 0.65f; freezeChance = 0.2f; freezeDuration = 1.5f;
                break;
            case 4:
                minDamage = 9; maxDamage = 13; attackInterval = 0.82f; groundZoneSlow = 0.3f;
                groundZoneRadius = 0.9f; groundZoneDuration = 2f; freezeChance = 0.4f; freezeDuration = 1.5f;
                break;
            case 5 when branch == TowerBranch.BranchA:
                minDamage = 11; maxDamage = 15; attackInterval = 0.78f; groundZoneSlow = 0.5f;
                groundZoneRadius = 1.3f; groundZoneDuration = 3f; freezeChance = 0.38f; freezeDuration = 2f;
                break;
            case 5 when branch == TowerBranch.BranchB:
                minDamage = 14; maxDamage = 20; attackInterval = 1.6f;
                splashRadius = 1.15f; splashMaxTargets = 5; freezeChance = 0.95f; freezeDuration = 1.35f; groundZoneSlow = 0.5f;
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

    protected override int GetUpgradeGoldCost(int targetLevel) => targetLevel switch { 2 => 100, 3 => 150, _ => 0 };
    protected override int GetUpgradeDiamondCost(int targetLevel) => targetLevel switch { 4 => 5, 5 => 10, _ => 0 };
}
