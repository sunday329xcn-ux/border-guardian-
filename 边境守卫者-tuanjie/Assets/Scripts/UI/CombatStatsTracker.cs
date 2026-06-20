using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class CombatStatsTracker
{
    struct TowerStats
    {
        public int TotalDamage;
        public int Kills;
    }

    static readonly Dictionary<TowerType, TowerStats> stats = new();
    static float combatStartTime = -1f;
    static float combatEndTime = -1f;

    public static void Reset()
    {
        stats.Clear();
        combatStartTime = -1f;
        combatEndTime = -1f;
    }

    public static void MarkCombatStarted()
    {
        if (combatStartTime >= 0f)
            return;

        combatStartTime = Time.time;
    }

    public static void MarkCombatEnded()
    {
        if (combatStartTime < 0f)
            return;

        combatEndTime = Time.time;
    }

    public static void RecordDamage(TowerBase tower, int damage)
    {
        if (tower == null || damage <= 0)
            return;

        MarkCombatStarted();

        if (!stats.TryGetValue(tower.TowerType, out var entry))
            entry = default;

        entry.TotalDamage += damage;
        stats[tower.TowerType] = entry;
    }

    public static void RecordKill(TowerBase tower)
    {
        if (tower == null)
            return;

        if (!stats.TryGetValue(tower.TowerType, out var entry))
            entry = default;

        entry.Kills++;
        stats[tower.TowerType] = entry;
    }

    public static float GetCombatDurationSeconds()
    {
        if (combatStartTime < 0f)
            return 0f;

        var endTime = combatEndTime > 0f ? combatEndTime : Time.time;
        return Mathf.Max(1f, endTime - combatStartTime);
    }

    public static int GetTotalDamage()
    {
        var total = 0;
        foreach (var pair in stats)
            total += pair.Value.TotalDamage;

        return total;
    }

    public static int GetTotalKills()
    {
        var total = 0;
        foreach (var pair in stats)
            total += pair.Value.Kills;

        return total;
    }

    public static string BuildSummaryText()
    {
        if (stats.Count == 0)
            return "No combat data yet.";

        var builder = new StringBuilder();
        if (combatStartTime >= 0f)
        {
            var duration = GetCombatDurationSeconds();
            builder.AppendLine($"Combat: {GetTotalDamage()} dmg · {GetTotalKills()} kills · {duration:F0}s · {GetTotalDamage() / duration:F0} DPS");
            builder.AppendLine();
        }

        foreach (var pair in stats.OrderByDescending(p => p.Value.TotalDamage))
        {
            var name = TowerBuildCatalog.GetDisplayName(pair.Key);
            builder.AppendLine($"{name}: {pair.Value.TotalDamage} dmg, {pair.Value.Kills} kills");
        }

        return builder.ToString().TrimEnd();
    }

    public static string BuildVictorySummaryText()
    {
        if (stats.Count == 0)
            return "No tower damage recorded this run.";

        var duration = GetCombatDurationSeconds();
        var totalDamage = GetTotalDamage();
        var totalKills = GetTotalKills();
        var overallDps = totalDamage / duration;

        var builder = new StringBuilder();
        builder.AppendLine(
            $"Total: {totalDamage} dmg · {totalKills} kills · {duration:F0}s · {overallDps:F0} DPS");
        builder.AppendLine();

        foreach (var pair in stats.OrderByDescending(p => p.Value.TotalDamage))
        {
            var name = TowerBuildCatalog.GetDisplayName(pair.Key);
            var dps = pair.Value.TotalDamage / duration;
            builder.AppendLine($"{name}: {pair.Value.TotalDamage} dmg ({dps:F0} DPS), {pair.Value.Kills} kills");
        }

        return builder.ToString().TrimEnd();
    }
}
