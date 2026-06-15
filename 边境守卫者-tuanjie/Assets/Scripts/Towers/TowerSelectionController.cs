using System;
using UnityEngine;

public static class TowerSelectionController
{
    static TowerBase selectedTower;

    public static TowerBase Selected => selectedTower;
    public static event Action<TowerBase> OnSelectionChanged;

    public static void Select(TowerBase tower)
    {
        if (selectedTower == tower)
            return;

        if (selectedTower != null)
            selectedTower.SetSelectedVisual(false);

        selectedTower = tower;

        if (selectedTower != null)
            selectedTower.SetSelectedVisual(true);

        OnSelectionChanged?.Invoke(selectedTower);
    }

    public static void Deselect()
    {
        Select(null);
    }

    public static void DeselectIf(TowerBase tower)
    {
        if (selectedTower == tower)
            Deselect();
    }
}
