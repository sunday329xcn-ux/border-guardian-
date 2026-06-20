using UnityEngine;

public class ArcaneTower : CombatTowerBase
{
    public const int BuildCost = 70;

    public static ArcaneTower Build(BuildSlot slot)
    {
        if (!TowerVisualFactory.TryPayForBuild(slot, BuildCost))
        {
            Debug.Log("Not enough gold for Arcane Tower.");
            return null;
        }

        var towerObject = TowerVisualFactory.CreateTowerObject("ArcaneTower", slot, new Color(0.62f, 0.35f, 0.85f));
        var tower = towerObject.AddComponent<ArcaneTower>();
        tower.normalColor = new Color(0.62f, 0.35f, 0.85f);
        tower.Setup(slot, BuildCost, TowerType.Arcane);
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
                minDamage = 10; maxDamage = 14; attackInterval = 0.9f; armorPenetration = 20f;
                break;
            case 2:
                minDamage = 15; maxDamage = 21; attackInterval = 0.9f; armorPenetration = 35f;
                break;
            case 3:
                minDamage = 20; maxDamage = 28; attackInterval = 0.88f; armorPenetration = 45f;
                break;
            case 4:
                minDamage = 24; maxDamage = 32; attackInterval = 0.82f; armorPenetration = 45f; armorStealPerHit = 2;
                break;
            case 5 when branch == TowerBranch.BranchA:
                minDamage = 27; maxDamage = 36; attackInterval = 0.78f; armorPenetration = 45f;
                armorStealPerHit = 2; destroyerAllyBonus = 2;
                break;
            case 5 when branch == TowerBranch.BranchB:
                minDamage = 14; maxDamage = 20; attackInterval = 1.25f;
                voidPulseCooldown = 8f;
                break;
        }

        transform.localScale = Vector3.one * (0.72f + level * 0.03f);
        TowerRangeScaling.ApplyTo(this);
    }

    void ResetCombatModifiers()
    {
        critChance = armorPenetration = splashRadius = stunDuration = groundZoneRadius = groundZoneDuration = groundZoneSlow = groundZoneDps = pierceArmorBonus = freezeChance = freezeDuration = 0f;
        splashMaxTargets = 1; prioritizeLowestHealth = prioritizeHighestHealth = pierceLine = false; armorStealPerHit = 0;
        voidPulseCooldown = 8f; voidPulseTimer = 0f;
        ConfigureSynergyDefaults();
    }

    protected override int GetUpgradeGoldCost(int targetLevel) => targetLevel switch { 2 => 110, 3 => 150, _ => 0 };
    protected override int GetUpgradeDiamondCost(int targetLevel) => targetLevel switch { 4 => 5, 5 => 10, _ => 0 };
}
