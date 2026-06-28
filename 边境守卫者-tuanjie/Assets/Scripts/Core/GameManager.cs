using System;
using UnityEngine;

/// <summary>
/// Global game state: gold, diamonds, lives (GDD 2.1 / 6).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] int startingGold = 200;
    [SerializeField] int startingDiamonds = 0;
    [SerializeField] int startingLives = 20;

    public int Gold { get; private set; }
    public int Diamonds { get; private set; }
    public int Lives { get; private set; }
    public int StartingLives => startingLives;
    public bool IsGameOver { get; private set; }

    public event Action OnResourcesChanged;
    public event Action OnGameOver;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ApplySessionStart();
    }

    public void ApplySessionStart()
    {
        startingGold = GameDifficultyService.GetStartingGold() + TalentService.StartingGoldBonus;
        startingLives = GameDifficultyService.GetStartingLives();
        ResetResources();
    }

    public void ResetResources()
    {
        IsGameOver = false;
        Gold = startingGold;
        Diamonds = startingDiamonds;
        Lives = startingLives;
        NotifyChanged();
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (Gold < amount) return false;

        Gold -= amount;
        NotifyChanged();
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        Gold += amount;
        NotifyChanged();
    }

    public void TryStealGold(int amount)
    {
        if (IsGameOver || amount <= 0)
            return;

        Gold = Mathf.Max(0, Gold - amount);
        NotifyChanged();
    }

    public bool TrySpendDiamonds(int amount)
    {
        if (amount <= 0) return true;
        if (Diamonds < amount) return false;

        Diamonds -= amount;
        NotifyChanged();
        return true;
    }

    public void AddDiamonds(int amount)
    {
        if (amount <= 0) return;
        Diamonds += amount;
        NotifyChanged();
    }

    public void TakeDamage(int amount)
    {
        if (IsGameOver || amount <= 0) return;

        Lives = Mathf.Max(0, Lives - amount);
        NotifyChanged();

        if (Lives <= 0)
        {
            IsGameOver = true;
            OnGameOver?.Invoke();
            Debug.Log("Level failed: no lives remaining.");
        }
    }

    public void RestoreLives()
    {
        if (IsGameOver)
            return;

        Lives = startingLives;
        NotifyChanged();
    }

    void NotifyChanged()
    {
        OnResourcesChanged?.Invoke();
    }
}
