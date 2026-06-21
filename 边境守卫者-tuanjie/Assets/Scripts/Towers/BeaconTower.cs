using UnityEngine;

public class BeaconTower : TowerBase
{
    public const int BuildCost = 90;

    public static BeaconTower Build(BuildSlot slot)
    {
        if (!TowerVisualFactory.TryPayForBuild(slot, BuildCost))
        {
            Debug.Log("Not enough gold for Beacon.");
            return null;
        }

        var towerObject = TowerVisualFactory.CreateTowerObject("BeaconTower", slot, new Color(0.98f, 0.65f, 0.28f));
        var tower = towerObject.AddComponent<BeaconTower>();
        tower.normalColor = new Color(0.98f, 0.65f, 0.28f);
        tower.Setup(slot, BuildCost, TowerType.Beacon);
        if (tower == null)
        {
            TowerVisualFactory.RefundBuild(slot, BuildCost);
            return null;
        }

        return tower;
    }

    protected override void ApplyLevelStats()
    {
        transform.localScale = Vector3.one * 0.7f;
    }

    protected override int GetUpgradeGoldCost(int targetLevel) => 0;
    protected override int GetUpgradeDiamondCost(int targetLevel) => 0;

    public override TowerUpgradeKind GetNextUpgradeKind() => TowerUpgradeKind.None;
}
