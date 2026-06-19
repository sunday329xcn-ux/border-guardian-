using UnityEngine;

public class DiamondMineTower : TowerBase
{
    public const int BuildCost = 120;

    float diamondAccumulator;

    public static DiamondMineTower Build(BuildSlot slot)
    {
        if (!TowerVisualFactory.TryPayForBuild(slot, BuildCost))
        {
            Debug.Log("Not enough gold for Diamond Mine.");
            return null;
        }

        var towerObject = TowerVisualFactory.CreateTowerObject("DiamondMine", slot, new Color(0.45f, 0.85f, 0.95f));
        var tower = towerObject.AddComponent<DiamondMineTower>();
        tower.normalColor = new Color(0.45f, 0.85f, 0.95f);
        tower.Setup(slot, BuildCost, TowerType.DiamondMine);
        if (tower == null)
        {
            TowerVisualFactory.RefundBuild(slot, BuildCost);
            return null;
        }

        return tower;
    }

    protected override void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        diamondAccumulator += 0.1f * Time.deltaTime;
        if (diamondAccumulator < 1f)
            return;

        var diamonds = Mathf.FloorToInt(diamondAccumulator);
        diamondAccumulator -= diamonds;
        GameManager.Instance.AddDiamonds(diamonds);
    }

    protected override void ApplyLevelStats()
    {
        transform.localScale = Vector3.one * 0.85f;
    }

    protected override int GetUpgradeGoldCost(int targetLevel) => 0;
    protected override int GetUpgradeDiamondCost(int targetLevel) => 0;

    public override TowerUpgradeKind GetNextUpgradeKind() => TowerUpgradeKind.None;
}
