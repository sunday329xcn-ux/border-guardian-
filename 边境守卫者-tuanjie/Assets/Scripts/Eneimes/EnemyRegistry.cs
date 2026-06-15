using System.Collections.Generic;

public static class EnemyRegistry
{
    static readonly List<EnemyBase> activeEnemies = new();

    public static IReadOnlyList<EnemyBase> ActiveEnemies => activeEnemies;

    public static void Register(EnemyBase enemy)
    {
        if (enemy == null || activeEnemies.Contains(enemy)) return;
        activeEnemies.Add(enemy);
    }

    public static void Unregister(EnemyBase enemy)
    {
        if (enemy == null) return;
        activeEnemies.Remove(enemy);
    }
}
