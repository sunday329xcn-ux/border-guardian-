using System;
using UnityEngine;

public readonly struct LevelVictoryResult
{
    public LevelVictoryResult(
        LevelId levelId,
        int starsEarned,
        int previousBestStars,
        int newStars,
        int keysGained,
        int totalKeys)
    {
        LevelId = levelId;
        StarsEarned = starsEarned;
        PreviousBestStars = previousBestStars;
        NewStars = newStars;
        KeysGained = keysGained;
        TotalKeys = totalKeys;
    }

    public LevelId LevelId { get; }
    public int StarsEarned { get; }
    public int PreviousBestStars { get; }
    public int NewStars { get; }
    public int KeysGained { get; }
    public int TotalKeys { get; }
    public bool Improved => StarsEarned > PreviousBestStars;
}

public static class LevelProgressService
{
    const string TotalKeysKey = "bg_total_keys";
    const string BestStarsKeyPrefix = "bg_best_stars_";

    public const int LavafallRiftKeyCost = 3;

    static bool loaded;
    static int totalKeys;

    public static event Action OnProgressChanged;

    public static int TotalKeys
    {
        get
        {
            EnsureLoaded();
            return totalKeys;
        }
    }

    public static int GetBestStars(LevelId levelId)
    {
        EnsureLoaded();
        return PlayerPrefs.GetInt(BestStarsKeyPrefix + (int)levelId, 0);
    }

    public static bool IsLevelUnlocked(LevelId levelId)
    {
        if (levelId == LevelId.GrimmForest)
            return true;

        if (levelId == LevelId.LavafallRift)
            return TotalKeys >= LavafallRiftKeyCost;

        return false;
    }

    public static string GetLevelDisplayName(LevelId levelId)
    {
        return levelId switch
        {
            LevelId.GrimmForest => "Grimm Forest",
            LevelId.LavafallRift => "Lavafall Rift",
            _ => levelId.ToString()
        };
    }

    public static LevelVictoryResult RecordVictory(LevelId levelId, int livesRemaining, int maxLives)
    {
        EnsureLoaded();

        var starsEarned = LevelStarRating.Calculate(livesRemaining, maxLives);
        var previousBest = GetBestStars(levelId);
        var newStars = Mathf.Max(0, starsEarned - previousBest);

        if (starsEarned > previousBest)
            PlayerPrefs.SetInt(BestStarsKeyPrefix + (int)levelId, starsEarned);

        if (newStars > 0)
        {
            totalKeys += newStars;
            PlayerPrefs.SetInt(TotalKeysKey, totalKeys);
        }

        if (starsEarned > previousBest || newStars > 0)
            PlayerPrefs.Save();

        var result = new LevelVictoryResult(
            levelId,
            starsEarned,
            previousBest,
            newStars,
            newStars,
            totalKeys);

        OnProgressChanged?.Invoke();
        return result;
    }

    public static void ResetAllProgress()
    {
        totalKeys = 0;
        PlayerPrefs.DeleteKey(TotalKeysKey);
        PlayerPrefs.DeleteKey(BestStarsKeyPrefix + (int)LevelId.GrimmForest);
        PlayerPrefs.DeleteKey(BestStarsKeyPrefix + (int)LevelId.LavafallRift);
        PlayerPrefs.Save();
        OnProgressChanged?.Invoke();
    }

    static void EnsureLoaded()
    {
        if (loaded)
            return;

        totalKeys = PlayerPrefs.GetInt(TotalKeysKey, 0);
        loaded = true;
    }
}
