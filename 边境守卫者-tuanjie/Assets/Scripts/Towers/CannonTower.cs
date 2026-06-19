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
                minDamage = 11; maxDamage = 17; attackInterval = 2.0f; range = 3.5f;
                break;
            case 2:
                minDamage = 18; maxDamage = 28; attackInterval = 1.8f; range = 3.5f; stunDuration = 0.3f;
                break;
            case 3:
                minDamage = 24; maxDamage = 36; attackInterval = 1.7f; range = 3.5f; splashRadius = 1.2f; stunDuration = 0.5f;
                break;
            case 4:
                minDamage = 28; maxDamage = 40; attackInterval = 1.6f; range = 3.5f; splashRadius = 1.3f;
                groundZoneRadius = 1.1f; groundZoneDuration = 2f; groundZoneDps = 6f;
                break;
            case 5 when branch == TowerBranch.BranchA:
                minDamage = 34; maxDamage = 48; attackInterval = 1.5f; range = 3.5f; splashRadius = 1.4f;
                groundZoneRadius = 1.5f; groundZoneDuration = 3f; groundZoneDps = 8f; groundZoneSlow = 0.2f;
                break;
            case 5 when branch == TowerBranch.BranchB:
                minDamage = 48; maxDamage = 68; attackInterval = 2.2f; range = 5.5f; splashRadius = 1.2f;
                prioritizeHighestHealth = true; armorPenetration = 15f; thunderArmorShred = 8f;
                break;
        }

        transform.localScale = Vector3.one * (0.78f + level * 0.03f);
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
