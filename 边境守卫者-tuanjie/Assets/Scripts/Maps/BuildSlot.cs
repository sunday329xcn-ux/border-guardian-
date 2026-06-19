using UnityEngine;

public class BuildSlot : MonoBehaviour
{
    [SerializeField] SpriteRenderer highlightRenderer;

    TowerBase occupant;

    public Vector2Int GridPosition { get; private set; }
    public bool IsPlatform { get; private set; }
    public bool IsOccupied { get; private set; }
    public TowerBase Occupant => occupant;

    public void Initialize(Vector2Int gridPosition, bool isPlatform, SpriteRenderer highlight)
    {
        GridPosition = gridPosition;
        IsPlatform = isPlatform;
        highlightRenderer = highlight;
        name = isPlatform ? $"BuildSlot_Platform_{gridPosition.x}_{gridPosition.y}" : $"BuildSlot_Ground_{gridPosition.x}_{gridPosition.y}";
    }

    public bool CanAcceptBuild()
    {
        ClearIfOccupantDestroyed();
        return !IsOccupied;
    }

    public bool TryOccupy(TowerBase tower)
    {
        ClearIfOccupantDestroyed();

        if (tower == null || IsOccupied)
            return false;

        IsOccupied = true;
        occupant = tower;
        SetHighlight(true);
        return true;
    }

    public void Release(TowerBase tower = null)
    {
        if (tower != null && occupant != null && occupant != tower)
            return;

        IsOccupied = false;
        occupant = null;
        SetHighlight(false);
    }

    public void ClearIfOccupantDestroyed()
    {
        if (!IsOccupied)
            return;

        if (occupant == null)
            Release();
    }

    public void SetHighlight(bool enabled)
    {
        if (highlightRenderer != null)
            highlightRenderer.enabled = enabled;
    }
}
