using UnityEngine;

public class ArrowTower : CombatTowerBase
{
    public const int BuildCost = 50;

    public static ArrowTower Build(BuildSlot slot)
    {
        if (!TowerVisualFactory.TryPayAndOccupy(slot, BuildCost))
        {
            Debug.Log("Not enough gold for Arrow Tower.");
            return null;
        }

        var towerObject = TowerVisualFactory.CreateTowerObject("ArrowTower", slot, new Color(0.82f, 0.72f, 0.28f));
        var tower = towerObject.AddComponent<ArrowTower>();
        tower.normalColor = new Color(0.82f, 0.72f, 0.28f);
        tower.Setup(slot, BuildCost, TowerType.Arrow);
        return tower;
    }

    protected override void ApplyLevelStats()
    {
        ResetCombatModifiers();
        canTargetFlying = true;

        switch (level)
        {
            case 1:
                minDamage = 4; maxDamage = 6; attackInterval = 0.6f; range = 3.5f;
                break;
            case 2:
                minDamage = 8; maxDamage = 11; attackInterval = 0.4f; range = 3.5f;
                break;
            case 3:
                minDamage = 14; maxDamage = 18; attackInterval = 0.35f; range = 3.5f; critChance = 0.1f;
                break;
            case 4:
                minDamage = 20; maxDamage = 26; attackInterval = 0.3f; range = 4.5f; critChance = 0.2f;
                break;
            case 5 when branch == TowerBranch.BranchA:
                minDamage = 28; maxDamage = 36; attackInterval = 0.25f; range = 4.5f; critChance = 0.2f;
                prioritizeLowestHealth = true;
                break;
            case 5 when branch == TowerBranch.BranchB:
                minDamage = 50; maxDamage = 70; attackInterval = 1.2f; range = 6f;
                pierceLine = true; pierceArmorBonus = 0.5f; canTargetFlying = false;
                break;
        }

        transform.localScale = Vector3.one * (0.72f + level * 0.03f);
    }

    void ResetCombatModifiers()
    {
        critChance = armorPenetration = splashRadius = stunDuration = groundZoneRadius = groundZoneDuration = groundZoneSlow = groundZoneDps = pierceArmorBonus = freezeChance = freezeDuration = 0f;
        splashMaxTargets = 1; prioritizeLowestHealth = prioritizeHighestHealth = pierceLine = false; armorStealPerHit = 0;
    }

    protected override int GetUpgradeGoldCost(int targetLevel) => targetLevel switch { 2 => 80, 3 => 120, _ => 0 };
    protected override int GetUpgradeDiamondCost(int targetLevel) => targetLevel switch { 4 => 5, 5 => 10, _ => 0 };
}
