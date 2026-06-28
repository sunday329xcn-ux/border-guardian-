using UnityEngine;

/// <summary>
/// Hero passive aura (P4.1): towers within range of the living hero gain a
/// flat outgoing-damage bonus. Combat towers query this from their damage
/// pipeline; when no hero exists the multiplier is a no-op 1.0.
/// </summary>
public static class HeroAuraService
{
    static HeroUnit activeHero;

    public static void SetHero(HeroUnit hero) => activeHero = hero;

    public static void ClearHero(HeroUnit hero)
    {
        if (activeHero == hero)
            activeHero = null;
    }

    public static float GetDamageMultiplier(Vector3 towerPosition)
    {
        if (activeHero == null || !activeHero.IsAlive)
            return 1f;

        var radius = activeHero.AuraRadius;
        if ((towerPosition - activeHero.transform.position).sqrMagnitude > radius * radius)
            return 1f;

        return 1f + activeHero.AuraDamageBonus;
    }
}
