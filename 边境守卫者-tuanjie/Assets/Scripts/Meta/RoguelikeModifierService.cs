using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Roguelike global buff stacks (P4.2). The player picks one of three offered
/// buffs after each cleared wave; stacks persist for the current run and are
/// reset at session start. Buff effects are queried from the relevant gameplay
/// systems (tower damage pipeline, kill rewards, synergy radius, etc.).
/// </summary>
public static class RoguelikeModifierService
{
    /// <summary>Master switch (could be gated behind meta progression later).</summary>
    public static bool Enabled = true;

    static int towerDamageStacks;
    static int killGoldStacks;
    static int synergyRangeStacks;
    static int callEarlyStacks;
    static int heroAuraStacks;

    public static float TowerDamageMultiplier => 1f + 0.10f * towerDamageStacks;
    public static int KillGoldBonus => killGoldStacks;
    public static float SynergyRangeBonus => 0.5f * synergyRangeStacks;
    public static float CallEarlyMultiplier => 1f + 0.25f * callEarlyStacks;
    public static float HeroAuraBonus => 0.05f * heroAuraStacks;

    public static void Reset()
    {
        towerDamageStacks = 0;
        killGoldStacks = 0;
        synergyRangeStacks = 0;
        callEarlyStacks = 0;
        heroAuraStacks = 0;
    }

    public static void Apply(RoguelikeBuffId buff)
    {
        switch (buff)
        {
            case RoguelikeBuffId.TowerDamage: towerDamageStacks++; break;
            case RoguelikeBuffId.KillGold: killGoldStacks++; break;
            case RoguelikeBuffId.SynergyRange: synergyRangeStacks++; break;
            case RoguelikeBuffId.CallEarly: callEarlyStacks++; break;
            case RoguelikeBuffId.HeroAura: heroAuraStacks++; break;
            case RoguelikeBuffId.FieldMedic: GameManager.Instance?.RestoreLives(); break;
        }
    }

    public static string GetTitle(RoguelikeBuffId buff) => buff switch
    {
        RoguelikeBuffId.TowerDamage => "Sharpened Arms",
        RoguelikeBuffId.KillGold => "Bounty Hunter",
        RoguelikeBuffId.SynergyRange => "Resonance",
        RoguelikeBuffId.CallEarly => "Vanguard",
        RoguelikeBuffId.HeroAura => "Inspiring Presence",
        RoguelikeBuffId.FieldMedic => "Field Medic",
        _ => "Buff"
    };

    public static string GetDescription(RoguelikeBuffId buff) => buff switch
    {
        RoguelikeBuffId.TowerDamage => "+10% tower damage",
        RoguelikeBuffId.KillGold => "+1 gold per kill",
        RoguelikeBuffId.SynergyRange => "+0.5 synergy radius",
        RoguelikeBuffId.CallEarly => "+25% Call Early gold",
        RoguelikeBuffId.HeroAura => "+5% hero aura damage",
        RoguelikeBuffId.FieldMedic => "Restore lives to full",
        _ => string.Empty
    };

    public static List<RoguelikeBuffId> RollChoices(int count)
    {
        var pool = new List<RoguelikeBuffId>
        {
            RoguelikeBuffId.TowerDamage,
            RoguelikeBuffId.KillGold,
            RoguelikeBuffId.SynergyRange,
            RoguelikeBuffId.CallEarly,
            RoguelikeBuffId.HeroAura,
            RoguelikeBuffId.FieldMedic
        };

        var result = new List<RoguelikeBuffId>();
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            var idx = Random.Range(0, pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        return result;
    }
}
