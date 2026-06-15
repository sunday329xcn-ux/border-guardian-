using UnityEngine;

public class BuildSlot : MonoBehaviour
{
    [SerializeField] SpriteRenderer highlightRenderer;

    public Vector2Int GridPosition { get; private set; }
    public bool IsPlatform { get; private set; }
    public bool IsOccupied { get; private set; }

    public void Initialize(Vector2Int gridPosition, bool isPlatform, SpriteRenderer highlight)
    {
        GridPosition = gridPosition;
        IsPlatform = isPlatform;
        highlightRenderer = highlight;
        name = isPlatform ? $"BuildSlot_Platform_{gridPosition.x}_{gridPosition.y}" : $"BuildSlot_Ground_{gridPosition.x}_{gridPosition.y}";
    }

    public bool TryOccupy()
    {
        if (IsOccupied) return false;
        IsOccupied = true;
        SetHighlight(true);
        return true;
    }

    public void Release()
    {
        IsOccupied = false;
        SetHighlight(false);
    }

    public void SetHighlight(bool enabled)
    {
        if (highlightRenderer != null)
            highlightRenderer.enabled = enabled;
    }
}
