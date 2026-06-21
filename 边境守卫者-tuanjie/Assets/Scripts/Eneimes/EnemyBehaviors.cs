using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBehaviorBase : MonoBehaviour
{
    protected EnemyBase enemy;

    protected virtual void Awake()
    {
        enemy = GetComponent<EnemyBase>();
    }

    public virtual void OnEnemyDeath(bool killedByPlayer) { }

    public virtual bool AllowsTowerTargeting(CombatTowerBase tower) => true;

    public virtual bool AllowsSoldierBlocking() => true;

    public virtual int ModifyPhysicalDamage(int damage, Vector3 damageSource) => damage;
}

public static class EnemyAreaDamage
{
    public static void DamageEnemiesInRadius(Vector3 center, float radius, int damage, DamageType damageType, EnemyBase exclude = null)
    {
        foreach (var target in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (target == null || target.IsDead || target == exclude)
                continue;

            if (Vector2.Distance(target.transform.position, center) > radius)
                continue;

            target.TakeDamage(damage, damageType);
        }
    }

    public static void DamageSoldiersInRadius(Vector3 center, float radius, int damage)
    {
        foreach (var soldier in SoldierRegistry.ActiveSoldiersSnapshot)
        {
            if (soldier == null || !soldier.IsAlive)
                continue;

            if (Vector2.Distance(soldier.transform.position, center) > radius)
                continue;

            soldier.TakeDamage(damage);
        }
    }

    public static void DamageTowersInRadius(Vector3 center, float radius, int damage)
    {
        foreach (var tower in TowerRegistry.ActiveTowersSnapshot)
        {
            if (tower == null)
                continue;

            if (Vector2.Distance(tower.transform.position, center) > radius)
                continue;

            tower.TakeTowerDamage(damage);
        }
    }
}

public class WraithBehavior : EnemyBehaviorBase
{
    const int HealAmount = 30;
    const float HealRadius = 1.6f;

    public override void OnEnemyDeath(bool killedByPlayer)
    {
        if (!killedByPlayer)
            return;

        foreach (var ally in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (ally == null || ally.IsDead || ally == enemy)
                continue;

            if (Vector2.Distance(ally.transform.position, transform.position) > HealRadius)
                continue;

            ally.Heal(HealAmount);
        }
    }
}

public class RockGolemBehavior : EnemyBehaviorBase
{
    const float CycleInterval = 10f;
    const float InvulnerableDuration = 2f;
    const int HealAmount = 50;

    float cycleTimer = 5f;
    float invulnerableTimer;

    void Update()
    {
        if (enemy == null || enemy.IsDead)
            return;

        if (invulnerableTimer > 0f)
        {
            invulnerableTimer -= Time.deltaTime;
            if (invulnerableTimer <= 0f)
                enemy.SetInvulnerable(false);

            return;
        }

        cycleTimer -= Time.deltaTime;
        if (cycleTimer > 0f)
            return;

        cycleTimer = CycleInterval;
        invulnerableTimer = InvulnerableDuration;
        enemy.SetInvulnerable(true);
        enemy.Heal(HealAmount);
    }
}

public class FireBomberBehavior : EnemyBehaviorBase
{
    const int ExplosionDamage = 60;
    const float ExplosionRadius = 1.5f;

    public override void OnEnemyDeath(bool killedByPlayer)
    {
        var center = transform.position;
        EnemyAreaDamage.DamageEnemiesInRadius(center, ExplosionRadius, ExplosionDamage, DamageType.True, enemy);
        EnemyAreaDamage.DamageSoldiersInRadius(center, ExplosionRadius, ExplosionDamage);
        EnemyAreaDamage.DamageTowersInRadius(center, ExplosionRadius, ExplosionDamage);
    }
}

public class ShadowPriestBehavior : EnemyBehaviorBase
{
    const float HealPerSecond = 15f;
    const float HealRadius = 2.5f;
    const float HealInterval = 1f;

    float healTimer;

    void Update()
    {
        if (enemy == null || enemy.IsDead)
            return;

        healTimer -= Time.deltaTime;
        if (healTimer > 0f)
            return;

        healTimer = HealInterval;
        var target = FindHealTarget();
        target?.Heal(Mathf.RoundToInt(HealPerSecond));
    }

    EnemyBase FindHealTarget()
    {
        EnemyBase best = null;
        var bestScore = float.MaxValue;

        foreach (var candidate in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (candidate == null || candidate.IsDead || candidate == enemy)
                continue;

            if (Vector2.Distance(candidate.transform.position, transform.position) > HealRadius)
                continue;

            if (candidate.PathProgress + 0.05f < enemy.PathProgress)
                continue;

            if (candidate.CurrentHealth >= candidate.MaxHealth)
                continue;

            var missingHealth = candidate.MaxHealth - candidate.CurrentHealth;
            var score = candidate.IsElite ? missingHealth * 0.5f : missingHealth;

            if (score >= bestScore)
                continue;

            bestScore = score;
            best = candidate;
        }

        return best;
    }
}

public class WolfRiderBehavior : EnemyBehaviorBase
{
    const float SummonInterval = 15f;
    float summonTimer = 8f;

    void Update()
    {
        if (enemy == null || enemy.IsDead || enemy.SpawnPath == null)
            return;

        summonTimer -= Time.deltaTime;
        if (summonTimer > 0f)
            return;

        summonTimer = SummonInterval;
        var spawnPos = transform.position + Vector3.left * 0.35f;
        EnemyBase.Spawn(EnemyType.Imp, enemy.SpawnPath, spawnPos);
    }
}

public class TowerBreakerBehavior : EnemyBehaviorBase
{
    const float AttackRange = 0.75f;
    const float MoveSpeed = 1.8f;
    const int TowerDamage = 80;
    const float AttackInterval = 2f;

    TowerBase targetTower;
    float attackCooldown;

    void Update()
    {
        if (enemy == null || enemy.IsDead)
            return;

        if (targetTower == null)
            targetTower = FindNearestTower();

        if (targetTower == null)
        {
            enemy.PathFollower?.Resume();
            return;
        }

        enemy.PathFollower?.Pause();
        var towerPos = targetTower.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, towerPos, MoveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, towerPos) > AttackRange)
            return;

        attackCooldown -= Time.deltaTime;
        if (attackCooldown > 0f)
            return;

        attackCooldown = AttackInterval;
        targetTower.TakeTowerDamage(TowerDamage);

        if (targetTower == null)
            targetTower = FindNearestTower();
    }

    TowerBase FindNearestTower()
    {
        TowerBase best = null;
        var bestDist = float.MaxValue;

        foreach (var tower in TowerRegistry.ActiveTowersSnapshot)
        {
            if (tower == null)
                continue;

            var dist = Vector2.Distance(transform.position, tower.transform.position);
            if (dist >= bestDist)
                continue;

            bestDist = dist;
            best = tower;
        }

        return best;
    }
}

public class AncientDragonBehavior : EnemyBehaviorBase
{
    const float Phase2HealthRatio = 0.5f;
    const float FireballInterval = 4f;
    const float FlameInterval = 3f;
    const int FireballDamage = 35;
    const float FireballRadius = 1.2f;
    const int Phase2ArmorBonus = 30;

    bool phaseTwo;
    float fireballTimer = 2f;
    float flameTimer = 1.5f;

    void Update()
    {
        if (enemy == null || enemy.IsDead)
            return;

        if (!phaseTwo && enemy.CurrentHealth <= enemy.MaxHealth * Phase2HealthRatio)
            EnterPhaseTwo();

        if (phaseTwo)
            UpdatePhaseTwo();
        else
            UpdatePhaseOne();
    }

    void EnterPhaseTwo()
    {
        phaseTwo = true;
        enemy.SetFlying(false);
        enemy.AddArmor(Phase2ArmorBonus);
        enemy.RaisePresentation("Phase 2 · Dragon lands!");
    }

    void UpdatePhaseOne()
    {
        fireballTimer -= Time.deltaTime;
        if (fireballTimer > 0f)
            return;

        fireballTimer = FireballInterval;
        CastFireball(transform.position + (Vector3)(Random.insideUnitCircle * 0.8f));
    }

    void UpdatePhaseTwo()
    {
        flameTimer -= Time.deltaTime;
        if (flameTimer > 0f)
            return;

        flameTimer = FlameInterval;
        SpawnFlamePath();
    }

    void CastFireball(Vector3 center)
    {
        EnemyAreaDamage.DamageEnemiesInRadius(center, FireballRadius, FireballDamage, DamageType.Magic);
        EnemyAreaDamage.DamageSoldiersInRadius(center, FireballRadius, FireballDamage);
        EnemyAreaDamage.DamageTowersInRadius(center, FireballRadius, FireballDamage / 2);
    }

    void SpawnFlamePath()
    {
        TowerGroundZone.SpawnFire(transform.position, 0.35f, 2f, 18f);
    }
}

public static class NullifierSuppressionService
{
    static readonly Dictionary<TowerBase, float> suppressUntil = new();

    public static void ApplySuppression(TowerBase tower, float duration)
    {
        if (tower == null || duration <= 0f)
            return;

        var endTime = Time.time + duration;
        if (suppressUntil.TryGetValue(tower, out var existing) && existing > endTime)
            return;

        suppressUntil[tower] = endTime;
    }

    public static bool IsSuppressed(TowerBase tower)
    {
        if (tower == null)
            return false;

        if (!suppressUntil.TryGetValue(tower, out var endTime))
            return false;

        if (Time.time < endTime)
            return true;

        suppressUntil.Remove(tower);
        return false;
    }
}

public class ShieldBearerBehavior : EnemyBehaviorBase
{
    const float FrontDamageMultiplier = 0.3f;
    const float FrontDotThreshold = 0.25f;

    public override int ModifyPhysicalDamage(int damage, Vector3 damageSource)
    {
        if (damage <= 0 || enemy == null)
            return damage;

        var toSource = (damageSource - transform.position).normalized;
        var facing = enemy.GetMovementFacing();
        var dot = Vector2.Dot(facing, toSource);
        return dot >= FrontDotThreshold ? Mathf.Max(1, Mathf.RoundToInt(damage * FrontDamageMultiplier)) : damage;
    }
}

public class SplitSlimeBehavior : EnemyBehaviorBase
{
    bool isMini;

    public void MarkAsMini()
    {
        isMini = true;
        transform.localScale = Vector3.one * 0.38f;
    }

    public override void OnEnemyDeath(bool killedByPlayer)
    {
        if (!killedByPlayer || isMini || enemy == null || enemy.SpawnPath == null)
            return;

        var childHealth = Mathf.Max(1, enemy.MaxHealth / 2);
        var progress = enemy.PathProgress;
        var spawnPos = transform.position;

        for (int i = 0; i < 2; i++)
        {
            var offset = (Vector3)(Random.insideUnitCircle * 0.25f);
            EnemyBase.SpawnSplitChild(EnemyType.SplitSlime, enemy.SpawnPath, spawnPos + offset, progress, childHealth);
        }
    }
}

public class ShadeBehavior : EnemyBehaviorBase
{
    const float RevealPathProgress = 0.72f;
    const int DetectionArrowLevel = 5;

    bool isRevealed;

    public bool IsStealthed => !isRevealed;

    void Update()
    {
        if (enemy == null || enemy.IsDead)
            return;

        var shouldReveal = enemy.PathProgress >= RevealPathProgress;
        var spotterRevealed = SupportTowerService.IsShadeRevealed(enemy);
        enemy.SetStealthVisual(!shouldReveal && !spotterRevealed);

        if (shouldReveal == isRevealed)
            return;

        isRevealed = shouldReveal;
    }

    protected override void Awake()
    {
        base.Awake();
        enemy?.SetStealthVisual(true);
    }

    public override bool AllowsTowerTargeting(CombatTowerBase tower)
    {
        if (isRevealed || tower == null)
            return true;

        if (SupportTowerService.IsShadeRevealed(enemy))
            return true;

        return tower.TowerType == TowerType.Arrow && tower.Level >= DetectionArrowLevel;
    }
}

public class WarDrummerBehavior : EnemyBehaviorBase
{
    const float AuraRadius = 2f;
    const float SpeedBuffPercent = 0.25f;
    const float RefreshInterval = 0.35f;

    float refreshTimer;

    void Update()
    {
        if (enemy == null || enemy.IsDead)
            return;

        refreshTimer -= Time.deltaTime;
        if (refreshTimer > 0f)
            return;

        refreshTimer = RefreshInterval;
        foreach (var ally in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (ally == null || ally.IsDead || ally == enemy)
                continue;

            if (Vector2.Distance(ally.transform.position, transform.position) > AuraRadius)
                continue;

            var speedEffect = ally.GetComponent<EnemySlowEffect>() ?? ally.gameObject.AddComponent<EnemySlowEffect>();
            speedEffect.ApplySpeedBuff(SpeedBuffPercent, RefreshInterval + 0.1f);
        }
    }
}

public class NullifierBehavior : EnemyBehaviorBase
{
    const float SuppressionRadius = 2.8f;
    const float SuppressionDuration = 5f;
    const float RefreshInterval = 0.5f;

    float refreshTimer;

    protected override void Awake()
    {
        base.Awake();
        enemy?.RaisePresentation("Nullifier · synergy jam!");
    }

    void Update()
    {
        if (enemy == null || enemy.IsDead)
            return;

        refreshTimer -= Time.deltaTime;
        if (refreshTimer > 0f)
            return;

        refreshTimer = RefreshInterval;
        foreach (var tower in TowerRegistry.ActiveTowersSnapshot)
        {
            if (tower == null || !tower.HasSynergyUnlocked)
                continue;

            if (Vector2.Distance(tower.transform.position, transform.position) > SuppressionRadius)
                continue;

            NullifierSuppressionService.ApplySuppression(tower, SuppressionDuration);
        }
    }
}

public class BatSwarmBehavior : EnemyBehaviorBase
{
}
