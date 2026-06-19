using System;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public static event Action<EnemyBase> OnPriorityEnemySpawned;
    public static event Action<EnemyBase, string> OnEnemyPresentation;
    [SerializeField] int maxHealth = 60;
    [SerializeField] int armor = 0;
    [SerializeField] int magicResist = 0;
    [SerializeField] bool isFlying = false;
    [SerializeField] int leakDamage = 1;
    [SerializeField] int stealGoldOnLeak = 0;
    [SerializeField] int goldReward = 1;
    [SerializeField] int diamondReward = 0;
    [SerializeField] float moveSpeed = 2.5f;
    [SerializeField] bool isElite = false;
    [SerializeField] bool isBoss = false;
    [SerializeField] bool ignoresBarracksBlock = false;
    [SerializeField] Color enemyColor = new Color(0.9f, 0.25f, 0.25f);

    PathFollower pathFollower;
    SpriteRenderer spriteRenderer;
    WaypointPath spawnPath;
    int currentHealth;
    int currentArmor;
    int currentMagicResist;
    int magicResistReduction;
    float magicResistReductionEndTime;
    float stunEndTime;
    bool isDead;
    bool isBlocked;
    bool isInvulnerable;
    float rootEndTime;

    public EnemyType EnemyType { get; private set; }
    public WaypointPath SpawnPath => spawnPath;
    public PathFollower PathFollower => pathFollower;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int Armor => currentArmor;
    public int MagicResist => magicResist;
    public bool IsFlying => isFlying;
    public bool IsDead => isDead;
    public bool IsBlocked => isBlocked;
    public bool IsElite => isElite;
    public bool IsBoss => isBoss;
    public bool IgnoresBarracksBlock => ignoresBarracksBlock;
    public bool IsInvulnerable => isInvulnerable;
    public bool IsRooted => Time.time < rootEndTime;
    public bool IsStunned => Time.time < stunEndTime;
    public float PathProgress => pathFollower != null ? pathFollower.PathProgress : 0f;

    public void SetBlocked(bool blocked)
    {
        if (ignoresBarracksBlock)
            return;

        isBlocked = blocked;
    }

    public static EnemyBase Spawn(EnemyType type, WaypointPath path, Vector3 position)
    {
        var stats = EnemyCatalog.Get(type);
        var enemyObject = new GameObject(EnemyCatalog.GetDisplayName(type));
        enemyObject.transform.position = position;
        enemyObject.transform.localScale = Vector3.one * stats.Scale;

        var renderer = enemyObject.AddComponent<SpriteRenderer>();
        renderer.sprite = MapGridControllerShared.GetWhiteSprite();
        renderer.sortingOrder = 5;

        var enemy = enemyObject.AddComponent<EnemyBase>();
        enemy.EnemyType = type;
        enemy.spriteRenderer = renderer;
        enemy.ApplyStats(stats);
        enemy.EnsureSelectionCollider(stats.Scale);
        enemy.Initialize(path);
        EnemyCatalog.AttachBehavior(enemy, type);
        if (stats.IsElite || stats.IsBoss)
            OnPriorityEnemySpawned?.Invoke(enemy);

        return enemy;
    }

    public void RaisePresentation(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        OnEnemyPresentation?.Invoke(this, message);
    }

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        EnemyRegistry.Register(this);
    }

    void OnDisable()
    {
        EnemyRegistry.Unregister(this);
    }

    void ApplyStats(EnemyStats stats)
    {
        maxHealth = stats.MaxHealth;
        currentArmor = stats.Armor;
        armor = stats.Armor;
        magicResist = stats.MagicResist;
        currentMagicResist = stats.MagicResist;
        isFlying = stats.IsFlying;
        moveSpeed = stats.MoveSpeed;
        goldReward = stats.GoldReward;
        diamondReward = stats.DiamondReward;
        leakDamage = stats.LeakDamage;
        stealGoldOnLeak = stats.StealGoldOnLeak;
        isElite = stats.IsElite;
        isBoss = stats.IsBoss;
        ignoresBarracksBlock = stats.IgnoresBarracksBlock;
        enemyColor = stats.Color;

        if (spriteRenderer != null)
            spriteRenderer.color = enemyColor;
    }

    public void Initialize(WaypointPath path)
    {
        spawnPath = path;
        currentHealth = maxHealth;
        pathFollower = gameObject.AddComponent<PathFollower>();
        pathFollower.OnPathComplete += HandleReachedGoal;
        pathFollower.Begin(path, moveSpeed);

        if (GetComponent<EnemySlowEffect>() == null)
            gameObject.AddComponent<EnemySlowEffect>();
    }

    public void SetFlying(bool flying)
    {
        isFlying = flying;
    }

    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;

        if (spriteRenderer != null)
            spriteRenderer.color = invulnerable ? Color.Lerp(enemyColor, Color.white, 0.45f) : enemyColor;
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public void ApplyRoot(float duration)
    {
        if (isDead || duration <= 0f)
            return;

        rootEndTime = Mathf.Max(rootEndTime, Time.time + duration);
    }

    public void AddArmor(int amount)
    {
        if (amount <= 0)
            return;

        currentArmor += amount;
    }

    public void ApplyStun(float duration)
    {
        if (isDead || duration <= 0f)
            return;

        stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);
    }

    public void ApplyMagicResistReduction(int amount, float duration)
    {
        if (isDead || amount <= 0 || duration <= 0f)
            return;

        magicResistReduction = Mathf.Max(magicResistReduction, amount);
        magicResistReductionEndTime = Mathf.Max(magicResistReductionEndTime, Time.time + duration);
    }

    public bool IsSlowed()
    {
        var slow = GetComponent<EnemySlowEffect>();
        return slow != null && slow.SpeedMultiplier < 0.99f;
    }

    public bool HasReducedMagicResist()
    {
        RefreshMagicResistReduction();
        return magicResistReduction > 0;
    }

    public int GetEffectiveMagicResist()
    {
        RefreshMagicResistReduction();
        return Mathf.Max(0, currentMagicResist - magicResistReduction);
    }

    void RefreshMagicResistReduction()
    {
        if (Time.time < magicResistReductionEndTime)
            return;

        magicResistReduction = 0;
        magicResistReductionEndTime = 0f;
    }

    public void SetSelectedVisual(bool selected)
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = selected
            ? Color.Lerp(enemyColor, Color.white, 0.35f)
            : isInvulnerable ? Color.Lerp(enemyColor, Color.white, 0.45f) : enemyColor;
    }

    public string GetArmorRating()
    {
        if (Armor >= 25)
            return "Very High";

        if (Armor >= 15)
            return "High";

        if (Armor >= 6)
            return "Medium";

        return Armor > 0 ? "Low" : "None";
    }

    public string GetMagicResistRating()
    {
        if (MagicResist >= 40)
            return "Very High";

        if (MagicResist >= 20)
            return "High";

        if (MagicResist >= 8)
            return "Medium";

        return MagicResist > 0 ? "Low" : "None";
    }

    public string GetAttackRating()
    {
        return EnemyCatalog.GetThreatRating(EnemyType, leakDamage, stealGoldOnLeak, isBoss, isElite);
    }

    void EnsureSelectionCollider(float scale)
    {
        var collider = GetComponent<CircleCollider2D>();
        if (collider == null)
            collider = gameObject.AddComponent<CircleCollider2D>();

        collider.radius = Mathf.Max(0.28f, scale * 0.55f);
        collider.isTrigger = true;
    }

    public void TakeDamage(int baseDamage, DamageType damageType, float armorPenetration = 0f)
    {
        if (isDead || baseDamage <= 0 || isInvulnerable)
            return;

        var effectiveArmor = Mathf.Max(0, currentArmor - Mathf.RoundToInt(armorPenetration));
        var finalDamage = damageType switch
        {
            DamageType.Physical => DamageCalculator.CalculatePhysicalDamage(baseDamage, effectiveArmor),
            DamageType.Magic => DamageCalculator.CalculateMagicDamage(baseDamage, GetEffectiveMagicResist()),
            DamageType.True => baseDamage,
            _ => baseDamage
        };

        currentHealth -= finalDamage;
        if (currentHealth <= 0)
            Die(false);
    }

    public void ReduceArmor(int amount)
    {
        if (amount <= 0)
            return;

        currentArmor = Mathf.Max(0, currentArmor - amount);
    }

    void HandleReachedGoal()
    {
        if (isDead)
            return;

        if (stealGoldOnLeak > 0 && GameManager.Instance != null)
            GameManager.Instance.TryStealGold(stealGoldOnLeak);
        else if (leakDamage > 0 && GameManager.Instance != null)
            GameManager.Instance.TakeDamage(leakDamage);

        Die(true);
    }

    void Die(bool leaked)
    {
        if (isDead)
            return;

        isDead = true;
        EnemySelectionController.DeselectIf(this);

        if (!leaked && GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(goldReward);

            if (diamondReward > 0)
                GameManager.Instance.AddDiamonds(diamondReward);
        }

        foreach (var behavior in GetComponents<EnemyBehaviorBase>())
            behavior.OnEnemyDeath(!leaked);

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (pathFollower != null)
            pathFollower.OnPathComplete -= HandleReachedGoal;
    }
}
