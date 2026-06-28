using UnityEngine;

/// <summary>Run mode (P4.3): Normal clears at the final scripted wave; Endless never ends.</summary>
public enum GameMode
{
    Normal,
    Endless
}

/// <summary>Optional fixed-constraint challenge applied to a run (P4.3).</summary>
public enum ChallengeId
{
    None,
    ThreeTowers,
    NoBarracks,
    EarlyBoss
}

/// <summary>
/// Holds the selected run mode and challenge (P4.3), chosen on the main menu and
/// persisted in PlayerPrefs. Gameplay systems query the constraint helpers
/// (disabled towers, tower cap, early boss) so the rules live in one place.
/// </summary>
public static class GameModeService
{
    const string ModeKey = "bg.gameMode";
    const string ChallengeKey = "bg.challenge";

    static GameMode mode = GameMode.Normal;
    static ChallengeId challenge = ChallengeId.None;

    public static GameMode Mode => mode;
    public static ChallengeId Challenge => challenge;
    public static bool IsEndless => mode == GameMode.Endless;

    public static void Load()
    {
        mode = (GameMode)Mathf.Clamp(PlayerPrefs.GetInt(ModeKey, 0), 0, 1);
        challenge = (ChallengeId)Mathf.Clamp(PlayerPrefs.GetInt(ChallengeKey, 0), 0, 3);
    }

    public static void SetMode(GameMode value)
    {
        mode = value;
        PlayerPrefs.SetInt(ModeKey, (int)value);
        PlayerPrefs.Save();
    }

    public static void SetChallenge(ChallengeId value)
    {
        challenge = value;
        PlayerPrefs.SetInt(ChallengeKey, (int)value);
        PlayerPrefs.Save();
    }

    public static GameMode CycleMode()
    {
        SetMode(mode == GameMode.Normal ? GameMode.Endless : GameMode.Normal);
        return mode;
    }

    public static ChallengeId CycleChallenge()
    {
        SetChallenge((ChallengeId)(((int)challenge + 1) % 4));
        return challenge;
    }

    // ---- Challenge constraint queries ----

    public static bool IsTowerDisabled(TowerType type) =>
        challenge == ChallengeId.NoBarracks && type == TowerType.Barracks;

    /// <summary>Maximum simultaneous towers (ThreeTowers challenge caps at 3).</summary>
    public static int MaxTowers => challenge == ChallengeId.ThreeTowers ? 3 : int.MaxValue;

    /// <summary>EarlyBoss injects a boss into wave 7.</summary>
    public static bool EarlyBoss => challenge == ChallengeId.EarlyBoss;

    public static string ModeName(GameMode value) => value == GameMode.Endless ? "Endless" : "Normal";

    public static string ChallengeName(ChallengeId value) => value switch
    {
        ChallengeId.ThreeTowers => "Only 3 Towers",
        ChallengeId.NoBarracks => "No Barracks",
        ChallengeId.EarlyBoss => "Early Boss (W7)",
        _ => "No Challenge"
    };
}
