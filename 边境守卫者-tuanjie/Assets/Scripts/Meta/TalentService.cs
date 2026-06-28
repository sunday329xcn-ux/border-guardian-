using System;
using UnityEngine;

public enum TalentId
{
    StartingGold,
    CallEarly,
    EnvironmentCooldown,
    HeroStartLevel
}

/// <summary>
/// Meta progression talent tree (P4.4). Keys earned from new star ratings
/// (<see cref="LevelProgressService"/>) are spent here on permanent, run-wide
/// passives. Purchases persist in <see cref="PlayerPrefs"/>. Effects are pure
/// queries consumed by gameplay systems so balance lives in one place.
/// </summary>
public static class TalentService
{
    const string PurchasedKeyPrefix = "bg_talent_";

    public static event Action OnChanged;

    static readonly TalentId[] all =
    {
        TalentId.StartingGold,
        TalentId.CallEarly,
        TalentId.EnvironmentCooldown,
        TalentId.HeroStartLevel
    };

    static bool loaded;
    static readonly System.Collections.Generic.HashSet<TalentId> purchased = new();

    public static System.Collections.Generic.IReadOnlyList<TalentId> All => all;

    public static void EnsureLoaded()
    {
        if (loaded)
            return;

        purchased.Clear();
        foreach (var id in all)
        {
            if (PlayerPrefs.GetInt(PurchasedKeyPrefix + (int)id, 0) == 1)
                purchased.Add(id);
        }

        loaded = true;
    }

    public static int GetCost(TalentId id) => id switch
    {
        TalentId.HeroStartLevel => 2,
        _ => 1
    };

    public static bool IsPurchased(TalentId id)
    {
        EnsureLoaded();
        return purchased.Contains(id);
    }

    public static int SpentKeys
    {
        get
        {
            EnsureLoaded();
            var total = 0;
            foreach (var id in purchased)
                total += GetCost(id);
            return total;
        }
    }

    public static int AvailableKeys => Mathf.Max(0, LevelProgressService.TotalKeys - SpentKeys);

    public static bool CanPurchase(TalentId id)
    {
        EnsureLoaded();
        return !IsPurchased(id) && AvailableKeys >= GetCost(id);
    }

    public static bool Purchase(TalentId id)
    {
        if (!CanPurchase(id))
            return false;

        purchased.Add(id);
        PlayerPrefs.SetInt(PurchasedKeyPrefix + (int)id, 1);
        PlayerPrefs.Save();
        OnChanged?.Invoke();
        return true;
    }

    public static void ResetAll()
    {
        EnsureLoaded();
        foreach (var id in all)
            PlayerPrefs.DeleteKey(PurchasedKeyPrefix + (int)id);
        purchased.Clear();
        PlayerPrefs.Save();
        OnChanged?.Invoke();
    }

    public static string GetTitle(TalentId id) => id switch
    {
        TalentId.StartingGold => "War Chest",
        TalentId.CallEarly => "Forced March",
        TalentId.EnvironmentCooldown => "Forest Attunement",
        TalentId.HeroStartLevel => "Veteran Hero",
        _ => id.ToString()
    };

    public static string GetDescription(TalentId id) => id switch
    {
        TalentId.StartingGold => "+50 starting gold each run.",
        TalentId.CallEarly => "Call Early bonus gold +20%.",
        TalentId.EnvironmentCooldown => "Ancient Tree / Trap cooldown -10%.",
        TalentId.HeroStartLevel => "Hero starts at level 2.",
        _ => string.Empty
    };

    // -------- Gameplay effect queries --------

    public static int StartingGoldBonus => IsPurchased(TalentId.StartingGold) ? 50 : 0;

    public static float CallEarlyTalentMultiplier => IsPurchased(TalentId.CallEarly) ? 1.2f : 1f;

    public static float EnvironmentCooldownMultiplier => IsPurchased(TalentId.EnvironmentCooldown) ? 0.9f : 1f;

    public static int HeroStartLevelBonus => IsPurchased(TalentId.HeroStartLevel) ? 1 : 0;
}
