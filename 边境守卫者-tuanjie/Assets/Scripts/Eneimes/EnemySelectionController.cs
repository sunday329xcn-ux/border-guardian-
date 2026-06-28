using System;
using UnityEngine;

public static class EnemySelectionController
{
    static EnemyBase selectedEnemy;

    public static EnemyBase Selected => selectedEnemy;
    public static event Action<EnemyBase> OnSelectionChanged;

    public static void Select(EnemyBase enemy)
    {
        if (selectedEnemy == enemy)
            return;

        if (selectedEnemy != null)
            selectedEnemy.SetSelectedVisual(false);

        selectedEnemy = enemy;

        if (selectedEnemy != null)
        {
            selectedEnemy.SetSelectedVisual(true);
            TowerSelectionController.Deselect();
        }

        OnSelectionChanged?.Invoke(selectedEnemy);
    }

    public static void Deselect()
    {
        Select(null);
    }

    public static void ResetState()
    {
        selectedEnemy = null;
    }

    public static void DeselectIf(EnemyBase enemy)
    {
        if (selectedEnemy == enemy)
            Deselect();
    }
}
