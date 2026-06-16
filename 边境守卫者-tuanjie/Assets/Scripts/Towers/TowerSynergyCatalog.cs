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
}
