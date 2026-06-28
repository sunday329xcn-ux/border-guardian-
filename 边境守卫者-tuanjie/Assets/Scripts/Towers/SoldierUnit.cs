using System.Collections.Generic;
using UnityEngine;

public static class SoldierRegistry
{
    static readonly List<SoldierUnit> soldiers = new();
    static readonly List<SoldierUnit> snapshotBufferA = new();
    static readonly List<SoldierUnit> snapshotBufferB = new();
    static int snapshotVersion;

    public static IReadOnlyList<SoldierUnit> ActiveSoldiers => soldiers;

    public static IReadOnlyList<SoldierUnit> ActiveSoldiersSnapshot =>
        RegistrySnapshot.Copy(soldiers, snapshotBufferA, snapshotBufferB, ref snapshotVersion);

    public static void Register(SoldierUnit soldier)
    {
        if (soldier != null && !soldiers.Contains(soldier))
            soldiers.Add(soldier);
    }

    public static void Unregister(SoldierUnit soldier)
    {
        if (soldier != null)
            soldiers.Remove(soldier);
    }
}

public class SoldierUnit : MonoBehaviour
{
    BarracksTower owner;
    SpriteRenderer spriteRenderer;
    int maxHealth;
    int currentHealth;
    int armor;
    int minDamage;
    int maxDamage;
    float attackInterval = 1f;
    float attackCooldown;
    float moveSpeed = 2.2f;
    Vector3 holdPosition;
    EnemyBase engagedEnemy;
    float lifetimeRemaining = -1f;

    public bool IsAlive => currentHealth > 0;

    public static SoldierUnit Spawn(BarracksTower barracks, Vector3 position, Color color, int hp, int armorValue, int minDmg, int maxDmg, float respawnHint = 1f)
    {
        var go = new GameObject("Soldier");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.42f;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = MapGridControllerShared.GetWhiteSprite();
        renderer.color = color;
        renderer.sortingOrder = VisualSorting.Soldiers;

        UnitVisualDecorator.Attach(go, new Vector2(0.9f, 0.4f), enableBob: false);
        var head = VisualPrimitives.Add(go.transform, "Head", VisualShape.Circle,
            Color.Lerp(color, Color.white, 0.5f), new Vector2(0.55f, 0.55f), new Vector2(0f, 0.32f),
            VisualSorting.Soldiers);
        head.transform.localPosition = new Vector3(0f, 0.32f, -0.05f);

        go.AddComponent<CircleCollider2D>().radius = 0.35f;

        var soldier = go.AddComponent<SoldierUnit>();
        soldier.owner = barracks;
        soldier.spriteRenderer = renderer;
        soldier.maxHealth = hp;
        soldier.currentHealth = hp;
        soldier.armor = armorValue;
        soldier.minDamage = minDmg;
        soldier.maxDamage = maxDmg;
        soldier.holdPosition = position;
        soldier.attackInterval = respawnHint;
        return soldier;
    }

    void OnEnable() => SoldierRegistry.Register(this);

    void OnDisable()
    {
        SoldierRegistry.Unregister(this);
        ReleaseEngagedEnemy();
    }

    void ReleaseEngagedEnemy()
    {
        if (engagedEnemy != null && !engagedEnemy.IsDead)
            engagedEnemy.SetBlocked(false);

        engagedEnemy = null;
    }

    public void SetHoldPosition(Vector3 position)
    {
        holdPosition = position;
    }

    /// <summary>Marks this soldier as temporary; it auto-expires after the given seconds (hero Reinforce, P4.1).</summary>
    public void SetTemporaryLifetime(float seconds)
    {
        lifetimeRemaining = Mathf.Max(0f, seconds);
    }

    void Update()
    {
        if (!IsAlive)
            return;

        if (lifetimeRemaining >= 0f)
        {
            lifetimeRemaining -= Time.deltaTime;
            if (lifetimeRemaining <= 0f)
            {
                Die();
                return;
            }
        }

        if (engagedEnemy == null || engagedEnemy.IsDead)
            engagedEnemy = FindNearestEnemyInRange(1.1f);

        if (engagedEnemy != null)
        {
            engagedEnemy.SetBlocked(true);
            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0f)
            {
                engagedEnemy.TakeDamage(Random.Range(minDamage, maxDamage + 1), DamageType.Physical, damageSource: transform.position);
                attackCooldown = attackInterval;
            }
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, holdPosition, moveSpeed * Time.deltaTime);
    }

    EnemyBase FindNearestEnemyInRange(float radius)
    {
        EnemyBase best = null;
        var bestDist = radius;

        foreach (var enemy in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (enemy == null || enemy.IsDead || !enemy.CanBeBlockedBySoldiers())
                continue;

            var dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist > bestDist)
                continue;

            bestDist = dist;
            best = enemy;
        }

        return best;
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive || amount <= 0)
            return;

        var final = DamageCalculator.CalculatePhysicalDamage(amount, GetEffectiveArmor());
        currentHealth -= final;
        if (currentHealth <= 0)
            Die();
    }

    int GetEffectiveArmor()
    {
        if (owner == null)
            return armor;

        var wardBonus = TowerSynergyService.CountNearby(owner, TowerType.Arcane, owner.SynergyRange) * 3;
        return armor + wardBonus;
    }

    void Die()
    {
        var deathPosition = transform.position;
        ReleaseEngagedEnemy();

        if (owner != null)
            owner.NotifySoldierDeath(this, deathPosition);

        Destroy(gameObject);
    }
}
