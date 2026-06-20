public static class LevelStarRating
{
    public const int MaxStars = 3;

    /// <summary>
    /// 1★ clear · 2★ lives ≥ half · 3★ lives ≥ max-2 (near perfect).
    /// </summary>
    public static int Calculate(int livesRemaining, int maxLives)
    {
        if (livesRemaining <= 0 || maxLives <= 0)
            return 0;

        if (livesRemaining >= maxLives - 2)
            return 3;

        if (livesRemaining >= maxLives / 2)
            return 2;

        return 1;
    }

    public static string BuildStarText(int stars, bool useEmpty = true)
    {
        stars = UnityEngine.Mathf.Clamp(stars, 0, MaxStars);
        var filled = new string('*', stars);
        if (!useEmpty || stars >= MaxStars)
            return filled;

        return filled + new string('-', MaxStars - stars);
    }
}
