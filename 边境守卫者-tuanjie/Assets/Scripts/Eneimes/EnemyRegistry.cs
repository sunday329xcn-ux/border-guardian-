using System.Collections.Generic;

public static class EnemyRegistry
{
    static readonly List<EnemyBase> activeEnemies = new();
    static readonly List<EnemyBase> snapshotBufferA = new();
    static readonly List<EnemyBase> snapshotBufferB = new();
    static int snapshotVersion;

    public static IReadOnlyList<EnemyBase> ActiveEnemies => activeEnemies;

    public static IReadOnlyList<EnemyBase> ActiveEnemiesSnapshot =>
        RegistrySnapshot.Copy(activeEnemies, snapshotBufferA, snapshotBufferB, ref snapshotVersion);

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
