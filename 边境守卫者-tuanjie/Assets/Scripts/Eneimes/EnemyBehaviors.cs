using UnityEngine;

public abstract class EnemyBehaviorBase : MonoBehaviour
{
    protected EnemyBase enemy;

    protected virtual void Awake()
    {
        enemy = GetComponent<EnemyBase>();
    }

    public virtual void OnEnemyDeath(bool killedByPlayer) { }
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
