using UnityEngine;

public static class SupportTowerService
{
    public const float BeaconRadius = 2.5f;
    public const float BountyRadius = 2.8f;

    public static bool IsShadeRevealed(EnemyBase enemy)
    {
        if (enemy == null || enemy.IsDead || enemy.EnemyType != EnemyType.Shade)
            return false;

        foreach (var tower in TowerRegistry.ActiveTowersSnapshot)
        {
            if (tower is not SpotterTower)
                continue;

            if (Vector2.Distance(tower.transform.position, enemy.transform.position) <= SpotterTower.RevealRadius)
                return true;
        }

        return false;
    }

    public static float GetAttackSpeedMultiplier(Vector3 towerPosition)
    {
        foreach (var tower in TowerRegistry.ActiveTowersSnapshot)
        {
            if (tower is not BeaconTower)
                continue;

            if (Vector2.Distance(tower.transform.position, towerPosition) <= BeaconRadius)
                return 1.1f;
        }

        return 1f;
    }

    public static int CalculateGoldReward(Vector3 killPosition, int baseGold)
    {
        if (baseGold <= 0)
            return 0;

        foreach (var tower in TowerRegistry.ActiveTowersSnapshot)
        {
            if (tower is not BountyShrineTower)
                continue;

            if (Vector2.Distance(tower.transform.position, killPosition) <= BountyRadius)
                return Mathf.Max(1, Mathf.RoundToInt(baseGold * 1.15f));
        }

        return baseGold;
    }
}
