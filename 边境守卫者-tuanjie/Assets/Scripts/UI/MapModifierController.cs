using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applies the visual layer of the active <see cref="MapModifier"/> (P4.2) as a
/// full-screen tint overlay. The overlay is purely cosmetic (raycastTarget off,
/// drawn beneath the HUD) so it never blocks input or overlaps interactive UI.
/// Rain's gameplay effect (slow amplification) is handled in EnemySlowEffect.
/// </summary>
public class MapModifierController : MonoBehaviour
{
    Image overlay;
    MapModifier appliedModifier = (MapModifier)(-1);

    void Start()
    {
        CreateOverlay();
        Apply(MapModifierService.Active);
    }

    void Update()
    {
        if (MapModifierService.Active != appliedModifier)
            Apply(MapModifierService.Active);
    }

    void CreateOverlay()
    {
        var go = new GameObject("MapModifierOverlay", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        go.transform.SetAsFirstSibling();

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        overlay = go.AddComponent<Image>();
        overlay.raycastTarget = false;
    }

    void Apply(MapModifier modifier)
    {
        appliedModifier = modifier;
        if (overlay == null)
            return;

        overlay.color = modifier switch
        {
            MapModifier.Night => new Color(0.05f, 0.07f, 0.18f, 0.34f),
            MapModifier.Fog => new Color(0.55f, 0.58f, 0.60f, 0.26f),
            MapModifier.Rain => new Color(0.15f, 0.25f, 0.45f, 0.24f),
            _ => new Color(0f, 0f, 0f, 0f)
        };

        overlay.enabled = modifier != MapModifier.None;
    }
}
