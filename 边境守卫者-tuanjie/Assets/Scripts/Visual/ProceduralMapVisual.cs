using UnityEngine;

/// <summary>
/// Procedural map texturing (P-A3). Replaces the flat per-cell colours with
/// cached Perlin-noise tiles (grass / path / platform / blocked) and upgrades the
/// spawn / goal markers to small geometric landmarks. Generates only a handful of
/// shared textures (one per cell type) and reuses them across all cells.
/// </summary>
public static class ProceduralMapVisual
{
    public static void Apply(MapGridController map)
    {
        if (map == null || map.Cells == null || map.TilesRoot == null)
            return;

        TextureTiles(map);
        UpgradeMarkers(map);
    }

    static void TextureTiles(MapGridController map)
    {
        for (int x = 0; x < MapGridSettings.Width; x++)
        for (int y = 0; y < MapGridSettings.Height; y++)
        {
            var tile = map.TilesRoot.Find($"Tile_{x}_{y}");
            if (tile == null)
                continue;

            var renderer = tile.GetComponent<SpriteRenderer>();
            if (renderer == null)
                continue;

            renderer.sprite = NoiseFor(map.Cells[x, y]);
            renderer.color = Color.white;

            // Break up repetition with a hashed quarter-turn (grass/blocked only).
            var cell = map.Cells[x, y];
            if (cell == MapCellType.BuildGround || cell == MapCellType.Blocked)
            {
                var turn = (Mathf.Abs(x * 73856093 ^ y * 19349663) % 4) * 90f;
                tile.localRotation = Quaternion.Euler(0f, 0f, turn);
            }
        }
    }

    static Sprite NoiseFor(MapCellType cellType) => cellType switch
    {
        MapCellType.Path => ProceduralSpriteFactory.GetNoiseSprite(
            VisualPalette.PathBase, VisualPalette.PathEdge, 48, 5f, 2),
        MapCellType.BuildPlatform => ProceduralSpriteFactory.GetNoiseSprite(
            VisualPalette.PlatformBase, Color.Lerp(VisualPalette.PlatformBase, Color.black, 0.3f), 48, 7f, 3),
        MapCellType.Blocked => ProceduralSpriteFactory.GetNoiseSprite(
            VisualPalette.Blocked, Color.Lerp(VisualPalette.Blocked, Color.black, 0.4f), 48, 10f, 4),
        _ => ProceduralSpriteFactory.GetNoiseSprite(
            VisualPalette.GrassBase, VisualPalette.GrassLight, 48, 6f, 1)
    };

    static void UpgradeMarkers(MapGridController map)
    {
        if (map.MarkersRoot == null)
            return;

        AddSpawnDecor(map.MarkersRoot.Find("SpawnMarker_Upper"), new Color(0.4f, 0.7f, 1f));
        AddSpawnDecor(map.MarkersRoot.Find("SpawnMarker_Lower"), new Color(0.5f, 0.65f, 1f));
        AddGoalDecor(map.MarkersRoot.Find("GoalMarker"));
    }

    static void AddSpawnDecor(Transform marker, Color color)
    {
        if (marker == null || marker.Find("Gate") != null)
            return;

        VisualPrimitives.Add(marker, "Gate", VisualShape.Triangle, color,
            new Vector2(1.1f, 0.9f), new Vector2(0f, 0.7f), VisualSorting.Markers);
    }

    static void AddGoalDecor(Transform marker)
    {
        if (marker == null || marker.Find("Core") != null)
            return;

        VisualPrimitives.Add(marker, "Ring", VisualShape.Ring, new Color(1f, 0.4f, 0.4f, 0.85f),
            new Vector2(1.6f, 1.6f), Vector2.zero, VisualSorting.Markers);
        VisualPrimitives.Add(marker, "Core", VisualShape.Diamond, new Color(1f, 0.7f, 0.5f),
            new Vector2(0.7f, 0.7f), Vector2.zero, VisualSorting.Markers);
        marker.gameObject.AddComponent<MarkerPulse>();
    }
}

/// <summary>Gentle scale pulse for the goal core (P-A3).</summary>
public class MarkerPulse : MonoBehaviour
{
    Vector3 baseScale;

    void Start() => baseScale = transform.localScale;

    void Update()
    {
        if (CombatFeedbackService.ReduceMotion)
            return;

        var pulse = 1f + Mathf.Sin(Time.unscaledTime * 2.2f) * 0.06f;
        transform.localScale = baseScale * pulse;
    }
}

/// <summary>
/// Slow pulsing ring on empty build slots (P-A3), replacing the static yellow
/// highlight as the idle "buildable" cue. Hidden while the slot is occupied.
/// </summary>
public class BuildSlotPulse : MonoBehaviour
{
    BuildSlot slot;
    SpriteRenderer ring;

    public void Bind(BuildSlot buildSlot, SpriteRenderer baseRenderer)
    {
        slot = buildSlot;

        var go = new GameObject("BuildPulse");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0f, 0.05f);
        ring = go.AddComponent<SpriteRenderer>();
        ring.sprite = ProceduralSpriteFactory.GetRingSprite(0.12f);
        ring.color = new Color(VisualPalette.BuildValid.r, VisualPalette.BuildValid.g, VisualPalette.BuildValid.b, 0.0f);
        ring.sortingOrder = VisualSorting.Zones;
    }

    void Update()
    {
        if (slot == null || ring == null)
            return;

        if (slot.IsOccupied || CombatFeedbackService.ReduceMotion)
        {
            ring.enabled = false;
            return;
        }

        ring.enabled = true;
        var t = (Mathf.Sin(Time.unscaledTime * 2.4f) + 1f) * 0.5f;
        ring.color = new Color(VisualPalette.BuildValid.r, VisualPalette.BuildValid.g, VisualPalette.BuildValid.b,
            Mathf.Lerp(0.12f, 0.4f, t));
        ring.transform.localScale = Vector3.one * Mathf.Lerp(1.18f, 1.3f, t);
    }
}
