using UnityEngine;

/// <summary>
/// Shared unit presentation layer (P-A1): a soft ground shadow, a hit scale
/// "punch", and an optional idle squash bob. Animations are applied as a
/// multiplier over the unit's base local scale and only while active, so they
/// don't fight external scaling (e.g. tower upgrade RefreshPresentation). All
/// motion respects <see cref="CombatFeedbackService.ReduceMotion"/>.
/// </summary>
public class UnitVisualDecorator : MonoBehaviour
{
    Vector3 baseScale = Vector3.one;
    bool enableBob;
    float bobPhase;

    float punchUntil = -1f;
    const float PunchDuration = 0.14f;
    const float PunchStrength = 0.16f;

    public static UnitVisualDecorator Attach(GameObject unit, Vector2 shadowScale, bool enableBob,
        int shadowSortingOrder = -1)
    {
        if (unit == null)
            return null;

        var decorator = unit.GetComponent<UnitVisualDecorator>();
        if (decorator == null)
            decorator = unit.AddComponent<UnitVisualDecorator>();

        decorator.baseScale = unit.transform.localScale;
        decorator.enableBob = enableBob;
        decorator.bobPhase = Random.Range(0f, Mathf.PI * 2f);
        decorator.EnsureShadow(unit.transform, shadowScale,
            shadowSortingOrder < 0 ? VisualSorting.Shadows : shadowSortingOrder);
        return decorator;
    }

    void EnsureShadow(Transform parent, Vector2 shadowScale, int sortingOrder)
    {
        if (parent.Find("UnitShadow") != null)
            return;

        VisualPrimitives.Add(parent, "UnitShadow", VisualShape.SoftShadow, VisualPalette.UnitShadow,
            shadowScale, new Vector2(0f, -0.34f), sortingOrder);
    }

    /// <summary>Call when the unit takes a hit to trigger a brief scale punch.</summary>
    public void Punch()
    {
        if (CombatFeedbackService.ReduceMotion)
            return;

        baseScale = ResolveBaseScale();
        punchUntil = Time.time + PunchDuration;
    }

    Vector3 ResolveBaseScale()
    {
        // If no animation is currently driving scale, the current transform scale
        // is the authoritative base (captures external upgrade scaling on towers).
        return punchUntil < 0f && !enableBob ? transform.localScale : baseScale;
    }

    void Update()
    {
        var punching = Time.time < punchUntil;
        var bobbing = enableBob && !CombatFeedbackService.ReduceMotion;

        if (!punching && !bobbing)
        {
            if (punchUntil >= 0f)
            {
                transform.localScale = baseScale;
                punchUntil = -1f;
            }
            return;
        }

        var factorX = 1f;
        var factorY = 1f;

        if (punching)
        {
            var t = 1f - Mathf.Clamp01((punchUntil - Time.time) / PunchDuration);
            var pulse = Mathf.Sin(t * Mathf.PI) * PunchStrength;
            factorX += pulse;
            factorY += pulse;
        }

        if (bobbing)
        {
            bobPhase += Time.deltaTime * 3.2f;
            var squash = Mathf.Sin(bobPhase) * 0.03f;
            factorY += squash;
            factorX -= squash * 0.5f;
        }

        transform.localScale = new Vector3(baseScale.x * factorX, baseScale.y * factorY, baseScale.z);
    }
}
