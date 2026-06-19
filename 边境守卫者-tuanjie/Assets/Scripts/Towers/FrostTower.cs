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
                minDamage = 2; maxDamage = 4; attackInterval = 1.2f; range = 2.5f; groundZoneSlow = 0.4f;
                break;
            case 2:
                minDamage = 4; maxDamage = 7; attackInterval = 1.0f; range = 3.0f; groundZoneSlow = 0.6f; freezeChance = 0.15f; freezeDuration = 1f;
                break;
            case 3:
                minDamage = 7; maxDamage = 10; attackInterval = 0.9f; range = 3.0f; groundZoneSlow = 0.65f; freezeChance = 0.2f; freezeDuration = 1.5f;
                break;
            case 4:
                minDamage = 10; maxDamage = 14; attackInterval = 0.8f; range = 3.0f; groundZoneSlow = 0.3f;
                groundZoneRadius = 0.9f; groundZoneDuration = 2f; freezeChance = 0.45f; freezeDuration = 1.5f;
                break;
            case 5 when branch == TowerBranch.BranchA:
                minDamage = 12; maxDamage = 16; attackInterval = 0.75f; range = 3.0f; groundZoneSlow = 0.5f;
                groundZoneRadius = 1.3f; groundZoneDuration = 3f; freezeChance = 0.4f; freezeDuration = 2f;
                break;
            case 5 when branch == TowerBranch.BranchB:
                minDamage = 18; maxDamage = 26; attackInterval = 1.5f; range = 3.0f;
                splashRadius = 1.2f; splashMaxTargets = 6; freezeChance = 1f; freezeDuration = 1.5f; groundZoneSlow = 0.5f;
                break;
        }

        transform.localScale = Vector3.one * (0.72f + level * 0.03f);
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
