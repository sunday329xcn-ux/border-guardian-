using UnityEngine;

/// <summary>
/// Environment mechanic visuals (P-A6): ancient-tree canopy + activation burst
/// and hunter-trap cross + trigger flash. Additive only — the gameplay scripts
/// (AncientTree / HunterTrap) keep all their logic and just call these helpers.
/// </summary>
public static class EnvironmentVisual
{
    public static void DecorateTree(Transform tree)
    {
        if (tree == null || tree.Find("Canopy") != null)
            return;

        var canopy = new GameObject("Canopy");
        canopy.transform.SetParent(tree, false);
        canopy.transform.localPosition = new Vector3(0f, 0.6f, -0.05f);

        VisualPrimitives.Add(canopy.transform, "LeafL", VisualShape.Circle, new Color(0.2f, 0.5f, 0.24f),
            new Vector2(0.9f, 0.9f), new Vector2(-0.25f, 0f), VisualSorting.Shadows);
        VisualPrimitives.Add(canopy.transform, "LeafR", VisualShape.Circle, new Color(0.25f, 0.58f, 0.28f),
            new Vector2(0.9f, 0.9f), new Vector2(0.25f, 0.12f), VisualSorting.Shadows);

        canopy.AddComponent<TreeCanopyPulse>().Bind(tree.GetComponent<AncientTree>());
    }

    public static void PlayTreeActivate(Vector3 from, Vector3 to)
    {
        if (CombatFeedbackService.ReduceMotion)
            return;

        var go = new GameObject("TreeActivateFx");
        var fx = go.AddComponent<EnvironmentLineFx>();
        fx.Begin(from, to, new Color(0.4f, 0.9f, 0.45f));

        SpawnRing(to, new Color(0.4f, 0.95f, 0.5f, 0.8f), 2f);
    }

    public static void DecorateTrap(Transform trap)
    {
        if (trap == null || trap.Find("CrossA") != null)
            return;

        var dark = new Color(0.2f, 0.13f, 0.05f);
        VisualPrimitives.Add(trap, "CrossA", VisualShape.Square, dark, new Vector2(0.9f, 0.16f), new Vector2(0f, 0f), VisualSorting.Markers, 45f);
        VisualPrimitives.Add(trap, "CrossB", VisualShape.Square, dark, new Vector2(0.9f, 0.16f), new Vector2(0f, 0f), VisualSorting.Markers, -45f);
    }

    public static void PlayTrapTrigger(Vector3 position)
    {
        SpawnRing(position, new Color(1f, 1f, 1f, 0.9f), 1.4f);
    }

    static void SpawnRing(Vector3 position, Color color, float maxScale)
    {
        var go = new GameObject("EnvRingFx");
        go.transform.position = position;
        go.AddComponent<EnvironmentRingFx>().Begin(color, maxScale);
    }
}

/// <summary>Slow green pulse on the tree canopy while the tree is ready (P-A6).</summary>
public class TreeCanopyPulse : MonoBehaviour
{
    AncientTree tree;
    Vector3 baseScale;

    public void Bind(AncientTree owner)
    {
        tree = owner;
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (CombatFeedbackService.ReduceMotion)
            return;

        var ready = tree == null || tree.IsReady;
        var amp = ready ? 0.06f : 0.01f;
        transform.localScale = baseScale * (1f + Mathf.Sin(Time.unscaledTime * 1.8f) * amp);
    }
}

/// <summary>Transient root-vine line from tree to its effect cell (P-A6).</summary>
public class EnvironmentLineFx : MonoBehaviour
{
    LineRenderer line;
    float age;
    const float Lifetime = 0.6f;

    public void Begin(Vector3 from, Vector3 to, Color color)
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.positionCount = 2;
        line.SetPosition(0, from);
        line.SetPosition(1, to);
        line.startWidth = 0.12f;
        line.endWidth = 0.04f;
        line.startColor = color;
        line.endColor = color;
        line.sortingOrder = VisualSorting.Vfx;
    }

    void Update()
    {
        age += Time.deltaTime;
        var t = Mathf.Clamp01(age / Lifetime);
        if (line != null)
        {
            var c = line.startColor;
            c.a = 1f - t;
            line.startColor = c;
            line.endColor = c;
        }

        if (t >= 1f)
            Destroy(gameObject);
    }
}

/// <summary>Generic expanding ring burst for environment feedback (P-A6).</summary>
public class EnvironmentRingFx : MonoBehaviour
{
    SpriteRenderer ring;
    float age;
    float maxScale;
    Color baseColor;
    const float Lifetime = 0.35f;

    public void Begin(Color color, float targetScale)
    {
        baseColor = color;
        maxScale = targetScale;
        ring = gameObject.AddComponent<SpriteRenderer>();
        ring.sprite = ProceduralSpriteFactory.GetRingSprite(0.2f);
        ring.color = color;
        ring.sortingOrder = VisualSorting.Vfx;
        transform.localScale = Vector3.one * 0.3f;
    }

    void Update()
    {
        age += Time.deltaTime;
        var t = Mathf.Clamp01(age / Lifetime);
        transform.localScale = Vector3.one * Mathf.Lerp(0.3f, maxScale, t);
        if (ring != null)
            ring.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - t));

        if (t >= 1f)
            Destroy(gameObject);
    }
}
