using UnityEngine;

public class CannonTower : CombatTowerBase
{
    public const int BuildCost = 100;

    public static CannonTower Build(BuildSlot slot)
    {
        if (!TowerVisualFactory.TryPayForBuild(slot, BuildCost))
        {
            Debug.Log("Not enough gold for Cannon Tower.");
            return null;
        }

        var towerObject = TowerVisualFactory.CreateTowerObject("CannonTower", slot, new Color(0.55f, 0.55f, 0.6f));
        var tower = towerObject.AddComponent<CannonTower>();
        tower.normalColor = new Color(0.55f, 0.55f, 0.6f);
        tower.Setup(slot, BuildCost, TowerType.Cannon);
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
        canTargetFlying = false;
        splashRadius = 1f;
        splashMaxTargets = 3;

        switch (level)
        {
            case 1:
                minDamage = 11; maxDamage = 17; attackInterval = 2.0f;
                break;
            case 2:
                minDamage = 17; maxDamage = 26; attackInterval = 1.85f; stunDuration = 0.3f;
                break;
            case 3:
                minDamage = 22; maxDamage = 34; attackInterval = 1.75f; splashRadius = 1.2f; stunDuration = 0.5f;
                break;
            case 4:
                minDamage = 26; maxDamage = 38; attackInterval = 1.65f; splashRadius = 1.3f;
                groundZoneRadius = 1.1f; groundZoneDuration = 2f; groundZoneDps = 5f;
                break;
            case 5 when branch == TowerBranch.BranchA:
                minDamage = 30; maxDamage = 44; attackInterval = 1.55f; splashRadius = 1.4f;
                groundZoneRadius = 1.5f; groundZoneDuration = 3f; groundZoneDps = 7f; groundZoneSlow = 0.2f;
                break;
            case 5 when branch == TowerBranch.BranchB:
                minDamage = 42; maxDamage = 60; attackInterval = 2.3f; splashRadius = 1.2f;
                prioritizeHighestHealth = true; armorPenetration = 12f; thunderArmorShred = 7f;
                break;
        }

        transform.localScale = Vector3.one * (0.78f + level * 0.03f);
        TowerRangeScaling.ApplyTo(this);
    }

    void ResetCombatModifiers()
    {
        critChance = armorPenetration = splashRadius = stunDuration = groundZoneRadius = groundZoneDuration = groundZoneSlow = groundZoneDps = pierceArmorBonus = freezeChance = freezeDuration = 0f;
        splashMaxTargets = 1; prioritizeLowestHealth = prioritizeHighestHealth = pierceLine = false; armorStealPerHit = 0;
        ConfigureSynergyDefaults();
    }

    protected override int GetUpgradeGoldCost(int targetLevel) => targetLevel switch { 2 => 120, 3 => 180, _ => 0 };
    protected override int GetUpgradeDiamondCost(int targetLevel) => targetLevel switch { 4 => 6, 5 => 12, _ => 0 };
}
