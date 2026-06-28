using UnityEngine;

/// <summary>
/// Global enemy health multiplier for Endless rounds (P4.3). WaveManager sets the
/// round before each endless wave spawns; EnemyBase reads the multiplier when
/// applying stats so all enemies scale uniformly.
/// </summary>
public static class EndlessScalingService
{
    public static float HealthMultiplier { get; private set; } = 1f;

    public static void Reset()
    {
        HealthMultiplier = 1f;
    }

    /// <summary>endlessRound = 1 for the first wave beyond the scripted set.</summary>
    public static void SetRound(int endlessRound)
    {
        HealthMultiplier = 1f + 0.25f * Mathf.Max(0, endlessRound);
    }
}
