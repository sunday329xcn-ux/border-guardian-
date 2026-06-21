using UnityEngine;

public class SpotterTower : TowerBase
{
    public const int BuildCost = 80;
    public const float RevealRadius = 3f;

    public static SpotterTower Build(BuildSlot slot)
    {
        if (!TowerVisualFactory.TryPayForBuild(slot, BuildCost))
        {
            Debug.Log("Not enough gold for Spotter.");
            return null;
        }

        var towerObject = TowerVisualFactory.CreateTowerObject("SpotterTower", slot, new Color(0.95f, 0.75f, 0.35f));
        var tower = towerObject.AddComponent<SpotterTower>();
        tower.normalColor = new Color(0.95f, 0.75f, 0.35f);
        tower.Setup(slot, BuildCost, TowerType.Spotter);
        if (tower == null)
        {
            TowerVisualFactory.RefundBuild(slot, BuildCost);
            return null;
        }

        return tower;
    }

    protected override void ApplyLevelStats()
    {
        transform.localScale = Vector3.one * 0.68f;
    }

    protected override int GetUpgradeGoldCost(int targetLevel) => 0;
    protected override int GetUpgradeDiamondCost(int targetLevel) => 0;

    public override TowerUpgradeKind GetNextUpgradeKind() => TowerUpgradeKind.None;
}
