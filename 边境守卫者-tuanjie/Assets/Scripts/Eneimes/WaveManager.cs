using System;
using System.Collections;
using UnityEngine;

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
    [SerializeField] int earlyStartBaseBonus = 10;
    [SerializeField] float preparationCountdown = 15f;

    Coroutine waveRoutine;
    float preparationTimer;
    bool spawnFinished;
    int currentWaveIndex;
    int totalWaves = GrimmForestWaves.TotalWaves;

    public WaveState State { get; private set; } = WaveState.Preparation;
    public int CurrentWaveNumber => currentWaveIndex + 1;
    public int TotalWaves => totalWaves;
    public float PreparationTimeLeft => preparationTimer;

    public event Action OnWaveStateChanged;

    void Start()
    {
        if (mapGridController == null)
            mapGridController = FindObjectOfType<MapGridController>();

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
        OnWaveStateChanged?.Invoke();
    }

    public void StartNextWave()
    {
        if (State != WaveState.Preparation)
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (currentWaveIndex >= totalWaves)
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

        var bonus = earlyStartBaseBonus + CurrentWaveNumber * 2;
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
        spawnFinished = false;
        OnWaveStateChanged?.Invoke();

        var wave = GrimmForestWaves.GetWave(waveIndex + 1);
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
                var takeUpperFork = UnityEngine.Random.value > 0.5f;
                var route = mapGridController.GetSpawnRoute(spawnIndex, takeUpperFork);
                EnemyBase.Spawn(group.enemyType, route, route.StartPoint);
                spawnCounter++;

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
            GameManager.Instance.AddGold(completedWave * 5);

            if (completedWave >= 4)
                GameManager.Instance.AddDiamonds(1);
        }

        if (currentWaveIndex >= totalWaves)
        {
            State = WaveState.Victory;
            OnWaveStateChanged?.Invoke();

            var hud = FindObjectOfType<GameHUD>();
            if (hud != null)
                hud.SetStatus("Victory!", true);

            return;
        }

        BeginPreparation();
    }

    public string GetStatusText()
    {
        return $"{GetWaveCounterText()}\n{GetWaveDetailText()}";
    }

    public string GetWaveCounterText()
    {
        return State == WaveState.Victory
            ? "Victory"
            : $"Wave {CurrentWaveNumber} / {totalWaves}";
    }

    public string GetWaveDetailText()
    {
        return State switch
        {
            WaveState.Preparation => $"Next in {Mathf.CeilToInt(preparationTimer)}s",
            WaveState.Spawning => "Spawning enemies...",
            WaveState.Combat => "In combat",
            WaveState.Victory => "All waves cleared",
            _ => string.Empty
        };
    }
}
