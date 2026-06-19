using System.Collections.Generic;
using System.Text;

public readonly struct CodexEntry
{
    public CodexEntry(string id, string title, string subtitle, string body)
    {
        Id = id;
        Title = title;
        Subtitle = subtitle;
        Body = body;
    }

    public string Id { get; }
    public string Title { get; }
    public string Subtitle { get; }
    public string Body { get; }
}

public static class GameCodexCatalog
{
    const string TowerHpLine = "Tower HP: 50–250 (Lv × 50, refills on upgrade)";

    public static IReadOnlyList<CodexEntry> GetTowerEntries()
    {
        return new[]
        {
            BuildArrowEntry(),
            BuildFrostEntry(),
            BuildCannonEntry(),
            BuildArcaneEntry(),
            BuildBarracksEntry(),
            BuildMineEntry()
        };
    }

    public static IReadOnlyList<CodexEntry> GetEnemyEntries()
    {
        return new[]
        {
            BuildEnemyEntry(EnemyType.Imp),
            BuildEnemyEntry(EnemyType.Orc),
            BuildEnemyEntry(EnemyType.GoblinRipper),
            BuildEnemyEntry(EnemyType.Wraith),
            BuildEnemyEntry(EnemyType.RockGolem),
            BuildEnemyEntry(EnemyType.FireBomber),
            BuildEnemyEntry(EnemyType.ShadowPriest),
            BuildEnemyEntry(EnemyType.WolfRider),
            BuildEnemyEntry(EnemyType.TowerBreaker),
            BuildEnemyEntry(EnemyType.AncientDragon)
        };
    }

    public static IReadOnlyList<CodexEntry> GetSynergyEntries()
    {
        var entries = new List<CodexEntry>
        {
            new CodexEntry(
                "synergy_overview",
                "Synergy Guide",
                "How tower pairs work",
                TowerSynergyCatalog.BuildCodexOverviewBody())
        };

        foreach (var rule in TowerSynergyCatalog.RulesList)
        {
            var title = char.ToUpperInvariant(rule.Id[0]) + rule.Id.Substring(1);
            entries.Add(new CodexEntry(
                $"synergy_{rule.Id}",
                title,
                TowerSynergyCatalog.GetCodexSubtitle(rule),
                TowerSynergyCatalog.GetCodexBody(rule)));
        }

        return entries;
    }

    static CodexEntry BuildArrowEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Tag: Marksman");
        body.AppendLine(TowerHpLine);
        body.AppendLine($"Build: {TowerBuildCatalog.GetBuildCost(TowerType.Arrow)}g  ·  Synergy: Volley (+25% vs slowed)");
        body.AppendLine();
        body.AppendLine("Lv.1 — 5–8 dmg, 0.6s, range 3.5, hits air");
        body.AppendLine("Lv.2 — 80g → 10–14 dmg, 0.4s");
        body.AppendLine("Lv.3 — 120g → 15–20 dmg, 0.35s, 10% crit ×2");
        body.AppendLine("Lv.4 — 5 dia → 18–24 dmg, 0.3s, range 4.5, 20% crit");
        body.AppendLine("Lv.5A Ranger — 10 dia → 24–32 dmg, 0.25s, focus low HP");
        body.AppendLine("Lv.5B Siege — 10 dia → 38–52 dmg, 1.2s, range 6, line pierce, no air");

        return new CodexEntry("arrow", "Arrow Tower", "Marksman · single-target DPS", body.ToString());
    }

    static CodexEntry BuildFrostEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Tag: Ice");
        body.AppendLine(TowerHpLine);
        body.AppendLine($"Build: {TowerBuildCatalog.GetBuildCost(TowerType.Frost)}g  ·  Synergy: Shatter (+ Cannon vs slowed)");
        body.AppendLine();
        body.AppendLine("Lv.1 — 2–4 magic, 1.2s, range 2.5, slow 40%");
        body.AppendLine("Lv.2 — 100g → 4–7 dmg, slow 60%, 15% freeze 1s");
        body.AppendLine("Lv.3 — 150g → 7–10 dmg, slow 65%, 20% freeze 1.5s");
        body.AppendLine("Lv.4 — 5 dia → 10–14 dmg, frost ground + stronger freeze");
        body.AppendLine("Lv.5A Permafrost — 10 dia → 12–16 dmg, larger frost zone 3s");
        body.AppendLine("Lv.5B Blizzard — 10 dia → 18–26 AoE splash, guaranteed freeze 1.5s");

        return new CodexEntry("frost", "Frost Tower", "Ice · slow & control", body.ToString());
    }

    static CodexEntry BuildCannonEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Tag: Explosive");
        body.AppendLine(TowerHpLine);
        body.AppendLine($"Build: {TowerBuildCatalog.GetBuildCost(TowerType.Cannon)}g  ·  Synergy: Shatter / Breach");
        body.AppendLine();
        body.AppendLine("Lv.1 — 11–17 dmg, 2.0s, splash ×3, no air");
        body.AppendLine("Lv.2 — 120g → 18–28 dmg, 0.3s stun");
        body.AppendLine("Lv.3 — 180g → 24–36 dmg, wider splash, 0.5s stun");
        body.AppendLine("Lv.4 — 6 dia → 28–40 dmg, fire ground 6 DPS");
        body.AppendLine("Lv.5A Incendiary — 12 dia → 34–48 dmg, stronger fire ground");
        body.AppendLine("Lv.5B Thunder — 12 dia → 48–68 dmg, range 5.5, focus high HP, −8 armor");

        return new CodexEntry("cannon", "Cannon Tower", "Explosive · AoE burst", body.ToString());
    }

    static CodexEntry BuildArcaneEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Tag: Arcane");
        body.AppendLine(TowerHpLine);
        body.AppendLine($"Build: {TowerBuildCatalog.GetBuildCost(TowerType.Arcane)}g  ·  Synergy: Breach / Ward");
        body.AppendLine();
        body.AppendLine("Lv.1 — 10–14 magic, 0.9s, 20% armor pen, hits air");
        body.AppendLine("Lv.2 — 110g → 16–22 dmg, 35% armor pen");
        body.AppendLine("Lv.3 — 150g → 22–30 dmg, 45% armor pen");
        body.AppendLine("Lv.4 — 5 dia → 26–34 dmg, steal 2 armor per hit");
        body.AppendLine("Lv.5A Destroyer — 10 dia → 30–40 dmg, steal armor & buff allies +2 dmg");
        body.AppendLine("Lv.5B Void Rift — 10 dia → 15–22 dmg + void pulse every 8s");

        return new CodexEntry("arcane", "Arcane Tower", "Arcane · armor shred", body.ToString());
    }

    static CodexEntry BuildBarracksEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Tag: Guardian  ·  Rally supported");
        body.AppendLine(TowerHpLine);
        body.AppendLine($"Build: {TowerBuildCatalog.GetBuildCost(TowerType.Barracks)}g  ·  Synergy: Ward (+ Arcane armor & dmg)");
        body.AppendLine();
        body.AppendLine("Lv.1 — 2 soldiers, 80 HP, 2–4 dmg, rally 1.8, respawn 8s");
        body.AppendLine("Lv.2 — 90g → 3 soldiers, 140 HP, death burst 10");
        body.AppendLine("Lv.3 — 130g → 200 HP, 9–12 dmg, rally 2.8, burst 20");
        body.AppendLine("Lv.4 — 5 dia → 280 HP, 14–18 dmg, burst 30");
        body.AppendLine("Lv.5A Paladin — 12 dia → 2 knights 400 HP, 20–26 dmg, rally 4.0");
        body.AppendLine("Lv.5B Assassin — 10 dia → 4 assassins 150 HP, 28–36 dmg, rally 4.0");

        return new CodexEntry("barracks", "Barracks", "Guardian · melee block", body.ToString());
    }

    static CodexEntry BuildMineEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Tag: —");
        body.AppendLine(TowerHpLine);
        body.AppendLine($"Build: {TowerBuildCatalog.GetBuildCost(TowerType.DiamondMine)}g  ·  Cannot upgrade");
        body.AppendLine();
        body.AppendLine("Produces 0.1 diamonds per second while alive.");
        body.AppendLine("Occupies one build platform for the whole mission.");

        return new CodexEntry("mine", "Diamond Mine", "Economy · passive diamonds", body.ToString());
    }

    static CodexEntry BuildEnemyEntry(EnemyType type)
    {
        var stats = EnemyCatalog.Get(type);
        var name = EnemyCatalog.GetDisplayName(type);
        var subtitle = BuildEnemySubtitle(stats);
        var body = new StringBuilder();

        body.AppendLine($"HP {stats.MaxHealth}  ·  Armor {stats.Armor}  ·  MR {stats.MagicResist}");
        body.AppendLine($"Speed {stats.MoveSpeed:0.#}  ·  {(stats.IsFlying ? "Flying" : "Ground")}");
        body.AppendLine(BuildRewardLine(stats));
        body.AppendLine(BuildLeakLine(stats));
        body.AppendLine();
        body.AppendLine(GetEnemyAbility(type));

        return new CodexEntry(type.ToString().ToLowerInvariant(), name, subtitle, body.ToString());
    }

    static string BuildEnemySubtitle(EnemyStats stats)
    {
        if (stats.IsBoss)
            return "BOSS";

        if (stats.IsElite)
            return "Elite";

        if (stats.IsFlying)
            return "Flying";

        return "Standard";
    }

    static string BuildRewardLine(EnemyStats stats)
    {
        if (stats.DiamondReward > 0)
            return $"Reward: {stats.GoldReward}g + {stats.DiamondReward} dia";

        return $"Reward: {stats.GoldReward}g";
    }

    static string BuildLeakLine(EnemyStats stats)
    {
        if (stats.StealGoldOnLeak > 0)
            return $"Leak: steals {stats.StealGoldOnLeak} gold (no life loss)";

        if (stats.IsBoss)
            return $"Leak: -{stats.LeakDamage} lives (BOSS)";

        if (stats.IsElite)
            return $"Leak: -{stats.LeakDamage} lives (Elite)";

        return $"Leak: -{stats.LeakDamage} life";
    }

    static string GetEnemyAbility(EnemyType type)
    {
        return type switch
        {
            EnemyType.Imp => "Ability: none.",
            EnemyType.Orc => "Ability: heavy armor frontliner.",
            EnemyType.GoblinRipper => "Ability: very fast; steals gold if it leaks.",
            EnemyType.Wraith => "Ability: flying; on death heals nearby allies 30 HP.",
            EnemyType.RockGolem => "Ability: every 10s, invulnerable 2s and heals 50 HP.",
            EnemyType.FireBomber => "Ability: death explosion 60 true dmg (enemies, soldiers, towers).",
            EnemyType.ShadowPriest => "Ability: heals forward allies 15 HP/s, prefers elites.",
            EnemyType.WolfRider => "Ability: spawns an Imp every 15s.",
            EnemyType.TowerBreaker => "Ability: ignores soldiers; destroys nearest tower 80 dmg / 2s.",
            EnemyType.AncientDragon => "Ability: BOSS — phase 1 flying fireballs; phase 2 lands (+30 armor, flame path).",
            _ => string.Empty
        };
    }
}
