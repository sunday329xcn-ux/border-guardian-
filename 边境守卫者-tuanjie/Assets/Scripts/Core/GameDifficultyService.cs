using UnityEngine;

public enum GameDifficulty
{
    Easy,
    Normal,
    Heroic
}

public static class GameDifficultyService
{
    static GameDifficulty current = GameDifficulty.Normal;

    public static GameDifficulty Current => current;

    public static void SetDifficulty(GameDifficulty difficulty)
    {
        current = difficulty;
    }

    public static string GetDisplayName(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => "Easy",
            GameDifficulty.Normal => "Normal",
            GameDifficulty.Heroic => "Heroic",
            _ => "Normal"
        };
    }

    public static int GetStartingLives()
    {
        return current switch
        {
            GameDifficulty.Easy => 25,
            GameDifficulty.Normal => 20,
            GameDifficulty.Heroic => 15,
            _ => 20
        };
    }

    public static int GetStartingGold()
    {
        return current switch
        {
            GameDifficulty.Easy => 250,
            GameDifficulty.Normal => 200,
            GameDifficulty.Heroic => 160,
            _ => 200
        };
    }

    public static int ScaleEnemyHealth(int baseHealth)
    {
        var multiplier = current switch
        {
            GameDifficulty.Easy => 0.85f,
            GameDifficulty.Normal => 1f,
            GameDifficulty.Heroic => 1.25f,
            _ => 1f
        };

        return Mathf.Max(1, Mathf.RoundToInt(baseHealth * multiplier));
    }
}
