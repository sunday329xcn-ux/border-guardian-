using System.Collections.Generic;
using UnityEngine;

public readonly struct PlatformTerrainMarkerSpec
{
    public PlatformTerrainMarkerSpec(Color color, Vector3 localScale, float rotationZ, Vector3 localOffset)
    {
        Color = color;
        LocalScale = localScale;
        RotationZ = rotationZ;
        LocalOffset = localOffset;
    }

    public Color Color { get; }
    public Vector3 LocalScale { get; }
    public float RotationZ { get; }
    public Vector3 LocalOffset { get; }
}

public static class PlatformTerrainCatalog
{
    public static bool IsSpecial(PlatformTerrainType terrain)
    {
        return terrain != PlatformTerrainType.Standard;
    }

    public static Color GetPlatformTint(PlatformTerrainType terrain)
    {
        return terrain switch
        {
            PlatformTerrainType.Highland => new Color(0.62f, 0.68f, 0.78f),
            PlatformTerrainType.RuneAttackSpeed => new Color(0.58f, 0.52f, 0.72f),
            PlatformTerrainType.RuneRange => new Color(0.52f, 0.62f, 0.58f),
            PlatformTerrainType.RuneSynergy => new Color(0.52f, 0.56f, 0.74f),
            PlatformTerrainType.RestrictedMagicOnly => new Color(0.66f, 0.48f, 0.58f),
            PlatformTerrainType.RestrictedNoBarracks => new Color(0.68f, 0.52f, 0.38f),
            PlatformTerrainType.Fragile => new Color(0.68f, 0.42f, 0.46f),
            _ => new Color(0.55f, 0.58f, 0.62f)
        };
    }

    public static bool TryGetMarkerSpec(PlatformTerrainType terrain, out PlatformTerrainMarkerSpec spec)
    {
        switch (terrain)
        {
            case PlatformTerrainType.Highland:
                spec = new PlatformTerrainMarkerSpec(
                    new Color(0.92f, 0.82f, 0.35f, 0.95f),
                    new Vector3(0.42f, 0.18f, 1f),
                    0f,
                    new Vector3(0f, 0.18f, 0f));
                return true;
            case PlatformTerrainType.RuneAttackSpeed:
                spec = new PlatformTerrainMarkerSpec(
                    new Color(0.35f, 0.92f, 0.95f, 0.95f),
                    new Vector3(0.24f, 0.24f, 1f),
                    45f,
                    Vector3.zero);
                return true;
            case PlatformTerrainType.RuneRange:
                spec = new PlatformTerrainMarkerSpec(
                    new Color(0.45f, 0.92f, 0.55f, 0.95f),
                    new Vector3(0.46f, 0.16f, 1f),
                    0f,
                    Vector3.zero);
                return true;
            case PlatformTerrainType.RuneSynergy:
                spec = new PlatformTerrainMarkerSpec(
                    new Color(0.55f, 0.72f, 1f, 0.95f),
                    new Vector3(0.28f, 0.28f, 1f),
                    0f,
                    Vector3.zero);
                return true;
            case PlatformTerrainType.RestrictedMagicOnly:
                spec = new PlatformTerrainMarkerSpec(
                    new Color(0.88f, 0.45f, 0.95f, 0.9f),
                    new Vector3(0.52f, 0.52f, 1f),
                    0f,
                    Vector3.zero);
                return true;
            case PlatformTerrainType.RestrictedNoBarracks:
                spec = new PlatformTerrainMarkerSpec(
                    new Color(0.95f, 0.62f, 0.25f, 0.95f),
                    new Vector3(0.16f, 0.52f, 1f),
                    45f,
                    Vector3.zero);
                return true;
            case PlatformTerrainType.Fragile:
                spec = new PlatformTerrainMarkerSpec(
                    new Color(0.95f, 0.35f, 0.35f, 0.9f),
                    new Vector3(0.34f, 0.34f, 1f),
                    0f,
                    Vector3.zero);
                return true;
            default:
                spec = default;
                return false;
        }
    }

    public static string GetDisplayName(PlatformTerrainType terrain)
    {
        return terrain switch
        {
            PlatformTerrainType.Highland => "Highland Platform",
            PlatformTerrainType.RuneAttackSpeed => "Rune Platform · Haste",
            PlatformTerrainType.RuneRange => "Rune Platform · Reach",
            PlatformTerrainType.RuneSynergy => "Rune Platform · Link",
            PlatformTerrainType.RestrictedMagicOnly => "Restricted Platform · Arcane Only",
            PlatformTerrainType.RestrictedNoBarracks => "Restricted Platform · No Barracks",
            PlatformTerrainType.Fragile => "Fragile Platform",
            _ => "Standard Platform"
        };
    }

    public static string GetSubtitle(PlatformTerrainType terrain)
    {
        return terrain switch
        {
            PlatformTerrainType.Highland => "Special · +10% range",
            PlatformTerrainType.RuneAttackSpeed => "Special · +15% attack speed",
            PlatformTerrainType.RuneRange => "Special · +10% range",
            PlatformTerrainType.RuneSynergy => "Special · +15% synergy radius",
            PlatformTerrainType.RestrictedMagicOnly => "Special · Arcane towers only",
            PlatformTerrainType.RestrictedNoBarracks => "Special · Barracks blocked",
            PlatformTerrainType.Fragile => "Special · +15% damage, high risk",
            _ => "Normal build tile"
        };
    }

    public static string GetInfoBody(PlatformTerrainType terrain)
    {
        return terrain switch
        {
            PlatformTerrainType.Highland =>
                "Elevated edge tile.\n\n" +
                "Bonus: +10% attack range (Rally range for Barracks).\n" +
                "Best for: Arrow, Frost, and long-range anchors.",
            PlatformTerrainType.RuneAttackSpeed =>
                "Ancient rune of haste.\n\n" +
                "Bonus: +15% attack speed after building.\n" +
                "Best for: sustained DPS towers.",
            PlatformTerrainType.RuneRange =>
                "Ancient rune of reach.\n\n" +
                "Bonus: +10% attack range after building.\n" +
                "Best for: covering fork lanes.",
            PlatformTerrainType.RuneSynergy =>
                "Ancient rune of link.\n\n" +
                "Bonus: +15% synergy radius (Lv.3+).\n" +
                "Best for: combo clusters.",
            PlatformTerrainType.RestrictedMagicOnly =>
                "Arcane-only sanctum.\n\n" +
                "Rule: only Arcane towers may be built here.\n" +
                "Other tower types are blocked.",
            PlatformTerrainType.RestrictedNoBarracks =>
                "Narrow choke platform.\n\n" +
                "Rule: Barracks cannot be built here.\n" +
                "All other tower types are allowed.",
            PlatformTerrainType.Fragile =>
                "Cracked platform near the path.\n\n" +
                "Bonus: +15% tower damage.\n" +
                "Risk: future sapper-type enemies will prioritize this tile.",
            _ =>
                "Standard build platform.\n\n" +
                "No terrain bonus or restriction.\n" +
                "Select a tower from the build bar to place here."
        };
    }

    public static string GetHoverSummary(PlatformTerrainType terrain)
    {
        return terrain switch
        {
            PlatformTerrainType.Highland => "+10% attack range after build.",
            PlatformTerrainType.RuneAttackSpeed => "+15% attack speed after build.",
            PlatformTerrainType.RuneRange => "+10% attack range after build.",
            PlatformTerrainType.RuneSynergy => "+15% synergy radius after build.",
            PlatformTerrainType.RestrictedMagicOnly => "Arcane towers only.",
            PlatformTerrainType.RestrictedNoBarracks => "Barracks not allowed.",
            PlatformTerrainType.Fragile => "+15% damage · sapper priority (planned).",
            _ => "No bonus or restriction."
        };
    }

    public static bool CanBuild(PlatformTerrainType terrain, TowerType towerType, out string reason)
    {
        reason = null;

        switch (terrain)
        {
            case PlatformTerrainType.RestrictedMagicOnly when towerType != TowerType.Arcane:
                reason = "Only Arcane towers can be built on this platform.";
                return false;
            case PlatformTerrainType.RestrictedNoBarracks when towerType == TowerType.Barracks:
                reason = "Barracks cannot be built on this platform.";
                return false;
            default:
                return true;
        }
    }

    public static void ApplyTerrainBonuses(TowerBase tower, PlatformTerrainType terrain)
    {
        if (tower == null || terrain == PlatformTerrainType.Standard)
            return;

        if (tower is BarracksTower barracks)
        {
            barracks.ApplyTerrainModifiers(terrain);
            return;
        }

        tower.ApplyTerrainModifiers(terrain);
    }

    public static IReadOnlyList<(Vector2Int Cell, PlatformTerrainType Terrain)> GetGrimmForestAssignments()
    {
        return GrimmForestTerrainAssignments;
    }

    static readonly (Vector2Int Cell, PlatformTerrainType Terrain)[] GrimmForestTerrainAssignments =
    {
        (new Vector2Int(2, 13), PlatformTerrainType.Highland),
        (new Vector2Int(4, 13), PlatformTerrainType.Highland),
        (new Vector2Int(6, 13), PlatformTerrainType.Highland),
        (new Vector2Int(18, 8), PlatformTerrainType.Highland),
        (new Vector2Int(18, 6), PlatformTerrainType.Highland),
        (new Vector2Int(16, 9), PlatformTerrainType.RuneAttackSpeed),
        (new Vector2Int(12, 8), PlatformTerrainType.RuneRange),
        (new Vector2Int(15, 10), PlatformTerrainType.RuneSynergy),
        (new Vector2Int(14, 8), PlatformTerrainType.RestrictedMagicOnly),
        (new Vector2Int(7, 8), PlatformTerrainType.RestrictedNoBarracks),
        (new Vector2Int(11, 3), PlatformTerrainType.Fragile),
        (new Vector2Int(11, 4), PlatformTerrainType.Fragile)
    };
}
