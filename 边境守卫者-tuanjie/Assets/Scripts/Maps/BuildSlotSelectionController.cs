using System;
using UnityEngine;

public static class BuildSlotSelectionController
{
    static BuildSlot selectedSlot;

    public static BuildSlot Selected => selectedSlot;
    public static event Action<BuildSlot> OnSelectionChanged;

    public static void Select(BuildSlot slot)
    {
        if (slot == null || !slot.CanAcceptBuild())
        {
            Deselect();
            return;
        }

        if (selectedSlot == slot)
        {
            Deselect();
            return;
        }

        SetSelectionVisual(selectedSlot, false);
        selectedSlot = slot;
        SetSelectionVisual(selectedSlot, true);
        TowerSelectionController.Deselect();
        OnSelectionChanged?.Invoke(selectedSlot);
    }

    public static void Deselect()
    {
        if (selectedSlot == null)
            return;

        SetSelectionVisual(selectedSlot, false);
        selectedSlot = null;
        OnSelectionChanged?.Invoke(null);
    }

    public static void DeselectIf(BuildSlot slot)
    {
        if (selectedSlot == slot)
            Deselect();
    }

    static void SetSelectionVisual(BuildSlot slot, bool enabled)
    {
        slot?.SetSelectionHighlight(enabled);
    }
}
