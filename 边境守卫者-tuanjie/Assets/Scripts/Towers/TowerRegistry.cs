using System.Collections.Generic;

public static class TowerRegistry
{
    static readonly List<TowerBase> activeTowers = new();

    public static IReadOnlyList<TowerBase> ActiveTowers => activeTowers;

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
