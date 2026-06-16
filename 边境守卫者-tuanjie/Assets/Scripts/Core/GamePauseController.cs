using System;
using UnityEngine;

public class GamePauseController : MonoBehaviour
{
    public static GamePauseController Instance { get; private set; }

    WaveManager waveManager;
    bool isPaused;

    public bool IsPaused => isPaused;

    public event Action<bool> OnPauseChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        waveManager = FindObjectOfType<WaveManager>();

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += HandleGameEnded;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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

        if (waveManager != null && waveManager.State == WaveState.Victory)
            return false;

        return true;
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
        Time.timeScale = 1f;

        if (wasPaused)
            OnPauseChanged?.Invoke(false);
    }

    void HandleGameEnded()
    {
        Resume();
    }
}
