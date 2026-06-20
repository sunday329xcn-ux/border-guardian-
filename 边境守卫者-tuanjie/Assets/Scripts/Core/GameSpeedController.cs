using System;
using UnityEngine;

/// <summary>
/// Game speed multiplier (1x / 2x / 3x). Pausing forces timeScale to 0 via GamePauseController.
/// </summary>
public class GameSpeedController : MonoBehaviour
{
    public static GameSpeedController Instance { get; private set; }

    static readonly float[] SpeedSteps = { 1f, 2f, 3f };

    int speedIndex;

    public float CurrentSpeed => SpeedSteps[speedIndex];
    public string CurrentSpeedLabel => $"{Mathf.RoundToInt(CurrentSpeed)}x";

    public event Action OnSpeedChanged;

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
        if (GamePauseController.Instance != null)
            GamePauseController.Instance.OnPauseChanged += HandlePauseChanged;

        ApplySpeedIfRunning();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (GamePauseController.Instance != null)
            GamePauseController.Instance.OnPauseChanged -= HandlePauseChanged;

        if (Time.timeScale > 0f && (GamePauseController.Instance == null || !GamePauseController.Instance.IsPaused))
            Time.timeScale = 1f;
    }

    public void CycleSpeed()
    {
        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        speedIndex = (speedIndex + 1) % SpeedSteps.Length;
        ApplySpeedIfRunning();
        OnSpeedChanged?.Invoke();
    }

    public void ResetSpeed()
    {
        speedIndex = 0;
        ApplySpeedIfRunning();
        OnSpeedChanged?.Invoke();
    }

    public void ApplySpeedIfRunning()
    {
        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            Time.timeScale = 1f;
            return;
        }

        Time.timeScale = CurrentSpeed;
    }

    void HandlePauseChanged(bool paused)
    {
        if (paused)
            Time.timeScale = 0f;
        else
            ApplySpeedIfRunning();
    }
}
