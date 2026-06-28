using UnityEngine;

/// <summary>Single-map variant modifiers (P4.2): same layout, changed atmosphere / tuning.</summary>
public enum MapModifier
{
    None,
    Night,
    Fog,
    Rain
}

/// <summary>
/// Holds the active map modifier (P4.2). Persisted in PlayerPrefs so the choice
/// survives a level restart. Gameplay tuning is exposed as queries; the visual
/// layer is applied by <c>MapModifierController</c>.
/// </summary>
public static class MapModifierService
{
    const string PrefKey = "bg.mapModifier";

    static MapModifier active = MapModifier.None;

    public static MapModifier Active => active;

    /// <summary>Rain amplifies slow effects (+10%).</summary>
    public static float SlowMultiplier => active == MapModifier.Rain ? 1.1f : 1f;

    public static void Load()
    {
        var stored = PlayerPrefs.GetInt(PrefKey, 0);
        active = (MapModifier)Mathf.Clamp(stored, 0, 3);
    }

    public static void Set(MapModifier modifier)
    {
        active = modifier;
        PlayerPrefs.SetInt(PrefKey, (int)modifier);
        PlayerPrefs.Save();
    }

    public static MapModifier Cycle()
    {
        Set((MapModifier)(((int)active + 1) % 4));
        return active;
    }

    public static string GetName(MapModifier modifier) => modifier switch
    {
        MapModifier.Night => "Night",
        MapModifier.Fog => "Fog",
        MapModifier.Rain => "Rain",
        _ => "Clear"
    };
}
