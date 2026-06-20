using UnityEngine;

public class BuildSlot : MonoBehaviour
{
    [SerializeField] SpriteRenderer highlightRenderer;
    [SerializeField] SpriteRenderer selectionHighlightRenderer;
    [SerializeField] SpriteRenderer baseRenderer;

    TowerBase occupant;

    public Vector2Int GridPosition { get; private set; }
    public bool IsPlatform { get; private set; }
    public bool IsOccupied { get; private set; }
    public TowerBase Occupant => occupant;
    public PlatformTerrainType TerrainType { get; private set; } = PlatformTerrainType.Standard;
    public bool IsFragilePlatform => TerrainType == PlatformTerrainType.Fragile;

    public void Initialize(
        Vector2Int gridPosition,
        bool isPlatform,
        SpriteRenderer platformRenderer,
        SpriteRenderer highlight,
        SpriteRenderer selectionHighlight,
        PlatformTerrainType terrainType)
    {
        GridPosition = gridPosition;
        IsPlatform = isPlatform;
        baseRenderer = platformRenderer;
        highlightRenderer = highlight;
        selectionHighlightRenderer = selectionHighlight;
        TerrainType = terrainType;
        name = isPlatform ? $"BuildSlot_Platform_{gridPosition.x}_{gridPosition.y}" : $"BuildSlot_Ground_{gridPosition.x}_{gridPosition.y}";
        ApplyTerrainVisual();
    }

    public void ApplyTerrainVisual()
    {
        if (baseRenderer != null)
            baseRenderer.color = PlatformTerrainCatalog.GetPlatformTint(TerrainType);

        foreach (Transform child in transform)
        {
            if (child.name == "TerrainMarker")
                Destroy(child.gameObject);
        }

        if (!PlatformTerrainCatalog.TryGetMarkerSpec(TerrainType, out var spec))
            return;

        var marker = new GameObject("TerrainMarker");
        marker.transform.SetParent(transform, false);
        marker.transform.localPosition = spec.LocalOffset;
        marker.transform.localScale = spec.LocalScale;
        marker.transform.localRotation = Quaternion.Euler(0f, 0f, spec.RotationZ);

        var renderer = marker.AddComponent<SpriteRenderer>();
        renderer.sprite = MapGridControllerShared.GetWhiteSprite();
        renderer.color = spec.Color;
        renderer.sortingOrder = 2;
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
        SetSelectionHighlight(false);
        SetHighlight(true);
        BuildSlotSelectionController.DeselectIf(this);
        return true;
    }

    public void Release(TowerBase tower = null)
    {
        if (tower != null && occupant != null && occupant != tower)
            return;

        IsOccupied = false;
        occupant = null;
        SetHighlight(false);
        SetSelectionHighlight(false);
    }

    public void SetSelectionHighlight(bool enabled)
    {
        if (selectionHighlightRenderer == null)
            return;

        selectionHighlightRenderer.enabled = enabled && !IsOccupied;
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
