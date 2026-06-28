using UnityEngine;

/// <summary>
/// Tower silhouette composer (P-A4). Builds distinct geometric decoration on top
/// of the plain coloured body produced by <c>TowerVisualFactory</c>: a per-type
/// accent shape, level pips, a branch accent, plus the shared shadow / hit-punch
/// (P-A1). Rebuilt idempotently from <c>TowerBase.RefreshPresentation</c> so it
/// stays in sync with level/branch without touching gameplay logic.
/// </summary>
public static class TowerVisualComposer
{
    const float DecorZ = -0.05f;

    public static SpriteRenderer Compose(SpriteRenderer body, TowerType type, int level, TowerBranch branch)
    {
        if (body == null)
            return null;

        body.sortingOrder = VisualSorting.Towers;

        var root = body.transform;
        UnitVisualDecorator.Attach(body.gameObject, new Vector2(1.0f, 0.42f), enableBob: false);

        var decor = VisualPrimitives.EnsureContainer(root, "Decor");
        BuildSilhouette(decor, type);
        BuildLevelPips(decor, level);
        BuildBranchAccent(decor, branch);
        ApplyDecorDepth(decor);
        return body;
    }

    public static void PlayLanding(GameObject tower)
    {
        if (tower == null || CombatFeedbackService.ReduceMotion)
            return;

        if (tower.GetComponent<TowerLandingAnim>() == null)
            tower.AddComponent<TowerLandingAnim>();
    }

    static void BuildSilhouette(Transform decor, TowerType type)
    {
        var accent = Color.Lerp(VisualPalette.ForTower(type), Color.white, 0.45f);
        var dark = Color.Lerp(VisualPalette.ForTower(type), Color.black, 0.35f);

        switch (type)
        {
            case TowerType.Arrow:
                VisualPrimitives.Add(decor, "Roof", VisualShape.Triangle, accent, new Vector2(0.5f, 0.45f), new Vector2(0f, 0.28f), VisualSorting.Towers);
                VisualPrimitives.Add(decor, "Slot", VisualShape.Square, dark, new Vector2(0.18f, 0.32f), new Vector2(0f, 0.02f), VisualSorting.Towers);
                break;
            case TowerType.Frost:
                VisualPrimitives.Add(decor, "Crystal", VisualShape.Diamond, new Color(0.85f, 0.95f, 1f), new Vector2(0.34f, 0.6f), new Vector2(0f, 0.2f), VisualSorting.Towers);
                break;
            case TowerType.Cannon:
                VisualPrimitives.Add(decor, "Barrel", VisualShape.Square, dark, new Vector2(0.62f, 0.2f), new Vector2(0.12f, 0.12f), VisualSorting.Towers, 8f);
                break;
            case TowerType.Arcane:
                VisualPrimitives.Add(decor, "Core", VisualShape.Diamond, accent, new Vector2(0.4f, 0.4f), new Vector2(0f, 0.18f), VisualSorting.Towers);
                VisualPrimitives.Add(decor, "Ring", VisualShape.Ring, new Color(accent.r, accent.g, accent.b, 0.7f), new Vector2(0.85f, 0.85f), new Vector2(0f, 0.05f), VisualSorting.Towers);
                break;
            case TowerType.Barracks:
                VisualPrimitives.Add(decor, "Pole", VisualShape.Square, dark, new Vector2(0.08f, 0.6f), new Vector2(0.16f, 0.2f), VisualSorting.Towers);
                VisualPrimitives.Add(decor, "Flag", VisualShape.Triangle, accent, new Vector2(0.3f, 0.22f), new Vector2(0.3f, 0.4f), VisualSorting.Towers, 90f);
                break;
            case TowerType.DiamondMine:
                VisualPrimitives.Add(decor, "Gem", VisualShape.Diamond, new Color(0.55f, 0.95f, 1f), new Vector2(0.36f, 0.5f), new Vector2(0f, 0.2f), VisualSorting.Towers);
                break;
            default:
                VisualPrimitives.Add(decor, "Mark", VisualShape.Circle, accent, new Vector2(0.34f, 0.34f), new Vector2(0f, 0.2f), VisualSorting.Towers);
                break;
        }
    }

    static void BuildLevelPips(Transform decor, int level)
    {
        var pips = Mathf.Clamp(level, 1, 5);
        var start = -(pips - 1) * 0.5f * 0.16f;
        for (int i = 0; i < pips; i++)
        {
            VisualPrimitives.Add(decor, $"Pip{i}", VisualShape.Square, VisualPalette.Selected,
                new Vector2(0.1f, 0.1f), new Vector2(start + i * 0.16f, -0.42f), VisualSorting.Towers);
        }
    }

    static void BuildBranchAccent(Transform decor, TowerBranch branch)
    {
        if (branch == TowerBranch.None)
            return;

        var warm = branch == TowerBranch.BranchA;
        var color = warm ? new Color(1f, 0.6f, 0.3f) : new Color(0.4f, 0.7f, 1f);
        var shape = warm ? VisualShape.Triangle : VisualShape.Diamond;
        VisualPrimitives.Add(decor, "BranchAccent", shape, color, new Vector2(0.2f, 0.2f),
            new Vector2(-0.28f, 0.34f), VisualSorting.Towers);
    }

    static void ApplyDecorDepth(Transform decor)
    {
        for (int i = 0; i < decor.childCount; i++)
        {
            var child = decor.GetChild(i);
            var p = child.localPosition;
            child.localPosition = new Vector3(p.x, p.y, DecorZ);
        }
    }
}

/// <summary>One-shot build "drop" scale animation (P-A4): 0 → 1.12 → 1 over 0.25s.</summary>
public class TowerLandingAnim : MonoBehaviour
{
    Vector3 target;
    float age;
    const float Duration = 0.25f;

    void Start()
    {
        target = transform.localScale;
    }

    void Update()
    {
        age += Time.deltaTime;
        var t = Mathf.Clamp01(age / Duration);
        var scale = t < 0.7f
            ? Mathf.Lerp(0f, 1.12f, t / 0.7f)
            : Mathf.Lerp(1.12f, 1f, (t - 0.7f) / 0.3f);

        transform.localScale = target * scale;

        if (t >= 1f)
        {
            transform.localScale = target;
            Destroy(this);
        }
    }
}
