using UnityEngine;

/// <summary>
/// Global colour palette for the zero-art visual pass (P-A0). Environment,
/// tower and semantic colours are centralised here so the procedural look can
/// be retuned in one place. Gameplay-driving colours (enemy DisplayColor in
/// <see cref="EnemyCatalog"/>) are intentionally left untouched.
/// </summary>
public static class VisualPalette
{
    // Environment
    public static readonly Color ForestDeep = new(0.12f, 0.16f, 0.12f);
    public static readonly Color GrassBase = new(0.22f, 0.34f, 0.22f);
    public static readonly Color GrassLight = new(0.28f, 0.48f, 0.28f);
    public static readonly Color PathBase = new(0.72f, 0.58f, 0.36f);
    public static readonly Color PathEdge = new(0.45f, 0.35f, 0.22f);
    public static readonly Color PlatformBase = new(0.42f, 0.45f, 0.48f);
    public static readonly Color Blocked = new(0.18f, 0.24f, 0.18f);

    // Tower primaries (mirror the colours used by the existing tower factories)
    public static readonly Color ArrowTower = new(0.82f, 0.72f, 0.28f);
    public static readonly Color FrostTower = new(0.45f, 0.78f, 0.95f);
    public static readonly Color CannonTower = new(0.55f, 0.55f, 0.60f);
    public static readonly Color ArcaneTower = new(0.62f, 0.35f, 0.85f);
    public static readonly Color BarracksTower = new(0.35f, 0.55f, 0.95f);
    public static readonly Color DiamondMineTower = new(0.45f, 0.85f, 0.95f);

    // Support towers
    public static readonly Color SpotterTower = new(0.85f, 0.85f, 0.55f);
    public static readonly Color BeaconTower = new(0.95f, 0.75f, 0.35f);
    public static readonly Color BountyShrineTower = new(0.95f, 0.85f, 0.45f);

    // Semantic
    public static readonly Color BuildValid = new(0.45f, 0.85f, 0.55f);
    public static readonly Color Selected = new(1f, 0.92f, 0.35f);
    public static readonly Color Synergy = new(0.55f, 0.75f, 1f);
    public static readonly Color EliteAccent = new(1f, 0.85f, 0.3f);
    public static readonly Color Danger = new(0.45f, 0.08f, 0.08f);

    // Unit shared
    public static readonly Color UnitShadow = new(0f, 0f, 0f, 0.32f);
    public static readonly Color HitFlash = Color.white;
    public static readonly Color Hero = new(0.95f, 0.85f, 0.45f);

    public static Color ForTower(TowerType type)
    {
        return type switch
        {
            TowerType.Arrow => ArrowTower,
            TowerType.Frost => FrostTower,
            TowerType.Cannon => CannonTower,
            TowerType.Arcane => ArcaneTower,
            TowerType.Barracks => BarracksTower,
            TowerType.DiamondMine => DiamondMineTower,
            TowerType.Spotter => SpotterTower,
            TowerType.Beacon => BeaconTower,
            TowerType.BountyShrine => BountyShrineTower,
            _ => Color.white
        };
    }
}
