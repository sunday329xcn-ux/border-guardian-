using System.Collections.Generic;
using System.Text;

public readonly struct TowerSynergyRule
{
    public TowerSynergyRule(
        string id,
        TowerType partnerA,
        TowerType partnerB,
        float range,
        string summary)
    {
        Id = id;
        PartnerA = partnerA;
        PartnerB = partnerB;
        Range = range;
        Summary = summary;
    }

    public string Id { get; }
    public TowerType PartnerA { get; }
    public TowerType PartnerB { get; }
    public float Range { get; }
    public string Summary { get; }
}

public static class TowerSynergyCatalog
{
    public const float DefaultRange = 2.8f;

    static readonly TowerSynergyRule[] Rules =
    {
        new TowerSynergyRule(
            "shatter",
            TowerType.Frost,
            TowerType.Cannon,
            DefaultRange,
            "Shatter: Cannon +30% vs slowed foes"),
        new TowerSynergyRule(
            "volley",
            TowerType.Frost,
            TowerType.Arrow,
            DefaultRange,
            "Volley: Arrow +25% vs slowed foes"),
        new TowerSynergyRule(
            "ward",
            TowerType.Arcane,
            TowerType.Barracks,
            DefaultRange,
            "Ward: Arcane +4 dmg per Barracks; soldiers +4 armor per Arcane"),
        new TowerSynergyRule(
            "breach",
            TowerType.Cannon,
            TowerType.Arcane,
            DefaultRange,
            "Breach: Cannon shreds MR; Arcane +15% vs debuffed foes")
    };

    public static IReadOnlyList<TowerSynergyRule> RulesList => Rules;

    public static TowerElementTag GetTag(TowerType type)
    {
        return type switch
        {
            TowerType.Arrow => TowerElementTag.Marksman,
            TowerType.Frost => TowerElementTag.Ice,
            TowerType.Cannon => TowerElementTag.Explosive,
            TowerType.Arcane => TowerElementTag.Arcane,
            TowerType.Barracks => TowerElementTag.Guardian,
            _ => TowerElementTag.None
        };
    }

    public static string GetTagName(TowerType type)
    {
        return GetTag(type) switch
        {
            TowerElementTag.Marksman => "Marksman",
            TowerElementTag.Ice => "Ice",
            TowerElementTag.Explosive => "Explosive",
            TowerElementTag.Arcane => "Arcane",
            TowerElementTag.Guardian => "Guardian",
            _ => string.Empty
        };
    }

    public static bool IsCombatTower(TowerType type)
    {
        return GetTag(type) != TowerElementTag.None;
    }

    public static string GetCodexSubtitle(TowerSynergyRule rule)
    {
        var partnerA = TowerBuildCatalog.GetDisplayName(rule.PartnerA);
        var partnerB = TowerBuildCatalog.GetDisplayName(rule.PartnerB);
        return $"{partnerA} + {partnerB}  ·  range {rule.Range:0.#}";
    }

    public static string GetCodexBody(TowerSynergyRule rule)
    {
        return rule.Id switch
        {
            "volley" => BuildVolleyCodexBody(),
            "shatter" => BuildShatterCodexBody(),
            "ward" => BuildWardCodexBody(),
            "breach" => BuildBreachCodexBody(),
            _ => rule.Summary
        };
    }

    static string BuildVolleyCodexBody()
    {
        var body = new StringBuilder();
        body.AppendLine("Partners: Frost + Arrow");
        body.AppendLine($"Activation: within {DefaultRange:0.#} tiles on build platforms.");
        body.AppendLine();
        body.AppendLine("Effect:");
        body.AppendLine("· Arrow Tower deals +25% damage to slowed enemies.");
        body.AppendLine("· Requires a Frost Tower nearby to apply slow.");
        body.AppendLine();
        body.AppendLine("Strategy:");
        body.AppendLine("· Place Frost first on a choke, then Arrow behind or beside it.");
        body.AppendLine("· Strong vs fast units (Goblin Ripper, Wolf Rider) once Frost lands slow.");
        body.AppendLine("· Frost Permafrost / Blizzard branches widen the slow window for Arrow DPS.");
        return body.ToString().TrimEnd();
    }

    static string BuildShatterCodexBody()
    {
        var body = new StringBuilder();
        body.AppendLine("Partners: Frost + Cannon");
        body.AppendLine($"Activation: within {DefaultRange:0.#} tiles on build platforms.");
        body.AppendLine();
        body.AppendLine("Effect:");
        body.AppendLine("· Cannon Tower deals +30% damage to slowed enemies.");
        body.AppendLine("· Splash still hits groups; bonus applies per hit on slowed targets.");
        body.AppendLine();
        body.AppendLine("Strategy:");
        body.AppendLine("· Best on merged paths where Frost can slow a clump before Cannon fires.");
        body.AppendLine("· Pairs well with Frost freeze proc — Cannon cannot hit air, so add Arrow/Arcane for flyers.");
        body.AppendLine("· Incendiary / Thunder Cannon branches add burn or armor shred on top of Shatter.");
        return body.ToString().TrimEnd();
    }

    static string BuildWardCodexBody()
    {
        var body = new StringBuilder();
        body.AppendLine("Partners: Arcane + Barracks");
        body.AppendLine($"Activation: within {DefaultRange:0.#} tiles on build platforms.");
        body.AppendLine();
        body.AppendLine("Effect:");
        body.AppendLine("· Each nearby Barracks adds +4 flat damage to Arcane Tower attacks.");
        body.AppendLine("· Each nearby Arcane Tower adds +4 armor to Barracks soldiers.");
        body.AppendLine("· Multiple partners stack (2 Barracks → +8 Arcane dmg).");
        body.AppendLine();
        body.AppendLine("Strategy:");
        body.AppendLine("· Cluster Arcane beside Barracks on a lane block point for both DPS and tankier soldiers.");
        body.AppendLine("· Helps soldiers survive Fire Bomber death blasts and Tower Breaker pressure.");
        body.AppendLine("· Arcane Destroyer branch still steals armor — Ward adds steady frontline bulk.");
        return body.ToString().TrimEnd();
    }

    static string BuildBreachCodexBody()
    {
        var body = new StringBuilder();
        body.AppendLine("Partners: Cannon + Arcane");
        body.AppendLine($"Activation: within {DefaultRange:0.#} tiles on build platforms.");
        body.AppendLine();
        body.AppendLine("Effect:");
        body.AppendLine("· Cannon hits shred −10 MR for 4 seconds on the target.");
        body.AppendLine("· Arcane Tower deals +15% damage to enemies under MR debuffs (including Breach shred).");
        body.AppendLine("· Both towers benefit when paired — Cannon sets up, Arcane exploits.");
        body.AppendLine();
        body.AppendLine("Strategy:");
        body.AppendLine("· Core combo vs high-MR waves (Wraith, Shadow Priest, Ancient Dragon phase 1).");
        body.AppendLine("· Cannon stun branches buy time for MR shred to stack on elites.");
        body.AppendLine("· Arcane armor pen and Breach bonus stack — place both on the same fork.");
        return body.ToString().TrimEnd();
    }

    public static string BuildCodexOverviewBody()
    {
        var body = new StringBuilder();
        body.AppendLine("Combat towers carry an element Tag. Pair the right Tags within range to unlock bonus effects.");
        body.AppendLine();
        body.AppendLine($"Range: {DefaultRange:0.#} tiles between tower centers (build platforms only).");
        body.AppendLine("Mine does not participate. Barracks counts as Guardian.");
        body.AppendLine();
        body.AppendLine("In battle:");
        body.AppendLine("· Select any combat tower — the info panel shows Tag and active synergies.");
        body.AppendLine("· \"Synergy: none nearby\" means move partners closer or build missing types.");
        body.AppendLine();
        body.AppendLine("Tag reference:");
        body.AppendLine("· Marksman — Arrow");
        body.AppendLine("· Ice — Frost");
        body.AppendLine("· Explosive — Cannon");
        body.AppendLine("· Arcane — Arcane");
        body.AppendLine("· Guardian — Barracks");
        body.AppendLine();
        body.AppendLine("All synergies (see list for details):");
        foreach (var rule in Rules)
        {
            body.Append("· ");
            body.AppendLine(rule.Summary);
        }

        body.AppendLine();
        body.AppendLine("Planning tips:");
        body.AppendLine("· Frost is the hub — it enables Volley (Arrow) and Shatter (Cannon).");
        body.AppendLine("· Breach solves MR-heavy waves; Ward stabilizes melee blocks.");
        body.AppendLine("· On forked lanes, duplicate mini-clusters rather than one distant pair.");
        return body.ToString().TrimEnd();
    }
}
