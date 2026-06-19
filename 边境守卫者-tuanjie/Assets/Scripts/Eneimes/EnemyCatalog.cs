using UnityEngine;

public readonly struct EnemyStats
{
    public EnemyStats(
        int maxHealth,
        int armor,
        int magicResist,
        bool isFlying,
        float moveSpeed,
        int goldReward,
        int diamondReward,
        int leakDamage,
        int stealGoldOnLeak,
        bool isElite,
        bool isBoss,
        bool ignoresBarracksBlock,
        Color color,
        float scale = 0.55f)
    {
        MaxHealth = maxHealth;
        Armor = armor;
        MagicResist = magicResist;
        IsFlying = isFlying;
        MoveSpeed = moveSpeed;
        GoldReward = goldReward;
        DiamondReward = diamondReward;
        LeakDamage = leakDamage;
        StealGoldOnLeak = stealGoldOnLeak;
        IsElite = isElite;
        IsBoss = isBoss;
        IgnoresBarracksBlock = ignoresBarracksBlock;
        Color = color;
        Scale = scale;
    }

    public int MaxHealth { get; }
    public int Armor { get; }
    public int MagicResist { get; }
    public bool IsFlying { get; }
    public float MoveSpeed { get; }
    public int GoldReward { get; }
    public int DiamondReward { get; }
    public int LeakDamage { get; }
    public int StealGoldOnLeak { get; }
    public bool IsElite { get; }
    public bool IsBoss { get; }
    public bool IgnoresBarracksBlock { get; }
    public Color Color { get; }
    public float Scale { get; }
}

public static class EnemyCatalog
{
    public static EnemyStats Get(EnemyType type)
    {
        return type switch
        {
            EnemyType.Imp => new EnemyStats(65, 0, 0, false, 2.5f, 1, 0, 1, 0, false, false, false, new Color(0.9f, 0.25f, 0.25f)),
            EnemyType.Orc => new EnemyStats(220, 10, 0, false, 1.8f, 3, 0, 1, 0, false, false, false, new Color(0.45f, 0.65f, 0.35f)),
            EnemyType.GoblinRipper => new EnemyStats(50, 0, 0, false, 3.4f, 1, 0, 0, 10, false, false, false, new Color(0.95f, 0.55f, 0.15f), 0.5f),
            EnemyType.Wraith => new EnemyStats(40, 0, 50, true, 3f, 2, 0, 1, 0, false, false, false, new Color(0.55f, 0.75f, 1f), 0.5f),
            EnemyType.RockGolem => new EnemyStats(1600, 32, 12, false, 1.2f, 15, 2, 5, 0, true, false, false, new Color(0.55f, 0.5f, 0.45f), 0.85f),
            EnemyType.FireBomber => new EnemyStats(100, 5, 30, false, 2.2f, 3, 0, 1, 0, false, false, false, new Color(1f, 0.35f, 0.1f)),
            EnemyType.ShadowPriest => new EnemyStats(180, 0, 20, false, 1.6f, 5, 0, 1, 0, false, false, false, new Color(0.45f, 0.2f, 0.55f)),
            EnemyType.WolfRider => new EnemyStats(150, 8, 0, false, 2.8f, 4, 0, 1, 0, false, false, false, new Color(0.6f, 0.45f, 0.25f)),
            EnemyType.TowerBreaker => new EnemyStats(950, 22, 12, false, 1.5f, 12, 1, 5, 0, true, false, true, new Color(0.35f, 0.35f, 0.4f), 0.8f),
            EnemyType.AncientDragon => new EnemyStats(4800, 18, 32, true, 1.6f, 50, 8, 10, 0, false, true, false, new Color(0.75f, 0.15f, 0.2f), 1.1f),
            _ => new EnemyStats(60, 0, 0, false, 2.5f, 1, 0, 1, 0, false, false, false, Color.red)
        };
    }

    public static string GetDisplayName(EnemyType type)
    {
        return type switch
        {
            EnemyType.Imp => "Imp",
            EnemyType.Orc => "Orc",
            EnemyType.GoblinRipper => "Ripper",
            EnemyType.Wraith => "Wraith",
            EnemyType.RockGolem => "Rock Golem",
            EnemyType.FireBomber => "Fire Bomber",
            EnemyType.ShadowPriest => "Shadow Priest",
            EnemyType.WolfRider => "Wolf Rider",
            EnemyType.TowerBreaker => "Tower Breaker",
            EnemyType.AncientDragon => "Ancient Dragon",
            _ => type.ToString()
        };
    }

    public static string GetThreatRating(
        EnemyType type,
        int leakDamage,
        int stealGoldOnLeak,
        bool isBoss,
        bool isElite)
    {
        if (isBoss)
            return "Very High (Boss)";

        if (type == EnemyType.TowerBreaker)
            return "Very High (Tower Break)";

        if (type == EnemyType.FireBomber)
            return "High (Explosion)";

        if (stealGoldOnLeak > 0)
            return "Medium (Gold Steal)";

        if (isElite)
            return "High (Elite)";

        if (leakDamage >= 5)
            return "High";

        return leakDamage > 1 ? "Medium" : "Low";
    }

    public static void AttachBehavior(EnemyBase enemy, EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Wraith:
                enemy.gameObject.AddComponent<WraithBehavior>();
                break;
            case EnemyType.RockGolem:
                enemy.gameObject.AddComponent<RockGolemBehavior>();
                break;
            case EnemyType.FireBomber:
                enemy.gameObject.AddComponent<FireBomberBehavior>();
                break;
            case EnemyType.ShadowPriest:
                enemy.gameObject.AddComponent<ShadowPriestBehavior>();
                break;
            case EnemyType.WolfRider:
                enemy.gameObject.AddComponent<WolfRiderBehavior>();
                break;
            case EnemyType.TowerBreaker:
                enemy.gameObject.AddComponent<TowerBreakerBehavior>();
                break;
            case EnemyType.AncientDragon:
                enemy.gameObject.AddComponent<AncientDragonBehavior>();
                break;
        }
    }
}
