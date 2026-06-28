using UnityEngine;

/// <summary>
/// Local high-score store (P4.3) keyed by run mode, persisted in PlayerPrefs.
/// Can later be backed by a platform leaderboard API. Score is computed as
/// clearedWaves * 100 + remainingLives * 5 so it rewards both depth and survival.
/// </summary>
public static class LeaderboardService
{
    public static int ComputeScore(int clearedWaves, int remainingLives) =>
        Mathf.Max(0, clearedWaves) * 100 + Mathf.Max(0, remainingLives) * 5;

    public static int GetBest(GameMode mode) => PlayerPrefs.GetInt(KeyFor(mode), 0);

    /// <summary>Stores the score if it beats the current best. Returns true on a new record.</summary>
    public static bool Submit(GameMode mode, int score)
    {
        if (score <= GetBest(mode))
            return false;

        PlayerPrefs.SetInt(KeyFor(mode), score);
        PlayerPrefs.Save();
        return true;
    }

    static string KeyFor(GameMode mode) => mode == GameMode.Endless ? "bg.best.endless" : "bg.best.normal";
}
