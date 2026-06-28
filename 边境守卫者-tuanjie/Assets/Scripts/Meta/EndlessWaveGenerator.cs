using System;
using System.Collections.Generic;

/// <summary>
/// Procedurally builds Endless waves (P4.3). Deterministic per round (seeded by
/// the round number) so a given run length is reproducible for scoring. Count
/// scales with the round; a boss is added every 5th round. Enemy HP scaling is
/// handled separately by <see cref="EndlessScalingService"/>.
/// </summary>
public static class EndlessWaveGenerator
{
    static readonly EnemyType[] Pool =
    {
        EnemyType.Orc,
        EnemyType.GoblinRipper,
        EnemyType.Wraith,
        EnemyType.RockGolem,
        EnemyType.FireBomber,
        EnemyType.ShadowPriest,
        EnemyType.WolfRider,
        EnemyType.ShieldBearer,
        EnemyType.SplitSlime,
        EnemyType.Shade,
        EnemyType.WarDrummer,
        EnemyType.Nullifier,
        EnemyType.BatSwarm
    };

    public static WaveDefinition Generate(int endlessRound)
    {
        var rng = new Random(1000 + endlessRound);
        var groups = new List<WaveSpawnGroup>();

        var groupCount = 2 + endlessRound / 2;
        var totalBudget = 8 + endlessRound * 3;

        for (int i = 0; i < groupCount; i++)
        {
            var type = Pool[rng.Next(Pool.Length)];
            var count = UnityEngine.Mathf.Max(2, totalBudget / groupCount);
            groups.Add(new WaveSpawnGroup
            {
                enemyType = type,
                count = count,
                spawnInterval = 0.55f,
                delayBeforeGroup = i == 0 ? 0f : 1.2f
            });
        }

        if (endlessRound % 5 == 0)
        {
            groups.Add(new WaveSpawnGroup
            {
                enemyType = EnemyType.AncientDragon,
                count = 1 + endlessRound / 10,
                spawnInterval = 2f,
                delayBeforeGroup = 2.5f
            });
        }

        return new WaveDefinition
        {
            note = $"Endless {endlessRound}",
            hintText = endlessRound % 5 == 0 ? "Endless boss wave" : "Endless wave",
            groups = groups.ToArray()
        };
    }
}
