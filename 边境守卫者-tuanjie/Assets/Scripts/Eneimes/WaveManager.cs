using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum WaveState
{
    Preparation,
    Spawning,
    Combat,
    Victory
}

public class WaveManager : MonoBehaviour
{
    [SerializeField] MapGridController mapGridController;
    [SerializeField] LevelId currentLevel = LevelId.GrimmForest;
    [SerializeField] int earlyStartBaseBonus = 14;
    [SerializeField] float preparationCountdown = 15f;

    Coroutine waveRoutine;
    float preparationTimer;
    float callEarlyBonusMultiplier = 1f;
    bool spawnFinished;
    int currentWaveIndex;
    int totalWaves = GrimmForestWaves.TotalWaves;
    int currentWaveTotalEnemies;
    int currentWaveSpawnedEnemies;
    bool gameplayStarted;
    bool scoreSubmitted;

    public WaveState State { get; private set; } = WaveState.Preparation;
    public LevelId CurrentLevelId => currentLevel;
    public int CurrentWaveNumber => currentWaveIndex + 1;
    public int TotalWaves => totalWaves;
    public float PreparationTimeLeft => preparationTimer;
    public float CallEarlyBonusMultiplier => callEarlyBonusMultiplier;
    public int CurrentWaveTotalEnemies => currentWaveTotalEnemies;
    public int CurrentWaveSpawnedEnemies => currentWaveSpawnedEnemies;
    public float WaveSpawnProgress =>
        currentWaveTotalEnemies <= 0 ? 0f : currentWaveSpawnedEnemies / (float)currentWaveTotalEnemies;
    public WaveDefinition UpcomingWaveDefinition =>
        (!GameModeService.IsEndless && currentWaveIndex >= totalWaves)
            ? null
            : GetWaveDefinition(currentWaveIndex + 1);

    public int ClearedWaves => currentWaveIndex;
    public int CurrentRunScore =>
        LeaderboardService.ComputeScore(currentWaveIndex, GameManager.Instance != null ? GameManager.Instance.Lives : 0);

    public event Action OnWaveStateChanged;
    public event Action OnWaveSpawnProgressChanged;
    /// <summary>Raised after a non-final wave is cleared, with the cleared wave number (P4.2 buff pick).</summary>
    public event Action<int> OnWaveCleared;

    void Start()
    {
        MapPlatformUnlockService.ResetSession();
        NullifierSuppressionService.Reset();
        TowerSelectionController.ResetState();
        EnemySelectionController.ResetState();
        BuildSlotSelectionController.ResetState();
        RoguelikeModifierService.Reset();
        MapModifierService.Load();
        GameModeService.Load();
        EndlessScalingService.Reset();

        if (mapGridController == null)
            mapGridController = FindObjectOfType<MapGridController>();

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += HandleGameOver;

        if (MainMenuUI.IsSessionStarted)
            StartGameplay();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= HandleGameOver;

        SubmitScore();
    }

    void HandleGameOver()
    {
        SubmitScore();
    }

    void SubmitScore()
    {
        if (scoreSubmitted)
            return;

        scoreSubmitted = true;
        LeaderboardService.Submit(GameModeService.Mode, CurrentRunScore);
    }

    WaveDefinition GetWaveDefinition(int waveNumber)
    {
        if (waveNumber > GrimmForestWaves.TotalWaves)
            return EndlessWaveGenerator.Generate(waveNumber - GrimmForestWaves.TotalWaves);

        var wave = GrimmForestWaves.GetWave(waveNumber);
        if (GameModeService.EarlyBoss && waveNumber == 7)
            wave = InjectBoss(wave);

        return wave;
    }

    static WaveDefinition InjectBoss(WaveDefinition source)
    {
        var groups = new List<WaveSpawnGroup>(source.groups)
        {
            new WaveSpawnGroup
            {
                enemyType = EnemyType.AncientDragon,
                count = 1,
                spawnInterval = 2f,
                delayBeforeGroup = 2f
            }
        };

        return new WaveDefinition
        {
            note = source.note,
            hintText = "Challenge: early boss",
            groups = groups.ToArray()
        };
    }

    public void StartGameplay()
    {
        if (gameplayStarted)
            return;

        gameplayStarted = true;
        BeginPreparation();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (State == WaveState.Preparation && preparationTimer > 0f)
        {
            preparationTimer -= Time.deltaTime;
            if (preparationTimer <= 0f)
                StartNextWave();
        }

        if (State == WaveState.Combat && spawnFinished && EnemyRegistry.ActiveEnemies.Count == 0)
            CompleteCurrentWave();
    }

    public void BeginPreparation()
    {
        State = WaveState.Preparation;
        preparationTimer = preparationCountdown;
        spawnFinished = false;

        if (mapGridController == null)
            mapGridController = FindObjectOfType<MapGridController>();

        MapPlatformUnlockService.TryUnlockForWave(CurrentWaveNumber, mapGridController);
        OnWaveStateChanged?.Invoke();
    }

    public void StartNextWave()
    {
        if (State != WaveState.Preparation)
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (!GameModeService.IsEndless && currentWaveIndex >= totalWaves)
            return;

        if (waveRoutine != null)
            StopCoroutine(waveRoutine);

        waveRoutine = StartCoroutine(SpawnWave(currentWaveIndex));
    }

    public void CallWaveEarly()
    {
        if (State != WaveState.Preparation)
            return;

        if (GameManager.Instance == null)
            return;

        var bonus = Mathf.RoundToInt((earlyStartBaseBonus + CurrentWaveNumber * 3)
            * callEarlyBonusMultiplier * RoguelikeModifierService.CallEarlyMultiplier
            * TalentService.CallEarlyTalentMultiplier);
        GameManager.Instance.AddGold(bonus);
        preparationTimer = 0f;
        StartNextWave();
    }

    IEnumerator SpawnWave(int waveIndex)
    {
        if (mapGridController == null || mapGridController.Path == null)
        {
            Debug.LogError("WaveManager is missing MapGridController or path reference.");
            yield break;
        }

        State = WaveState.Spawning;
        CombatStatsTracker.MarkCombatStarted();
        spawnFinished = false;
        OnWaveStateChanged?.Invoke();

        var waveNumber = waveIndex + 1;
        if (GameModeService.IsEndless && waveNumber > GrimmForestWaves.TotalWaves)
            EndlessScalingService.SetRound(waveNumber - GrimmForestWaves.TotalWaves);
        else
            EndlessScalingService.Reset();

        var wave = GetWaveDefinition(waveNumber);
        currentWaveTotalEnemies = WavePreviewHelper.CountTotalEnemies(wave);
        currentWaveSpawnedEnemies = 0;
        OnWaveSpawnProgressChanged?.Invoke();
        var spawnCounter = 0;

        foreach (var group in wave.groups)
        {
            if (group.delayBeforeGroup > 0f)
                yield return new WaitForSeconds(group.delayBeforeGroup);

            for (int i = 0; i < group.count; i++)
            {
                if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                    yield break;

                var spawnIndex = spawnCounter % 2;
                var routeControl = mapGridController.Route;
                ForkBranchChoice branch;
                if (routeControl != null)
                    branch = routeControl.ResolveForkBranch(spawnCounter);
                else
                    branch = ForkBranchChoice.Central;

                var route = routeControl != null
                    ? routeControl.GetRoute(spawnIndex, branch)
                    : mapGridController.GetSpawnRoute(
                        spawnIndex,
                        branch == ForkBranchChoice.UpperScenic);
                EnemyBase.Spawn(group.enemyType, route, route.StartPoint);
                spawnCounter++;
                currentWaveSpawnedEnemies++;
                OnWaveSpawnProgressChanged?.Invoke();

                if (i < group.count - 1 && group.spawnInterval > 0f)
                    yield return new WaitForSeconds(group.spawnInterval);
            }
        }

        spawnFinished = true;
        State = WaveState.Combat;
        OnWaveStateChanged?.Invoke();
        waveRoutine = null;
    }

    void CompleteCurrentWave()
    {
        var completedWave = CurrentWaveNumber;
        currentWaveIndex++;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(completedWave * 8);

            if (completedWave >= 4)
                GameManager.Instance.AddDiamonds(1);
        }

        if (!GameModeService.IsEndless && currentWaveIndex >= totalWaves)
        {
            if (GamePauseController.Instance != null)
                GamePauseController.Instance.Resume();

            State = WaveState.Victory;
            OnWaveStateChanged?.Invoke();

            SubmitScore();
            ShowVictoryResult();
            return;
        }

        OnWaveCleared?.Invoke(completedWave);
        BeginPreparation();
    }

    void ShowVictoryResult()
    {
        CombatStatsTracker.MarkCombatEnded();

        var gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            var hud = FindObjectOfType<GameHUD>();
            if (hud != null)
                hud.SetStatus("Victory!", true);

            return;
        }

        var result = LevelProgressService.RecordVictory(
            currentLevel,
            gameManager.Lives,
            gameManager.StartingLives);

        var victoryUi = FindObjectOfType<VictoryResultUI>();
        if (victoryUi != null)
        {
            victoryUi.Show(result, gameManager.Lives, gameManager.StartingLives);
            return;
        }

        var fallbackHud = FindObjectOfType<GameHUD>();
        if (fallbackHud != null)
            fallbackHud.SetStatus("Victory!", true);
    }

    public string GetStatusText()
    {
        return $"{GetWaveCounterText()}\n{GetWaveDetailText()}";
    }

    public string GetWaveCounterText()
    {
        if (State == WaveState.Victory)
            return "Victory";

        if (GameModeService.IsEndless && CurrentWaveNumber > totalWaves)
            return $"Endless {CurrentWaveNumber - totalWaves}";

        return $"Wave {CurrentWaveNumber} / {totalWaves}";
    }

    public string GetWaveDetailText()
    {
        return State switch
        {
            WaveState.Preparation => $"Next in {Mathf.CeilToInt(preparationTimer)}s",
            WaveState.Spawning => $"Spawning {Mathf.RoundToInt(WaveSpawnProgress * 100f)}%",
            WaveState.Combat when currentWaveTotalEnemies > 0 => $"Wave {CurrentWaveNumber}: 100% spawned",
            WaveState.Combat => "In combat",
            WaveState.Victory => "All waves cleared",
            _ => string.Empty
        };
    }

    public string GetUpcomingEnemySummary()
    {
        var wave = UpcomingWaveDefinition;
        return wave == null ? string.Empty : WavePreviewHelper.BuildEnemySummary(wave);
    }

    public string GetUpcomingHint()
    {
        var wave = UpcomingWaveDefinition;
        return wave == null ? string.Empty : WavePreviewHelper.GetHint(wave);
    }

    public bool WillUpcomingWaveUseSpawnLane(int spawnIndex)
    {
        var wave = UpcomingWaveDefinition;
        return wave != null && WavePreviewHelper.UsesSpawnLane(wave, spawnIndex);
    }

    public string GetUpcomingEnemySummaryForSpawnLane(int spawnIndex)
    {
        var wave = UpcomingWaveDefinition;
        return wave == null
            ? string.Empty
            : WavePreviewHelper.BuildEnemySummaryForSpawnLane(wave, spawnIndex);
    }

    public int GetUpcomingEnemyCountForSpawnLane(int spawnIndex)
    {
        var wave = UpcomingWaveDefinition;
        return wave == null ? 0 : WavePreviewHelper.CountEnemiesForSpawnLane(wave, spawnIndex);
    }

    public static string GetSpawnLaneDisplayName(int spawnIndex)
    {
        return spawnIndex == 0 ? "Upper Route" : "Lower Route";
    }

    public void SetCallEarlyBonusMultiplier(float multiplier)
    {
        callEarlyBonusMultiplier = Mathf.Max(1f, multiplier);
    }

    public void ResetLevel()
    {
        Time.timeScale = 1f;
        MainMenuUI.MarkSessionStarted();

        if (GamePauseController.Instance != null)
            GamePauseController.Instance.Resume();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
