using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Procedural UI skin layer (P-A8): a runtime rounded-rect 9-slice panel sprite,
/// geometric build-bar icons per tower, and a reusable fade-in. Everything is
/// generated at runtime (no external art) and gated by <see cref="Enabled"/> so
/// it can be toggled off without touching call sites.
/// </summary>
public static class ProceduralUiSkin
{
    public static bool Enabled = true;

    static Sprite panelSprite;
    public static Sprite PanelSprite => panelSprite ??= ProceduralSpriteFactory.GetRoundedRectSprite(48, 12, 14);

    /// <summary>Applies the rounded 9-slice sprite to a panel background image.</summary>
    public static void ApplyPanel(Image image)
    {
        if (!Enabled || image == null)
            return;

        image.sprite = PanelSprite;
        image.type = Image.Type.Sliced;
    }

    public static void AddTowerIcon(Transform button, TowerType type)
    {
        if (!Enabled || button == null || button.Find("TypeIcon") != null)
            return;

        var go = new GameObject("TypeIcon", typeof(RectTransform));
        go.transform.SetParent(button, false);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(5f, -5f);
        rect.sizeDelta = IconSizeFor(type);

        var image = go.AddComponent<Image>();
        image.sprite = VisualPrimitives.SpriteFor(IconShapeFor(type));
        image.color = Color.Lerp(VisualPalette.ForTower(type), Color.white, 0.25f);
        image.raycastTarget = false;
    }

    static VisualShape IconShapeFor(TowerType type) => type switch
    {
        TowerType.Arrow => VisualShape.Triangle,
        TowerType.Frost => VisualShape.Circle,
        TowerType.Cannon => VisualShape.Square,
        TowerType.Arcane => VisualShape.Diamond,
        TowerType.Barracks => VisualShape.Triangle,
        TowerType.DiamondMine => VisualShape.Diamond,
        _ => VisualShape.Circle
    };

    static Vector2 IconSizeFor(TowerType type) =>
        type == TowerType.Cannon ? new Vector2(20f, 9f) : new Vector2(16f, 16f);

    public static void AttachFade(GameObject panel)
    {
        if (!Enabled || panel == null)
            return;

        if (panel.GetComponent<UiFadeIn>() == null)
            panel.AddComponent<UiFadeIn>();
    }
}

/// <summary>CanvasGroup fade-in played whenever the object is enabled (P-A8).</summary>
public class UiFadeIn : MonoBehaviour
{
    CanvasGroup group;
    float age;
    bool playing;
    const float Duration = 0.15f;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        if (CombatFeedbackService.ReduceMotion)
        {
            if (group != null)
                group.alpha = 1f;
            playing = false;
            return;
        }

        age = 0f;
        playing = true;
        if (group != null)
            group.alpha = 0f;
    }

    void Update()
    {
        if (!playing || group == null)
            return;

        age += Time.unscaledDeltaTime;
        group.alpha = Mathf.Clamp01(age / Duration);

        if (age >= Duration)
        {
            group.alpha = 1f;
            playing = false;
        }
    }
}
