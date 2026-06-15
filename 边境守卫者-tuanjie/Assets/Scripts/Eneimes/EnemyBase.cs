using UnityEngine;

public class EnemyBase : MonoBehaviour
{
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
    bool isDead;
    bool isBlocked;
    bool isInvulnerable;

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
        enemy.Initialize(path);
        EnemyCatalog.AttachBehavior(enemy, type);
        return enemy;
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

    public void AddArmor(int amount)
    {
        if (amount <= 0)
            return;

        currentArmor += amount;
    }

    public void TakeDamage(int baseDamage, DamageType damageType, float armorPenetration = 0f)
    {
        if (isDead || baseDamage <= 0 || isInvulnerable)
            return;

        var effectiveArmor = Mathf.Max(0, currentArmor - Mathf.RoundToInt(armorPenetration));
        var finalDamage = damageType switch
        {
            DamageType.Physical => DamageCalculator.CalculatePhysicalDamage(baseDamage, effectiveArmor),
            DamageType.Magic => DamageCalculator.CalculateMagicDamage(baseDamage, magicResist),
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
