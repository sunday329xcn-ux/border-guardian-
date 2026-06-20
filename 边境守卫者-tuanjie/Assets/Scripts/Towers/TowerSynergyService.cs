using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class TowerSynergyService
{
    public static bool HasPartner(TowerBase tower, TowerType partnerType, float range = TowerSynergyCatalog.DefaultRange)
    {
        return CountNearby(tower, partnerType, range) > 0;
    }

    public static int CountNearby(TowerBase tower, TowerType partnerType, float range)
    {
        if (tower == null || range <= 0f || !tower.HasSynergyUnlocked)
            return 0;

        var count = 0;
        var rangeSqr = range * range;

        foreach (var candidate in TowerRegistry.ActiveTowersSnapshot)
        {
            if (candidate == null || candidate == tower || candidate.TowerType != partnerType)
                continue;

            if (!candidate.HasSynergyUnlocked)
                continue;

            if ((candidate.transform.position - tower.transform.position).sqrMagnitude > rangeSqr)
                continue;

            count++;
        }

        return count;
    }

    public static bool IsRuleActive(TowerBase tower, TowerSynergyRule rule)
    {
        if (tower == null || !tower.HasSynergyUnlocked || !TowerSynergyCatalog.IsCombatTower(tower.TowerType))
            return false;

        if (tower.TowerType == rule.PartnerA)
            return HasPartner(tower, rule.PartnerB, tower.SynergyRange);

        if (tower.TowerType == rule.PartnerB)
            return HasPartner(tower, rule.PartnerA, tower.SynergyRange);

        return false;
    }

    public static List<TowerSynergyRule> GetActiveRules(TowerBase tower)
    {
        var active = new List<TowerSynergyRule>();

        if (tower == null)
            return active;

        foreach (var rule in TowerSynergyCatalog.RulesList)
        {
            if (IsRuleActive(tower, rule))
                active.Add(rule);
        }

        return active;
    }

    public static string BuildPanelSummary(TowerBase tower)
    {
        if (tower == null || tower is DiamondMineTower)
            return string.Empty;

        var tagName = TowerSynergyCatalog.GetTagName(tower.TowerType);
        if (string.IsNullOrEmpty(tagName))
            return string.Empty;

        var builder = new StringBuilder();
        builder.Append("\nTag: ");
        builder.Append(tagName);

        if (!tower.HasSynergyUnlocked)
        {
            builder.Append("\nSynergy: unlocks at Lv.3");
            return builder.ToString();
        }

        var activeRules = GetActiveRules(tower);
        if (activeRules.Count == 0)
        {
            builder.Append("\nSynergy: none nearby");
            return builder.ToString();
        }

        builder.Append("\nSynergy active:");
        foreach (var rule in activeRules)
        {
            builder.Append('\n');
            builder.Append('·');
            builder.Append(' ');
            builder.Append(rule.Summary);
        }

        return builder.ToString();
    }
}
