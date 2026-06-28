using System.Collections.Generic;
using UnityEngine;

public class BarracksTower : TowerBase
{
    public const int BuildCost = 60;

    readonly List<SoldierUnit> soldiers = new();

    Vector3 rallyPoint;
    float rallyRange;
    int soldierCount;
    int soldierHp;
    int soldierArmor;
    int soldierMinDamage;
    int soldierMaxDamage;
    float respawnInterval = 8f;
    float respawnTimer;
    float soldierAttackInterval = 1f;
    int deathExplosionDamage;
    Color soldierColor = new Color(0.35f, 0.55f, 0.95f);

    public override bool SupportsRally => true;
    public float RallyRange => rallyRange;
    public bool IsPlacingRally { get; private set; }

    public static BarracksTower Build(BuildSlot slot)
    {
        if (!TowerVisualFactory.TryPayForBuild(slot, BuildCost))
        {
            Debug.Log("Not enough gold for Barracks.");
            return null;
        }

        var towerObject = TowerVisualFactory.CreateTowerObject("Barracks", slot, new Color(0.35f, 0.55f, 0.95f));
        var tower = towerObject.AddComponent<BarracksTower>();
        tower.normalColor = new Color(0.35f, 0.55f, 0.95f);
        tower.Setup(slot, BuildCost, TowerType.Barracks);
        if (tower == null)
        {
            TowerVisualFactory.RefundBuild(slot, BuildCost);
            return null;
        }

        return tower;
    }

    protected override void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        soldiers.RemoveAll(s => s == null);
        respawnTimer -= Time.deltaTime;

        while (soldiers.Count < soldierCount && respawnTimer <= 0f)
        {
            SpawnSoldier();
            respawnTimer = respawnInterval;
        }
    }

    public void BeginRallyPlacement()
    {
        IsPlacingRally = true;
    }

    public void CancelRallyPlacement()
    {
        IsPlacingRally = false;
    }

    public bool TrySetRallyPoint(Vector3 worldPoint)
    {
        if (Vector2.Distance(transform.position, worldPoint) > rallyRange)
            return false;

        rallyPoint = worldPoint;
        IsPlacingRally = false;

        for (int i = 0; i < soldiers.Count; i++)
        {
            if (soldiers[i] == null)
                continue;

            var offset = Random.insideUnitCircle * 0.35f;
            soldiers[i].SetHoldPosition(rallyPoint + new Vector3(offset.x, offset.y, 0f));
        }

        return true;
    }

    public bool IsPointInsideRallyRange(Vector3 worldPoint)
    {
        return Vector2.Distance(transform.position, worldPoint) <= rallyRange;
    }

    protected override void ApplyLevelStats()
    {
        ClearSoldiers();
        deathExplosionDamage = 0;

        switch (level)
        {
            case 1:
                soldierCount = 2; soldierHp = 80; soldierArmor = 0; soldierMinDamage = 2; soldierMaxDamage = 4;
                respawnInterval = 8f; soldierAttackInterval = 1f;
                break;
            case 2:
                soldierCount = 3; soldierHp = 140; soldierArmor = 5; soldierMinDamage = 5; soldierMaxDamage = 8;
                respawnInterval = 7f; deathExplosionDamage = 10; soldierAttackInterval = 0.9f;
                break;
            case 3:
                soldierCount = 3; soldierHp = 200; soldierArmor = 10; soldierMinDamage = 9; soldierMaxDamage = 12;
                respawnInterval = 6f; deathExplosionDamage = 20; soldierAttackInterval = 0.8f;
                break;
            case 4:
                soldierCount = 3; soldierHp = 280; soldierArmor = 15; soldierMinDamage = 14; soldierMaxDamage = 18;
                respawnInterval = 5.5f; deathExplosionDamage = 30; soldierAttackInterval = 0.7f;
                break;
            case 5 when branch == TowerBranch.BranchA:
                soldierCount = 2; soldierHp = 400; soldierArmor = 25; soldierMinDamage = 20; soldierMaxDamage = 26;
                respawnInterval = 5f; deathExplosionDamage = 30; soldierAttackInterval = 0.65f;
                soldierColor = new Color(0.85f, 0.75f, 0.25f);
                break;
            case 5 when branch == TowerBranch.BranchB:
                soldierCount = 4; soldierHp = 150; soldierArmor = 5; soldierMinDamage = 28; soldierMaxDamage = 36;
                respawnInterval = 4f; deathExplosionDamage = 15; soldierAttackInterval = 0.5f;
                soldierColor = new Color(0.35f, 0.35f, 0.45f);
                break;
        }

        rallyPoint = transform.position + Vector3.right * 0.8f;
        respawnTimer = 0f;
        transform.localScale = Vector3.one * (0.78f + level * 0.03f);
        TowerRangeScaling.ApplyTo(this);

        for (int i = 0; i < soldierCount; i++)
            SpawnSoldier();
    }

    public void SetRallyRange(float value)
    {
        rallyRange = value;
    }

    public void ApplyTerrainModifiers(PlatformTerrainType terrain)
    {
        switch (terrain)
        {
            case PlatformTerrainType.Highland:
            case PlatformTerrainType.RuneRange:
                rallyRange *= 1.10f;
                break;
            case PlatformTerrainType.RuneAttackSpeed:
                respawnInterval /= 1.15f;
                break;
            case PlatformTerrainType.RuneSynergy:
                synergyRange *= 1.15f;
                break;
            case PlatformTerrainType.Fragile:
                soldierMinDamage = Mathf.Max(1, Mathf.RoundToInt(soldierMinDamage * 1.15f));
                soldierMaxDamage = Mathf.Max(soldierMinDamage, Mathf.RoundToInt(soldierMaxDamage * 1.15f));
                break;
        }
    }

    void SpawnSoldier()
    {
        var offset = Random.insideUnitCircle * 0.35f;
        var spawnPos = rallyPoint + new Vector3(offset.x, offset.y, 0f);
        var soldier = SoldierUnit.Spawn(this, spawnPos, soldierColor, soldierHp, soldierArmor, soldierMinDamage, soldierMaxDamage, soldierAttackInterval);
        soldiers.Add(soldier);
    }

    public void NotifySoldierDeath(SoldierUnit soldier, Vector3 deathPosition)
    {
        soldiers.Remove(soldier);
        OnSoldierDied(deathPosition);
    }

    void OnSoldierDied(Vector3 deathPosition)
    {
        if (deathExplosionDamage <= 0)
            return;

        foreach (var enemy in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            if (Vector2.Distance(enemy.transform.position, deathPosition) > 1.1f)
                continue;

            enemy.TakeDamage(deathExplosionDamage, DamageType.Physical, damageSource: deathPosition);
        }
    }

    protected override void OnSold()
    {
        ClearSoldiers();
    }

    protected override void OnDestroy()
    {
        ClearSoldiers();
        base.OnDestroy();
    }

    void ClearSoldiers()
    {
        foreach (var soldier in soldiers)
        {
            if (soldier != null)
                Destroy(soldier.gameObject);
        }

        soldiers.Clear();
    }

    protected override int GetUpgradeGoldCost(int targetLevel) => targetLevel switch { 2 => 90, 3 => 130, _ => 0 };
    protected override int GetUpgradeDiamondCost(int targetLevel) => targetLevel switch { 4 => 5, 5 => branch == TowerBranch.BranchA ? 12 : 10, _ => 0 };

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, rallyRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(rallyPoint, 0.15f);
    }
}
