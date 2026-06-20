using UnityEngine;

/// <summary>
/// Attack range and synergy activation range scale with tower level;
/// Lv.5 branches apply different deltas (some longer, some unchanged, some shorter).
/// Synergy (blue ring) unlocks at Lv.3+.
/// </summary>
public static class TowerRangeScaling
{
    public const int SynergyUnlockLevel = 3;
    const float GlobalAttackRangeScale = 0.8f;
    const float SynergyMinRange = 2.8f;

    struct Profile
    {
        public float BaseAttack;
        public float AttackPerLevel;
        public float BaseSynergy;
        public float SynergyPerLevel;
        public float BranchAAttackDelta;
        public float BranchASynergyDelta;
        public float BranchBAttackDelta;
        public float BranchBSynergyDelta;
    }

    public static bool HasSynergyUnlocked(TowerBase tower)
    {
        return tower != null && tower.Level >= SynergyUnlockLevel;
    }

    public static void ApplyTo(TowerBase tower)
    {
        if (tower == null || tower is DiamondMineTower)
            return;

        if (tower is BarracksTower barracks)
        {
            ApplyBarracks(barracks);
            return;
        }

        if (!TowerSynergyCatalog.IsCombatTower(tower.TowerType))
            return;

        var profile = GetProfile(tower.TowerType);
        Compute(
            tower.Level,
            tower.Branch,
            profile,
            out var attackRange,
            out var synergyRange);

        tower.SetRanges(attackRange, synergyRange);
    }

    public static float GetPreviewAttackRange(TowerType type)
    {
        if (type == TowerType.Barracks)
            return ScaleAttackRange(1.7f);

        if (type == TowerType.DiamondMine)
            return 0f;

        var profile = GetProfile(type);
        Compute(1, TowerBranch.None, profile, out var attackRange, out _);
        return attackRange;
    }

    static void ApplyBarracks(BarracksTower barracks)
    {
        var level = barracks.Level;
        var branch = barracks.Branch;

        var rally = 1.7f + (level - 1) * 0.22f;
        var synergy = 0f;

        if (level >= SynergyUnlockLevel)
            synergy = 2.35f + (level - 1) * 0.1f;

        if (level >= 5 && branch != TowerBranch.None)
        {
            if (branch == TowerBranch.BranchA)
            {
                rally += 0.55f;
                if (level >= SynergyUnlockLevel)
                    synergy += 0.35f;
            }
            else
            {
                rally += 0.35f;
                if (level >= SynergyUnlockLevel)
                    synergy -= 0.15f;
            }
        }

        rally = ScaleAttackRange(rally);
        synergy = level >= SynergyUnlockLevel ? FinalizeSynergyRange(synergy) : 0f;

        barracks.SetRallyRange(Mathf.Clamp(rally, 1.2f, 3.9f));
        barracks.SetRanges(
            Mathf.Clamp(rally, 1.2f, 3.9f),
            synergy);
    }

    static void Compute(
        int level,
        TowerBranch branch,
        Profile profile,
        out float attackRange,
        out float synergyRange)
    {
        var safeLevel = Mathf.Clamp(level, 1, 5);
        attackRange = profile.BaseAttack + (safeLevel - 1) * profile.AttackPerLevel;
        synergyRange = safeLevel >= SynergyUnlockLevel
            ? profile.BaseSynergy + (safeLevel - 1) * profile.SynergyPerLevel
            : 0f;

        if (safeLevel >= 5 && branch != TowerBranch.None)
        {
            if (branch == TowerBranch.BranchA)
            {
                attackRange += profile.BranchAAttackDelta;
                synergyRange += profile.BranchASynergyDelta;
            }
            else
            {
                attackRange += profile.BranchBAttackDelta;
                synergyRange += profile.BranchBSynergyDelta;
            }
        }

        attackRange = ScaleAttackRange(Mathf.Clamp(attackRange, 1.8f, 6.5f));
        synergyRange = safeLevel >= SynergyUnlockLevel
            ? FinalizeSynergyRange(synergyRange)
            : 0f;
    }

    static float ScaleAttackRange(float value) => value * GlobalAttackRangeScale;

    static float FinalizeSynergyRange(float value) =>
        Mathf.Clamp(value, SynergyMinRange, 4.2f);

    static Profile GetProfile(TowerType type)
    {
        return type switch
        {
            TowerType.Arrow => new Profile
            {
                BaseAttack = 3.2f,
                AttackPerLevel = 0.1f,
                BaseSynergy = 2.4f,
                SynergyPerLevel = 0.08f,
                BranchAAttackDelta = 0.25f,
                BranchASynergyDelta = 0.35f,
                BranchBAttackDelta = 1.15f,
                BranchBSynergyDelta = -0.4f
            },
            TowerType.Frost => new Profile
            {
                BaseAttack = 2.45f,
                AttackPerLevel = 0.08f,
                BaseSynergy = 2.35f,
                SynergyPerLevel = 0.08f,
                BranchAAttackDelta = 0f,
                BranchASynergyDelta = 0.3f,
                BranchBAttackDelta = -0.15f,
                BranchBSynergyDelta = -0.2f
            },
            TowerType.Cannon => new Profile
            {
                BaseAttack = 3.15f,
                AttackPerLevel = 0.06f,
                BaseSynergy = 2.45f,
                SynergyPerLevel = 0.07f,
                BranchAAttackDelta = 0f,
                BranchASynergyDelta = 0.25f,
                BranchBAttackDelta = 0.75f,
                BranchBSynergyDelta = -0.25f
            },
            TowerType.Arcane => new Profile
            {
                BaseAttack = 3.25f,
                AttackPerLevel = 0.08f,
                BaseSynergy = 2.5f,
                SynergyPerLevel = 0.09f,
                BranchAAttackDelta = -0.2f,
                BranchASynergyDelta = 0.35f,
                BranchBAttackDelta = 0f,
                BranchBSynergyDelta = -0.3f
            },
            _ => new Profile
            {
                BaseAttack = 3.2f,
                AttackPerLevel = 0.08f,
                BaseSynergy = 2.5f,
                SynergyPerLevel = 0.08f
            }
        };
    }
}
