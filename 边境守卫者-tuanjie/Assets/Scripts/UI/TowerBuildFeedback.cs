using UnityEngine;

public static class TowerBuildFeedback
{
    const float MessageDuration = 2.8f;

    public static void ShowInsufficientGold(TowerType type)
    {
        var cost = TowerBuildCatalog.GetBuildCost(type);
        var name = TowerBuildCatalog.GetDisplayName(type);
        var gold = GameManager.Instance != null ? GameManager.Instance.Gold : 0;
        Show($"Not enough gold: {name} needs {cost}g (you have {gold}g).");
    }

    public static void ShowSelectPlatformFirst()
    {
        Show("Select an empty platform first, then choose a tower.");
    }

    public static void Show(string message)
    {
        var hud = Object.FindObjectOfType<GameHUD>();
        if (hud != null)
            hud.ShowTransientMessage(message, MessageDuration);
        else
            Debug.Log(message);
    }
}
