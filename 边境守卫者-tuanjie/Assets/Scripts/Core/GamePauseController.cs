using System;
using UnityEngine;

public class GamePauseController : MonoBehaviour
{
    public static GamePauseController Instance { get; private set; }

    WaveManager waveManager;
    bool isPaused;
    bool modalFreeze;

    public bool IsPaused => isPaused || modalFreeze;

    public event Action<bool> OnPauseChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureSpeedController();
        Time.timeScale = GameSpeedController.Instance != null
            ? GameSpeedController.Instance.CurrentSpeed
            : 1f;
    }

    void Start()
    {
        waveManager = FindObjectOfType<WaveManager>();

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += HandleGameEnded;
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        // Esc only toggles pause during an active gameplay session, and never
        // while a modal (e.g. exit confirm) has frozen time.
        if (!MainMenuUI.IsSessionStarted || modalFreeze)
            return;

        TogglePause();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= HandleGameEnded;
    }

    public bool CanPause()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return false;

        if (waveManager == null)
            waveManager = FindObjectOfType<WaveManager>();

        if (waveManager != null && waveManager.State == WaveState.Victory)
            return false;

        return true;
    }

    /// <summary>
    /// Freeze time for a modal overlay (e.g. exit confirm) without flipping the
    /// pause state / firing OnPauseChanged. IsPaused still reports true so other
    /// systems (drag-build, previews) stay consistent.
    /// </summary>
    public void BeginModalFreeze()
    {
        if (modalFreeze)
            return;

        modalFreeze = true;
        Time.timeScale = 0f;
    }

    public void EndModalFreeze()
    {
        if (!modalFreeze)
            return;

        modalFreeze = false;

        if (isPaused)
            return;

        if (GameSpeedController.Instance != null)
            GameSpeedController.Instance.ApplySpeedIfRunning();
        else
            Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (isPaused || !CanPause())
            return;

        isPaused = true;
        Time.timeScale = 0f;
        OnPauseChanged?.Invoke(true);
    }

    public void Resume()
    {
        var wasPaused = isPaused;
        isPaused = false;

        if (GameSpeedController.Instance != null)
            GameSpeedController.Instance.ApplySpeedIfRunning();
        else
            Time.timeScale = 1f;

        if (wasPaused)
            OnPauseChanged?.Invoke(false);
    }

    void HandleGameEnded()
    {
        GameSpeedController.Instance?.ResetSpeed();
        Resume();
    }

    static void EnsureSpeedController()
    {
        if (GameSpeedController.Instance != null)
            return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.gameObject.AddComponent<GameSpeedController>();
            return;
        }

        var speedObject = new GameObject("GameSpeedController");
        speedObject.AddComponent<GameSpeedController>();
    }
}
