using UnityEngine;

public static class DamageCalculator
{
    public static int CalculatePhysicalDamage(int baseDamage, int armor)
    {
        if (baseDamage <= 0) return 0;

        armor = Mathf.Max(0, armor);
        return Mathf.Max(1, Mathf.RoundToInt(baseDamage * 100f / (100f + armor)));
    }

    public static int CalculateMagicDamage(int baseDamage, int magicResistPercent)
    {
        if (baseDamage <= 0) return 0;

        magicResistPercent = Mathf.Clamp(magicResistPercent, 0, 95);
        return Mathf.Max(1, Mathf.RoundToInt(baseDamage * (100f - magicResistPercent) / 100f));
    }
}
