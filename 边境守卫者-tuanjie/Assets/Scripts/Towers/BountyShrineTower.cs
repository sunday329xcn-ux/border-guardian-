using UnityEngine;

public class BountyShrineTower : TowerBase
{
    public const int BuildCost = 100;

    public static BountyShrineTower Build(BuildSlot slot)
    {
        if (!TowerVisualFactory.TryPayForBuild(slot, BuildCost))
        {
            Debug.Log("Not enough gold for Bounty Shrine.");
            return null;
        }

        var towerObject = TowerVisualFactory.CreateTowerObject("BountyShrine", slot, new Color(0.55f, 0.88f, 0.45f));
        var tower = towerObject.AddComponent<BountyShrineTower>();
        tower.normalColor = new Color(0.55f, 0.88f, 0.45f);
        tower.Setup(slot, BuildCost, TowerType.BountyShrine);
        if (tower == null)
        {
            TowerVisualFactory.RefundBuild(slot, BuildCost);
            return null;
        }

        return tower;
    }

    protected override void ApplyLevelStats()
    {
        transform.localScale = Vector3.one * 0.72f;
    }

    protected override int GetUpgradeGoldCost(int targetLevel) => 0;
    protected override int GetUpgradeDiamondCost(int targetLevel) => 0;

    public override TowerUpgradeKind GetNextUpgradeKind() => TowerUpgradeKind.None;
}
