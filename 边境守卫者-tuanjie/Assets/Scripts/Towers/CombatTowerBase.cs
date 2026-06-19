using UnityEngine;

public abstract class CombatTowerBase : TowerBase
{
    protected int minDamage;
    protected int maxDamage;
    protected float critChance;
    protected float armorPenetration;
    protected float splashRadius;
    protected int splashMaxTargets = 1;
    protected float stunDuration;
    protected float groundZoneRadius;
    protected float groundZoneDuration;
    protected float groundZoneSlow;
    protected float groundZoneDps;
    protected bool prioritizeLowestHealth;
    protected bool prioritizeHighestHealth;
    protected bool pierceLine;
    protected float pierceArmorBonus;
    protected float freezeChance;
    protected float freezeDuration;
    protected int armorStealPerHit;
    protected int stackedBonusDamage;
    protected float stackedBonusExpireTime;
    protected float voidPulseCooldown;
    protected float voidPulseTimer;

    protected float thunderArmorShred;
    protected int destroyerAllyBonus;
    protected int wardDamagePerBarracks;
    protected int wardArmorPerArcane;
    protected float breachMagicResistShred;
    protected float breachMagicResistDuration;
    protected float breachArcaneBonus;
    protected float shatterBonus;
    protected float volleyBonus;

    float attackCooldown;

    protected override void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (Time.time > stackedBonusExpireTime)
            stackedBonusDamage = 0;

        voidPulseTimer -= Time.deltaTime;

        attackCooldown -= Time.deltaTime;
        if (attackCooldown > 0f)
            return;

        var target = AcquireTarget();
        if (target == null)
            return;

        PerformAttack(target);
        attackCooldown = attackInterval;
    }

    protected virtual EnemyBase AcquireTarget()
    {
        if (prioritizeLowestHealth)
            return FindTargetLowestHealth();

        if (prioritizeHighestHealth)
            return FindTargetHighestHealth();

        return FindTargetByProgress();
    }

    protected EnemyBase FindTargetByProgress()
    {
        EnemyBase bestTarget = null;
        var bestProgress = float.MinValue;
        var rangeSqr = range * range;

        foreach (var enemy in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (!IsValidTarget(enemy, rangeSqr))
                continue;

            if (enemy.PathProgress <= bestProgress)
                continue;

            bestProgress = enemy.PathProgress;
            bestTarget = enemy;
        }

        return bestTarget;
    }

    protected EnemyBase FindTargetLowestHealth()
    {
        EnemyBase bestTarget = null;
        var lowestHealth = int.MaxValue;
        var rangeSqr = range * range;

        foreach (var enemy in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (!IsValidTarget(enemy, rangeSqr))
                continue;

            if (enemy.CurrentHealth >= lowestHealth)
                continue;

            lowestHealth = enemy.CurrentHealth;
            bestTarget = enemy;
        }

        return bestTarget;
    }

    protected EnemyBase FindTargetHighestHealth()
    {
        EnemyBase bestTarget = null;
        var highestHealth = int.MinValue;
        var rangeSqr = range * range;

        foreach (var enemy in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (!IsValidTarget(enemy, rangeSqr))
                continue;

            if (enemy.CurrentHealth <= highestHealth)
                continue;

            highestHealth = enemy.CurrentHealth;
            bestTarget = enemy;
        }

        return bestTarget;
    }

    bool IsValidTarget(EnemyBase enemy, float rangeSqr)
    {
        if (enemy == null || enemy.IsDead)
            return false;

        if (!canTargetFlying && enemy.IsFlying)
            return false;

        var offset = enemy.transform.position - transform.position;
        return offset.sqrMagnitude <= rangeSqr;
    }

    protected virtual void PerformAttack(EnemyBase target)
    {
        if (pierceLine)
        {
            PerformLineAttack(target);
            return;
        }

        if (splashRadius > 0.1f)
        {
            PerformSplashAttack(target);
            return;
        }

        if (level >= 5 && branch == TowerBranch.BranchB && TowerType == TowerType.Arcane)
        {
            DealSingleShot(target);
            TriggerVoidPulse();
            return;
        }

        DealSingleShot(target);
    }

    void PerformLineAttack(EnemyBase primary)
    {
        var direction = (primary.transform.position - transform.position).normalized;
        foreach (var enemy in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            if (!canTargetFlying && enemy.IsFlying)
                continue;

            var toEnemy = enemy.transform.position - transform.position;
            if (toEnemy.magnitude > range)
                continue;

            var alignment = Vector2.Dot(direction, toEnemy.normalized);
            if (alignment < 0.85f)
                continue;

            var damage = RollDamage();
            if (enemy.Armor > 0)
                damage = Mathf.RoundToInt(damage * (1f + pierceArmorBonus));

            damage = ApplyOutgoingDamageSynergies(damage, enemy);
            enemy.TakeDamage(damage, DamageType.Physical, armorPenetration);
            ApplyOnHitEffects(enemy);
        }
    }

    void PerformSplashAttack(EnemyBase primary)
    {
        var hitCount = 0;
        foreach (var enemy in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            if (Vector2.Distance(enemy.transform.position, primary.transform.position) > splashRadius)
                continue;

            DealSingleShot(enemy, false);
            hitCount++;
            if (hitCount >= splashMaxTargets)
                break;
        }

        SpawnGroundEffect(primary.transform.position);
    }

    void DealSingleShot(EnemyBase target, bool spawnZone = true)
    {
        if (level >= 5 && branch == TowerBranch.BranchA && TowerType == TowerType.Arrow && Random.value < 0.35f)
        {
            for (int i = 0; i < 3; i++)
                ApplyDamageToEnemy(target);

            return;
        }

        ApplyDamageToEnemy(target);

        if (spawnZone)
            SpawnGroundEffect(target.transform.position);
    }

    void ApplyDamageToEnemy(EnemyBase target)
    {
        var damage = RollDamage() + stackedBonusDamage;
        var damageType = GetDamageType();

        if (critChance > 0f && Random.value < critChance)
            damage *= 2;

        damage = ApplyOutgoingDamageSynergies(damage, target);

        if (freezeChance > 0f && Random.value < freezeChance)
        {
            var slow = target.GetComponent<EnemySlowEffect>();
            if (slow != null)
                slow.ApplySlow(1f, freezeDuration);
        }

        target.TakeDamage(damage, damageType, armorPenetration);

        if (stunDuration > 0f)
            target.ApplyStun(stunDuration);

        if (armorStealPerHit > 0)
        {
            stackedBonusDamage += armorStealPerHit;
            stackedBonusExpireTime = Time.time + 5f;
            target.ReduceArmor(armorStealPerHit);
            ShareDestroyerBonus();
        }

        ApplyOnHitEffects(target);
        ApplySlowFromTower(target);
    }

    void ShareDestroyerBonus()
    {
        if (destroyerAllyBonus <= 0)
            return;

        foreach (var tower in TowerRegistry.ActiveTowersSnapshot)
        {
            if (tower == null || tower == this || tower is not CombatTowerBase combatAlly)
                continue;

            if (Vector2.Distance(transform.position, combatAlly.transform.position) > TowerSynergyCatalog.DefaultRange)
                continue;

            combatAlly.AddTemporaryDamageBonus(destroyerAllyBonus, 5f);
        }
    }

    void ApplyOnHitEffects(EnemyBase target)
    {
        if (target == null || target.IsDead)
            return;

        if (thunderArmorShred > 0f)
            target.ReduceArmor(Mathf.RoundToInt(thunderArmorShred));

        if (breachMagicResistShred > 0f &&
            TowerSynergyService.HasPartner(this, TowerType.Arcane, TowerSynergyCatalog.DefaultRange))
        {
            target.ApplyMagicResistReduction(
                Mathf.RoundToInt(breachMagicResistShred),
                breachMagicResistDuration);
        }
    }

    int ApplyOutgoingDamageSynergies(int damage, EnemyBase target)
    {
        if (target == null)
            return damage;

        if (TowerType == TowerType.Cannon &&
            shatterBonus > 0f &&
            target.IsSlowed() &&
            TowerSynergyService.HasPartner(this, TowerType.Frost, TowerSynergyCatalog.DefaultRange))
        {
            damage = Mathf.RoundToInt(damage * (1f + shatterBonus));
        }

        if (TowerType == TowerType.Arrow &&
            volleyBonus > 0f &&
            target.IsSlowed() &&
            TowerSynergyService.HasPartner(this, TowerType.Frost, TowerSynergyCatalog.DefaultRange))
        {
            damage = Mathf.RoundToInt(damage * (1f + volleyBonus));
        }

        if (TowerType == TowerType.Arcane &&
            breachArcaneBonus > 0f &&
            target.HasReducedMagicResist() &&
            TowerSynergyService.HasPartner(this, TowerType.Cannon, TowerSynergyCatalog.DefaultRange))
        {
            damage = Mathf.RoundToInt(damage * (1f + breachArcaneBonus));
        }

        return damage;
    }

    public void AddTemporaryDamageBonus(int amount, float duration)
    {
        if (amount <= 0 || duration <= 0f)
            return;

        stackedBonusDamage += amount;
        stackedBonusExpireTime = Mathf.Max(stackedBonusExpireTime, Time.time + duration);
    }

    int RollDamage()
    {
        var damage = Random.Range(minDamage, maxDamage + 1);

        if (TowerType == TowerType.Arcane && wardDamagePerBarracks > 0)
        {
            damage += wardDamagePerBarracks *
                      TowerSynergyService.CountNearby(this, TowerType.Barracks, TowerSynergyCatalog.DefaultRange);
        }

        return damage;
    }

    DamageType GetDamageType()
    {
        return TowerType is TowerType.Frost or TowerType.Arcane ? DamageType.Magic : DamageType.Physical;
    }

    void ApplySlowFromTower(EnemyBase target)
    {
        if (TowerType != TowerType.Frost || groundZoneSlow <= 0f)
            return;

        var slow = target.GetComponent<EnemySlowEffect>();
        if (slow != null)
            slow.ApplySlow(groundZoneSlow, 1.5f);
    }

    void SpawnGroundEffect(Vector3 position)
    {
        if (groundZoneRadius <= 0f || groundZoneDuration <= 0f)
            return;

        if (groundZoneDps > 0f)
            TowerGroundZone.SpawnFire(position, groundZoneRadius, groundZoneDuration, groundZoneDps, groundZoneSlow);
        else
            TowerGroundZone.Spawn(position, groundZoneRadius, groundZoneDuration, groundZoneSlow);
    }

    void TriggerVoidPulse()
    {
        if (voidPulseTimer > 0f)
            return;

        voidPulseTimer = voidPulseCooldown;
        foreach (var enemy in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            if (Vector2.Distance(enemy.transform.position, transform.position) > range)
                continue;

            var toCenter = transform.position - enemy.transform.position;
            enemy.transform.position += toCenter.normalized * 0.35f;
            var pulseDamage = ApplyOutgoingDamageSynergies(12, enemy);
            enemy.TakeDamage(pulseDamage, DamageType.True);
        }
    }

    protected void ConfigureSynergyDefaults()
    {
        shatterBonus = 0.3f;
        volleyBonus = 0.25f;
        wardDamagePerBarracks = 4;
        wardArmorPerArcane = 4;
        breachMagicResistShred = 10f;
        breachMagicResistDuration = 4f;
        breachArcaneBonus = 0.15f;
        destroyerAllyBonus = 0;
        thunderArmorShred = 0f;
    }
}
