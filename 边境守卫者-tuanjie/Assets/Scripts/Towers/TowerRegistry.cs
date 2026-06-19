using System.Collections.Generic;

public static class TowerRegistry
{
    static readonly List<TowerBase> activeTowers = new();
    static readonly List<TowerBase> snapshotBufferA = new();
    static readonly List<TowerBase> snapshotBufferB = new();
    static int snapshotVersion;

    public static IReadOnlyList<TowerBase> ActiveTowers => activeTowers;

    public static IReadOnlyList<TowerBase> ActiveTowersSnapshot =>
        RegistrySnapshot.Copy(activeTowers, snapshotBufferA, snapshotBufferB, ref snapshotVersion);

    public static void Register(TowerBase tower)
    {
        if (tower == null || activeTowers.Contains(tower))
            return;

        activeTowers.Add(tower);
    }

    public static void Unregister(TowerBase tower)
    {
        if (tower == null)
            return;

        activeTowers.Remove(tower);
    }
}
