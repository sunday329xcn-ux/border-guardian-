using System.Collections.Generic;
using UnityEngine;

public enum EasterEggId
{
    CornerTile,
    AllTowerTypes
}

public class EasterEggController : MonoBehaviour
{
    public static EasterEggController Instance { get; private set; }

    const float CornerClickRadius = 0.55f;
    const int DiamondRewardMin = 1;
    const int DiamondRewardMax = 3;

    static readonly TowerType[] RequiredTowerTypes =
    {
        TowerType.Arrow,
        TowerType.Frost,
        TowerType.Cannon,
        TowerType.Arcane,
        TowerType.Barracks,
        TowerType.DiamondMine
    };

    readonly HashSet<EasterEggId> triggeredEggs = new();
    readonly HashSet<TowerType> builtTowerTypes = new();
    bool startHintShown;
    bool startHintVisible;
    WaveManager waveManager;

    public const float HideHintWhenPrepSecondsLeft = 10f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
            waveManager.OnWaveStateChanged += HandleWaveStateChanged;
    }

    void OnDestroy()
    {
        if (waveManager != null)
            waveManager.OnWaveStateChanged -= HandleWaveStateChanged;

        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (!startHintVisible || waveManager == null)
            return;

        if (ShouldHideStartHint())
            HideStartHint();
    }

    static bool ShouldHideStartHint(WaveManager manager)
    {
        if (manager == null)
            return true;

        if (manager.State != WaveState.Preparation || manager.CurrentWaveNumber != 1)
            return true;

        return manager.PreparationTimeLeft <= HideHintWhenPrepSecondsLeft;
    }

    bool ShouldHideStartHint() => ShouldHideStartHint(waveManager);

    void HideStartHint()
    {
        startHintVisible = false;
        EasterEggHintBannerUI.HideHint();
    }

    void HandleWaveStateChanged()
    {
        if (startHintShown || waveManager == null)
            return;

        if (waveManager.CurrentWaveNumber != 1 || waveManager.State != WaveState.Preparation)
            return;

        startHintShown = true;
        startHintVisible = true;
        EasterEggHintBannerUI.ShowStartHint();

        if (ShouldHideStartHint())
            HideStartHint();
    }

    public bool TryHandleCornerClick(Vector3 worldPoint)
    {
        if (triggeredEggs.Contains(EasterEggId.CornerTile))
            return false;

        var corner = GrimmForestMapLayout.EasterEggCornerCell;
        if (!MapGridSettings.IsPointInCell(worldPoint, corner, CornerClickRadius))
            return false;

        TriggerEgg(EasterEggId.CornerTile);
        return true;
    }

    public void RegisterTowerBuilt(TowerType towerType)
    {
        if (triggeredEggs.Contains(EasterEggId.AllTowerTypes))
            return;

        builtTowerTypes.Add(towerType);
        if (builtTowerTypes.Count < RequiredTowerTypes.Length)
            return;

        foreach (var required in RequiredTowerTypes)
        {
            if (!builtTowerTypes.Contains(required))
                return;
        }

        TriggerEgg(EasterEggId.AllTowerTypes);
    }

    void TriggerEgg(EasterEggId eggId)
    {
        if (triggeredEggs.Contains(eggId))
            return;

        triggeredEggs.Add(eggId);
        ApplyReward(eggId, out var title, out var rewardText);
        EasterEggCelebrationUI.ShowCelebration(title, rewardText);
    }

    void ApplyReward(EasterEggId eggId, out string title, out string rewardText)
    {
        switch (eggId)
        {
            case EasterEggId.CornerTile:
                if (waveManager == null)
                    waveManager = FindObjectOfType<WaveManager>();
                if (waveManager != null)
                    waveManager.SetCallEarlyBonusMultiplier(1.5f);

                title = "Congratulations!";
                rewardText = "Secret corner found!\nCall Early gold bonus +50% for the rest of this level.";
                break;

            case EasterEggId.AllTowerTypes:
                var diamonds = Random.Range(DiamondRewardMin, DiamondRewardMax + 1);
                GameManager.Instance?.AddDiamonds(diamonds);
                title = "Congratulations!";
                rewardText = $"Master builder!\nYou earned {diamonds} diamond(s).";
                break;

            default:
                title = "Congratulations!";
                rewardText = "Bonus unlocked!";
                break;
        }
    }
}
