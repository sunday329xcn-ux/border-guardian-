using System.Collections;
using UnityEngine;

/// <summary>
/// Movable hero unit (P4.1). Commanded with right-click, auto-engages the
/// nearest blockable enemy in melee range, has limited HP, dies and revives on
/// a long cooldown, and gains XP from player kills to level up (stronger melee,
/// HP and aura). The passive aura is exposed via <see cref="HeroAuraService"/>.
/// </summary>
public class HeroUnit : MonoBehaviour
{
    public const float ReviveCooldown = 25f;

    SpriteRenderer body;
    SpriteRenderer shadow;

    int maxHealth;
    int currentHealth;
    int level = 1;
    int experience;
    int xpToNext;

    float moveSpeed = 3.2f;
    Vector3 destination;
    Vector3 spawnPoint;

    int minDamage;
    int maxDamage;
    float attackInterval = 0.8f;
    float attackCooldown;
    const float EngageRange = 1.0f;
    EnemyBase engagedEnemy;
    float contactDamageAccumulator;

    bool isDead;
    float reviveAt;
    Coroutine flashRoutine;

    public bool IsAlive => !isDead;
    public bool IsDead => isDead;
    public int Level => level;
    public int CurrentHealth => Mathf.Max(0, currentHealth);
    public int MaxHealth => maxHealth;
    public int Experience => experience;
    public int ExperienceToNext => xpToNext;
    public float ReviveRemaining => isDead ? Mathf.Max(0f, reviveAt - Time.time) : 0f;
    public float AuraRadius => 1.5f * MapGridSettings.CellSize;
    public float AuraDamageBonus => 0.05f + 0.01f * (level - 1) + RoguelikeModifierService.HeroAuraBonus;

    public static HeroUnit Spawn(Vector3 position)
    {
        position.z = 0f;
        var go = new GameObject("Hero");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.6f;

        var hero = go.AddComponent<HeroUnit>();
        hero.spawnPoint = position;
        hero.destination = position;

        var shadowObj = new GameObject("HeroShadow");
        shadowObj.transform.SetParent(go.transform, false);
        shadowObj.transform.localPosition = new Vector3(0f, -0.42f, 0f);
        shadowObj.transform.localScale = new Vector3(1.1f, 0.5f, 1f);
        hero.shadow = shadowObj.AddComponent<SpriteRenderer>();
        hero.shadow.sprite = ProceduralSpriteFactory.GetSoftShadowSprite();
        hero.shadow.color = VisualPalette.UnitShadow;
        hero.shadow.sortingOrder = VisualSorting.Shadows;

        var bodyRenderer = go.AddComponent<SpriteRenderer>();
        bodyRenderer.sprite = ProceduralSpriteFactory.GetCircleSprite();
        bodyRenderer.color = VisualPalette.Hero;
        bodyRenderer.sortingOrder = VisualSorting.Soldiers + 1;
        hero.body = bodyRenderer;

        hero.level = 1 + TalentService.HeroStartLevelBonus;
        hero.ApplyLevelStats();
        hero.currentHealth = hero.maxHealth;
        hero.xpToNext = hero.ComputeXpToNext();

        HeroAuraService.SetHero(hero);
        return hero;
    }

    void ApplyLevelStats()
    {
        maxHealth = 300 + 60 * (level - 1);
        minDamage = 12 + 3 * (level - 1);
        maxDamage = 18 + 4 * (level - 1);
        attackInterval = Mathf.Max(0.5f, 0.8f - 0.03f * (level - 1));
    }

    int ComputeXpToNext() => 20 + (level - 1) * 15;

    public void SetDestination(Vector3 worldPoint)
    {
        if (isDead)
            return;

        worldPoint.z = 0f;
        destination = worldPoint;
        ReleaseEngaged();
    }

    public void AddExperience(int amount)
    {
        if (isDead || amount <= 0)
            return;

        experience += amount;
        while (experience >= xpToNext)
        {
            experience -= xpToNext;
            level++;
            ApplyLevelStats();
            currentHealth = maxHealth;
            xpToNext = ComputeXpToNext();
        }
    }

    void Update()
    {
        if (isDead)
        {
            if (Time.time >= reviveAt)
                Revive();
            return;
        }

        if (engagedEnemy == null || engagedEnemy.IsDead)
            engagedEnemy = FindEnemyInRange(EngageRange);

        if (engagedEnemy != null && !engagedEnemy.IsDead)
        {
            engagedEnemy.SetBlocked(true);
            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0f)
            {
                engagedEnemy.TakeDamage(Random.Range(minDamage, maxDamage + 1),
                    DamageType.Physical, damageSource: transform.position);
                attackCooldown = attackInterval;
            }

            TakeContactDamage(engagedEnemy);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
        }
    }

    void TakeContactDamage(EnemyBase enemy)
    {
        var dps = enemy.IsBoss ? 22f : enemy.IsElite ? 12f : 6f;
        contactDamageAccumulator += dps * Time.deltaTime;
        if (contactDamageAccumulator < 1f)
            return;

        var dmg = Mathf.FloorToInt(contactDamageAccumulator);
        contactDamageAccumulator -= dmg;
        ApplyDamage(dmg);
    }

    void ApplyDamage(int amount)
    {
        if (isDead || amount <= 0)
            return;

        currentHealth -= amount;
        PlayHitFlash();

        if (currentHealth <= 0)
            Die();
    }

    void PlayHitFlash()
    {
        if (body == null || !gameObject.activeInHierarchy)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        body.color = VisualPalette.HitFlash;
        yield return new WaitForSecondsRealtime(0.05f);
        if (body != null)
            body.color = VisualPalette.Hero;
        flashRoutine = null;
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        currentHealth = 0;
        ReleaseEngaged();
        reviveAt = Time.time + ReviveCooldown;

        if (body != null)
            body.enabled = false;
        if (shadow != null)
            shadow.enabled = false;
    }

    void Revive()
    {
        isDead = false;
        transform.position = spawnPoint;
        destination = spawnPoint;
        currentHealth = maxHealth;

        if (body != null)
        {
            body.enabled = true;
            body.color = VisualPalette.Hero;
        }

        if (shadow != null)
            shadow.enabled = true;
    }

    void ReleaseEngaged()
    {
        if (engagedEnemy != null && !engagedEnemy.IsDead)
            engagedEnemy.SetBlocked(false);

        engagedEnemy = null;
    }

    EnemyBase FindEnemyInRange(float radius)
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

    void OnDisable()
    {
        ReleaseEngaged();
    }

    void OnDestroy()
    {
        HeroAuraService.ClearHero(this);
    }
}
