using System;
using UnityEngine;

public abstract class TowerBase : MonoBehaviour
{
    public static event Action<TowerBase, int> OnTowerDamaged;
    public const int MaxLevel = 5;

    [SerializeField] protected float range = 3.5f;
    [SerializeField] protected float attackInterval = 0.6f;
    [SerializeField] protected bool canTargetFlying = true;

    protected BuildSlot occupiedSlot;
    protected SpriteRenderer spriteRenderer;
    protected Color normalColor = Color.white;
    protected int level = 1;
    protected int totalGoldSpent;
    protected int totalDiamondSpent;
    protected TowerBranch branch = TowerBranch.None;
    protected int maxTowerHealth = 50;
    protected int currentTowerHealth;
    protected float synergyRange = 2.5f;

    public TowerType TowerType { get; protected set; }
    public int Level => level;
    public TowerBranch Branch => branch;
    public int TotalGoldSpent => totalGoldSpent;
    public int TotalDiamondSpent => totalDiamondSpent;
    public BuildSlot OccupiedSlot => occupiedSlot;
    public float Range => range;
    public float SynergyRange => synergyRange > 0.01f
        ? synergyRange + RoguelikeModifierService.SynergyRangeBonus
        : synergyRange;
    public bool HasSynergyUnlocked => level >= TowerRangeScaling.SynergyUnlockLevel;
    public int CurrentTowerHealth => currentTowerHealth;
    public int MaxTowerHealth => maxTowerHealth;
    public bool CanTargetFlyingEnemies => canTargetFlying;

    public bool IsMaxLevel => level >= MaxLevel || this is DiamondMineTower;
    public bool CanUpgradeWithGold => level < 3;
    public bool CanUpgradeToLevel4 => level == 3;
    public bool CanChooseBranchUpgrade => level == 4;

    public virtual bool SupportsRally => false;

    protected virtual void Update() { }

    public void Setup(BuildSlot slot, int buildCost, TowerType towerType)
    {
        if (slot == null || !slot.TryOccupy(this))
        {
            Destroy(gameObject);
            return;
        }

        TowerType = towerType;
        occupiedSlot = slot;
        totalGoldSpent = buildCost;
        totalDiamondSpent = 0;
        level = 1;
        branch = TowerBranch.None;

        EnsureSelectionCollider();
        ApplyLevelStats();
        PlatformTerrainCatalog.ApplyTerrainBonuses(this, slot.TerrainType);
        ApplyTowerHealthForLevel();
        RefreshPresentation();
        OnSetupComplete();
        TowerVisualComposer.PlayLanding(gameObject);
    }

    protected virtual void OnEnable()
    {
        TowerRegistry.Register(this);
    }

    protected virtual void OnDisable()
    {
        TowerRegistry.Unregister(this);
    }

    public virtual void TakeTowerDamage(int amount)
    {
        if (amount <= 0 || currentTowerHealth <= 0)
            return;

        currentTowerHealth -= amount;
        OnTowerDamaged?.Invoke(this, amount);
        GetComponent<UnitVisualDecorator>()?.Punch();

        if (currentTowerHealth > 0)
            return;

        if (occupiedSlot != null)
            occupiedSlot.Release(this);

        TowerSelectionController.DeselectIf(this);
        Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (occupiedSlot != null)
        {
            occupiedSlot.Release(this);
            occupiedSlot = null;
        }

        TowerSelectionController.DeselectIf(this);
    }

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void OnSetupComplete() { }

    public virtual TowerUpgradeKind GetNextUpgradeKind()
    {
        if (IsMaxLevel)
            return TowerUpgradeKind.None;

        if (level < 3)
            return TowerUpgradeKind.Gold;

        if (level == 3)
            return TowerUpgradeKind.DiamondLevel4;

        if (level == 4)
            return TowerUpgradeKind.DiamondLevel5Branch;

        return TowerUpgradeKind.None;
    }

    public bool TryUpgradeGold()
    {
        if (!CanUpgradeWithGold)
            return false;

        var cost = GetUpgradeGoldCost(level + 1);
        if (cost <= 0 || GameManager.Instance == null || !GameManager.Instance.TrySpendGold(cost))
            return false;

        totalGoldSpent += cost;
        level++;
        ApplyLevelStats();
        ReapplyTerrainBonuses();
        ApplyTowerHealthForLevel();
        RefreshPresentation();
        return true;
    }

    public bool TryUpgradeLevel4()
    {
        if (!CanUpgradeToLevel4)
            return false;

        var cost = GetUpgradeDiamondCost(4);
        if (cost <= 0 || GameManager.Instance == null || !GameManager.Instance.TrySpendDiamonds(cost))
            return false;

        totalDiamondSpent += cost;
        level = 4;
        ApplyLevelStats();
        ReapplyTerrainBonuses();
        ApplyTowerHealthForLevel();
        RefreshPresentation();
        return true;
    }

    public bool TryUpgradeBranch(TowerBranch selectedBranch)
    {
        if (!CanChooseBranchUpgrade || selectedBranch == TowerBranch.None)
            return false;

        var cost = GetBranchDiamondCost(selectedBranch);
        if (cost <= 0 || GameManager.Instance == null || !GameManager.Instance.TrySpendDiamonds(cost))
            return false;

        totalDiamondSpent += cost;
        branch = selectedBranch;
        level = 5;
        ApplyLevelStats();
        ReapplyTerrainBonuses();
        ApplyTowerHealthForLevel();
        RefreshPresentation();
        return true;
    }

    public bool TryUpgrade()
    {
        return GetNextUpgradeKind() switch
        {
            TowerUpgradeKind.Gold => TryUpgradeGold(),
            TowerUpgradeKind.DiamondLevel4 => TryUpgradeLevel4(),
            _ => false
        };
    }

    public bool TrySell()
    {
        if (GameManager.Instance == null)
            return false;

        GameManager.Instance.AddGold(totalGoldSpent / 2);
        if (totalDiamondSpent > 0)
            GameManager.Instance.AddDiamonds(totalDiamondSpent / 2);
        OnSold();
        if (occupiedSlot != null)
            occupiedSlot.Release(this);

        TowerSelectionController.DeselectIf(this);
        Destroy(gameObject);
        return true;
    }

    protected virtual void OnSold() { }

    public string GetBranchDisplayName()
    {
        if (branch == TowerBranch.None)
            return string.Empty;

        return TowerBuildCatalog.GetBranchName(TowerType, branch);
    }

    public string GetDisplayName()
    {
        if (level >= 5 && branch != TowerBranch.None)
            return $"{TowerBuildCatalog.GetDisplayName(TowerType)} ({GetBranchDisplayName()})";

        return TowerBuildCatalog.GetDisplayName(TowerType);
    }

    public int GetUpgradeGoldCostForNextLevel()
    {
        return level < 3 ? GetUpgradeGoldCost(level + 1) : 0;
    }

    public int GetUpgradeDiamondCostForNextLevel()
    {
        if (level == 3)
            return GetUpgradeDiamondCost(4);

        if (level == 4)
            return GetUpgradeDiamondCost(5);

        return 0;
    }

    public int GetBranchDiamondCost(TowerBranch selectedBranch)
    {
        if (level != 4)
            return 0;

        if (TowerType == TowerType.Barracks && selectedBranch == TowerBranch.BranchA)
            return 12;

        if (TowerType == TowerType.Cannon)
            return 12;

        return GetUpgradeDiamondCost(5);
    }

    public int GetSellRefund() => totalGoldSpent / 2;

    public void SetRanges(float attackRange, float synergyActivationRange)
    {
        range = attackRange;
        synergyRange = synergyActivationRange;
    }

    public virtual void ApplyTerrainModifiers(PlatformTerrainType terrain)
    {
        switch (terrain)
        {
            case PlatformTerrainType.Highland:
            case PlatformTerrainType.RuneRange:
                MultiplyRange(1.10f);
                break;
            case PlatformTerrainType.RuneAttackSpeed:
                MultiplyAttackSpeed(1.15f);
                break;
            case PlatformTerrainType.RuneSynergy:
                MultiplySynergyRange(1.15f);
                break;
            case PlatformTerrainType.Fragile:
                ApplyFragileDamageBonus();
                break;
        }
    }

    protected void MultiplyRange(float multiplier)
    {
        if (multiplier <= 0f)
            return;

        range *= multiplier;
    }

    protected void MultiplyAttackSpeed(float multiplier)
    {
        if (multiplier <= 0f)
            return;

        attackInterval /= multiplier;
    }

    protected void MultiplySynergyRange(float multiplier)
    {
        if (multiplier <= 0f)
            return;

        synergyRange *= multiplier;
    }

    protected virtual void ApplyFragileDamageBonus() { }

    void ReapplyTerrainBonuses()
    {
        if (occupiedSlot != null)
            PlatformTerrainCatalog.ApplyTerrainBonuses(this, occupiedSlot.TerrainType);
    }

    public void SetSelectedVisual(bool selected)
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = selected
            ? Color.Lerp(normalColor, Color.white, 0.35f)
            : normalColor;
    }

    protected virtual void RefreshPresentation()
    {
        transform.localScale = Vector3.one * (0.64f + level * 0.036f);

        if (spriteRenderer != null && TowerSelectionController.Selected != this)
            spriteRenderer.color = normalColor;

        if (spriteRenderer != null)
            TowerVisualComposer.Compose(spriteRenderer, TowerType, level, branch);
    }

    protected abstract void ApplyLevelStats();
    protected abstract int GetUpgradeGoldCost(int targetLevel);
    protected abstract int GetUpgradeDiamondCost(int targetLevel);

    protected void ApplyTowerHealthForLevel()
    {
        maxTowerHealth = level * 50;
        currentTowerHealth = maxTowerHealth;
    }

    void EnsureSelectionCollider()
    {
        var collider = GetComponent<CircleCollider2D>();
        if (collider != null)
            return;

        collider = gameObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.42f;
        collider.isTrigger = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.92f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
