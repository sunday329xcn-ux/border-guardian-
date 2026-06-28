using UnityEngine;

/// <summary>
/// Central sorting-order registry for the zero-art visual pass (P-A0).
/// Replaces the scattered magic numbers previously hard-coded across map,
/// tower, enemy and VFX renderers. Keep all new visual layers referencing
/// these constants so future effects never fight for draw order.
/// </summary>
public static class VisualSorting
{
    public const int Background = -10;
    public const int Ground = 0;
    public const int GroundDecor = 1;
    public const int Zones = 2;
    public const int Shadows = 3;
    public const int Towers = 4;
    public const int Enemies = 5;
    public const int Soldiers = 6;
    public const int Projectiles = 7;
    public const int Vfx = 8;
    public const int Markers = 9;
    public const int WorldUi = 10;

    /// <summary>Floating combat text overlay canvas (screen-space).</summary>
    public const int OverlayCanvas = 200;
}
