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
            BuildMineEntry(),
            BuildSpotterEntry(),
            BuildBeaconEntry(),
            BuildBountyShrineEntry()
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
            BuildEnemyEntry(EnemyType.AncientDragon),
            BuildEnemyEntry(EnemyType.ShieldBearer),
            BuildEnemyEntry(EnemyType.SplitSlime),
            BuildEnemyEntry(EnemyType.Shade),
            BuildEnemyEntry(EnemyType.WarDrummer),
            BuildEnemyEntry(EnemyType.Nullifier),
            BuildEnemyEntry(EnemyType.BatSwarm)
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

    public static IReadOnlyList<CodexEntry> GetMapEntries()
    {
        return new[]
        {
            BuildMapOverviewEntry(),
            BuildPlatformTerrainEntry(),
            BuildForkGateEntry(),
            BuildTemporaryBlockEntry()
        };
    }

    static CodexEntry BuildPlatformTerrainEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Build platforms use color + marker shape to show terrain type.");
        body.AppendLine("Hover an empty platform to see terrain info before building.");
        body.AppendLine();
        body.AppendLine("Types on Grimm Forest:");
        body.AppendLine("· Standard — gray tile, no marker.");
        body.AppendLine("· Highland — blue-gray + gold bar · +10% range.");
        body.AppendLine("· Rune Haste — purple + cyan diamond · +15% attack speed.");
        body.AppendLine("· Rune Reach — green tint + bar · +10% range.");
        body.AppendLine("· Rune Link — blue tint + circle · +15% synergy radius.");
        body.AppendLine("· Arcane Only — rose tile + ring · Arcane towers only.");
        body.AppendLine("· No Barracks — orange tile + slash · Barracks blocked.");
        body.AppendLine("· Fragile — red tint + inner square · +15% damage, high risk.");

        return new CodexEntry(
            "map_platform_terrain",
            "Platform Terrain",
            "Standard · special tiles",
            body.ToString());
    }

    static CodexEntry BuildMapOverviewEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Grimm Forest (20×14) uses dual spawns, a fork junction, and scenic detours.");
        body.AppendLine();
        body.AppendLine("Route flow:");
        body.AppendLine("· Upper spawn (0,12) — longer northern path.");
        body.AppendLine("· Lower spawn (0,1) — shorter southern choke.");
        body.AppendLine("· Both merge at fork gate (10,7), then rejoin at (17,7) → goal (19,7).");
        body.AppendLine();
        body.AppendLine("Player tools at the fork:");
        body.AppendLine("· Fork Gate — choose Upper / Lower / None (central trunk).");
        body.AppendLine("· Temporary Block — seal a scenic bridge to force detour.");
        body.AppendLine();
        body.AppendLine("Interact: click the fork marker or orange block tile on the map to open the control popup.");

        return new CodexEntry(
            "map_overview",
            "Grimm Forest Routes",
            "Dual spawn · fork · scenic branches",
            body.ToString());
    }

    static CodexEntry BuildForkGateEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Location: fork junction (10,7).");
        body.AppendLine();
        body.AppendLine("Modes (click fork marker at junction):");
        body.AppendLine("· None (default) — faint cyan marker at junction; all enemies use central trunk.");
        body.AppendLine("· Upper — north scenic branch; green gate marker at junction.");
        body.AppendLine("· Lower — south scenic branch; gold gate marker at junction.");
        body.AppendLine();
        body.AppendLine("Rules:");
        body.AppendLine($"· Each switch to a different mode costs {MapRouteController.ForkSwitchGoldCost} gold.");
        body.AppendLine("· Re-selecting the current mode costs nothing.");
        body.AppendLine("· Takes effect immediately for newly spawned enemies (prep, spawn, or combat).");
        body.AppendLine();
        body.AppendLine("Tips:");
        body.AppendLine("· None when your central cluster has the DPS.");
        body.AppendLine("· Upper/Lower to drag waves through isolated platforms or overlapping fields.");

        return new CodexEntry(
            "map_fork_gate",
            "Fork Gate",
            "Upper · Lower · None",
            body.ToString());
    }

    static CodexEntry BuildTemporaryBlockEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Seal a scenic bridge to redirect that fork onto the central detour.");
        body.AppendLine();
        body.AppendLine("Block points (orange markers on map):");
        body.AppendLine("· North scenic — cell (13,9) on the upper fork primary path.");
        body.AppendLine("· South scenic — cell (13,5) on the lower fork primary path.");
        body.AppendLine();
        body.AppendLine("Cost & timing:");
        body.AppendLine($"· {MapRouteController.BlockGoldCost} gold per activation.");
        body.AppendLine($"· Block lasts {MapRouteController.BlockDurationSeconds:0.#} seconds.");
        body.AppendLine($"· Cooldown {MapRouteController.BlockCooldownSeconds:0.#}s before the next block.");
        body.AppendLine();
        body.AppendLine("Effect:");
        body.AppendLine("· While active, enemies on that scenic branch use the central detour path.");
        body.AppendLine("· Only affects enemies spawned after the block starts.");
        body.AppendLine("· Works best with Fork Gate set to Upper or Lower on that side.");
        body.AppendLine();
        body.AppendLine("Click the orange block tile on the map to open the block popup.");

        return new CodexEntry(
            "map_temp_block",
            "Temporary Block",
            "Scenic bridge seal · forced detour",
            body.ToString());
    }

    static CodexEntry BuildArrowEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Tag: Marksman");
        body.AppendLine(TowerHpLine);
        body.AppendLine($"Build: {TowerBuildCatalog.GetBuildCost(TowerType.Arrow)}g  ·  Synergy: Volley (+18% vs slowed)");
        body.AppendLine("Attack range ×0.8; synergy ring 2.8+ tiles from Lv.3 (not scaled). Lv.5 branches shift synergy.");
        body.AppendLine();
        body.AppendLine("Lv.1 — 5–8 dmg, 0.6s, range ~2.6, hits air");
        body.AppendLine("Lv.2 — 80g → 10–14 dmg, 0.4s");
        body.AppendLine("Lv.3 — 120g → 14–19 dmg, 0.35s, 10% crit ×2");
        body.AppendLine("Lv.4 — 5 dia → 16–22 dmg, 0.32s, ~3.5 range, 18% crit");
        body.AppendLine("Lv.5A Ranger — 10 dia → 20–27 dmg, 0.28s, longer synergy, focus low HP, detects Shade");
        body.AppendLine("Lv.5B Siege — 10 dia → 30–42 dmg, 1.35s, long range, line pierce, no air, detects Shade");

        return new CodexEntry("arrow", "Arrow Tower", "Marksman · single-target DPS", body.ToString());
    }

    static CodexEntry BuildFrostEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Tag: Ice");
        body.AppendLine(TowerHpLine);
        body.AppendLine($"Build: {TowerBuildCatalog.GetBuildCost(TowerType.Frost)}g  ·  Synergy: Shatter (+ Cannon vs slowed)");
        body.AppendLine();
        body.AppendLine("Lv.1 — 2–4 magic, 1.2s, range ~2.5, slow 40%");
        body.AppendLine("Lv.2 — 100g → 4–7 dmg, slow 60%, 15% freeze 1s");
        body.AppendLine("Lv.3 — 150g → 7–10 dmg, slow 65%, 20% freeze 1.5s");
        body.AppendLine("Lv.4 — 5 dia → 9–13 dmg, frost ground + stronger freeze");
        body.AppendLine("Lv.5A Permafrost — 10 dia → 11–15 dmg, larger frost zone; synergy range up");
        body.AppendLine("Lv.5B Blizzard — 10 dia → 14–20 AoE splash; shorter attack & synergy range");

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
        body.AppendLine("Lv.2 — 120g → 17–26 dmg, 0.3s stun");
        body.AppendLine("Lv.3 — 180g → 22–34 dmg, wider splash, 0.5s stun");
        body.AppendLine("Lv.4 — 6 dia → 26–38 dmg, fire ground 5 DPS");
        body.AppendLine("Lv.5A Incendiary — 12 dia → 30–44 dmg, stronger fire ground");
        body.AppendLine("Lv.5B Thunder — 12 dia → 42–60 dmg, long range, focus high HP, −7 armor");

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
        body.AppendLine("Lv.2 — 110g → 15–21 dmg, 35% armor pen");
        body.AppendLine("Lv.3 — 150g → 20–28 dmg, 45% armor pen");
        body.AppendLine("Lv.4 — 5 dia → 24–32 dmg, steal 2 armor per hit");
        body.AppendLine("Lv.5A Destroyer — 10 dia → 27–36 dmg, steal armor & buff allies +2 dmg; shorter range");
        body.AppendLine("Lv.5B Void Rift — 10 dia → 14–20 dmg + void pulse every 8s; tighter synergy");

        return new CodexEntry("arcane", "Arcane Tower", "Arcane · armor shred", body.ToString());
    }

    static CodexEntry BuildBarracksEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Tag: Guardian  ·  Rally supported");
        body.AppendLine(TowerHpLine);
        body.AppendLine($"Build: {TowerBuildCatalog.GetBuildCost(TowerType.Barracks)}g  ·  Synergy: Ward (+ Arcane armor & dmg)");
        body.AppendLine();
        body.AppendLine("Lv.1 — 2 soldiers, 80 HP, 2–4 dmg, rally ~1.4, respawn 8s (no synergy until Lv.3)");
        body.AppendLine("Lv.2 — 90g → 3 soldiers, 140 HP, death burst 10");
        body.AppendLine("Lv.3 — 130g → 200 HP, 9–12 dmg, rally ~2.1, burst 20");
        body.AppendLine("Lv.4 — 5 dia → 280 HP, 14–18 dmg, burst 30");
        body.AppendLine("Lv.5A Paladin — 12 dia → 2 knights 400 HP, 20–26 dmg, rally ~3.1");
        body.AppendLine("Lv.5B Assassin — 10 dia → 4 assassins 150 HP, 28–36 dmg, rally ~2.9");

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

    static CodexEntry BuildSpotterEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Support · no combat output.");
        body.AppendLine(TowerHpLine);
        body.AppendLine($"Build: {TowerBuildCatalog.GetBuildCost(TowerType.Spotter)}g  ·  Cannot upgrade");
        body.AppendLine();
        body.AppendLine("Reveal radius: 3 tiles.");
        body.AppendLine("Reveals Shade enemies so all towers can target them.");
        body.AppendLine("Build before wave 11 stealth pressure — Arrow Lv.5 also detects Shade.");

        return new CodexEntry("spotter", "Spotter", "Support · stealth reveal", body.ToString());
    }

    static CodexEntry BuildBeaconEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Support · no combat output.");
        body.AppendLine(TowerHpLine);
        body.AppendLine($"Build: {TowerBuildCatalog.GetBuildCost(TowerType.Beacon)}g  ·  Cannot upgrade");
        body.AppendLine();
        body.AppendLine("Aura radius: 2.5 tiles.");
        body.AppendLine("Nearby combat towers gain +10% attack speed.");
        body.AppendLine("Neutral tag — does not participate in synergy links.");

        return new CodexEntry("beacon", "Beacon", "Support · attack speed aura", body.ToString());
    }

    static CodexEntry BuildBountyShrineEntry()
    {
        var body = new StringBuilder();
        body.AppendLine("Support · no combat output.");
        body.AppendLine(TowerHpLine);
        body.AppendLine($"Build: {TowerBuildCatalog.GetBuildCost(TowerType.BountyShrine)}g  ·  Cannot upgrade");
        body.AppendLine();
        body.AppendLine("Aura radius: 2.8 tiles.");
        body.AppendLine("Enemy kills inside the aura grant +15% gold.");
        body.AppendLine("Pairs well with AoE clusters on fork lanes.");

        return new CodexEntry("bounty", "Bounty Shrine", "Support · kill gold bonus", body.ToString());
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
            EnemyType.ShieldBearer => "Ability: blocks 70% frontal physical damage; sides and rear are vulnerable.",
            EnemyType.SplitSlime => "Ability: splits into 2 mini slimes (50% HP each) on death.",
            EnemyType.Shade => "Ability: stealthed until 72% path; Spotter reveals; Arrow Lv.5 detects.",
            EnemyType.WarDrummer => "Ability: +25% move speed aura (2.0 range) for nearby allies.",
            EnemyType.Nullifier => "Ability: elite — disables tower synergies within 2.8 range for 5s.",
            EnemyType.BatSwarm => "Ability: flying swarm; only anti-air towers can target it.",
            _ => string.Empty
        };
    }
}
