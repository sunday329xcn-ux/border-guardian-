using System.Collections.Generic;
using UnityEngine;

public static class MapGridLayoutUtility
{
    public static Vector3 CellToLocal(int x, int y)
    {
        return MapGridSettings.GridToWorld(x, y);
    }

    public static void SnapTransformToCell(Transform target, Vector2Int cell)
    {
        if (target == null)
            return;

        target.localPosition = CellToLocal(cell.x, cell.y);
    }

    public static void SnapOccupiedTowers(IEnumerable<BuildSlot> slots)
    {
        if (slots == null)
            return;

        foreach (var slot in slots)
        {
            if (slot == null)
                continue;

            SnapTransformToCell(slot.transform, slot.GridPosition);

            if (slot.IsOccupied && slot.Occupant != null)
                slot.Occupant.transform.position = slot.transform.position;
        }
    }
}
