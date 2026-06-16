using UnityEngine;

public class MapEnvironmentController : MonoBehaviour
{
    const int TrapGoldCost = 50;
    const float TrapCooldownSeconds = 60f;
    const float CellClickRadius = 0.55f;

    readonly AncientTree[] ancientTrees = new AncientTree[2];
    TrapPlacementSlot[] trapSlots;

    Transform environmentRoot;
    HunterTrap activeTrap;
    float trapCooldownRemaining;

    public bool CanPlaceTrap =>
        activeTrap == null && trapCooldownRemaining <= 0f
        && GameManager.Instance != null
        && GameManager.Instance.Gold >= TrapGoldCost
        && !GameManager.Instance.IsGameOver
        && (GamePauseController.Instance == null || !GamePauseController.Instance.IsPaused);

    public float TrapCooldownRemaining => Mathf.Max(0f, trapCooldownRemaining);

    public void BuildEnvironment(Transform parent)
    {
        environmentRoot = new GameObject("Environment").transform;
        environmentRoot.SetParent(parent, false);

        for (int i = 0; i < GrimmForestMapLayout.AncientTreeCells.Length; i++)
        {
            var treeCell = GrimmForestMapLayout.AncientTreeCells[i];
            var effectCell = GrimmForestMapLayout.AncientTreeEffectCells[i];
            var treeObject = new GameObject($"AncientTree_{i}");
            treeObject.transform.SetParent(environmentRoot, false);

            var tree = treeObject.AddComponent<AncientTree>();
            tree.Initialize(
                MapGridSettings.GridToWorld(treeCell.x, treeCell.y),
                MapGridSettings.GridToWorld(effectCell.x, effectCell.y));
            ancientTrees[i] = tree;
        }

        var placementCells = GrimmForestMapLayout.HunterTrapPlacementCells;
        trapSlots = new TrapPlacementSlot[placementCells.Length];
        for (int i = 0; i < placementCells.Length; i++)
        {
            trapSlots[i] = TrapPlacementSlot.Create(environmentRoot, placementCells[i]);
        }
    }

    void Update()
    {
        if (trapCooldownRemaining > 0f)
            trapCooldownRemaining -= Time.deltaTime;

        RefreshTrapSlotVisuals();
    }

    void RefreshTrapSlotVisuals()
    {
        var readyToPlace = CanPlaceTrap;
        var onCooldown = trapCooldownRemaining > 0f;

        foreach (var slot in trapSlots)
        {
            if (slot == null)
                continue;

            var occupied = activeTrap != null && activeTrap.GridCell == slot.GridCell;
            slot.SetVisualState(readyToPlace, onCooldown, occupied);
        }
    }

    public bool TryHandleClick(Vector3 worldPoint)
    {
        if (TryPlaceHunterTrap(worldPoint))
            return true;

        foreach (var tree in ancientTrees)
        {
            if (tree == null)
                continue;

            if (Vector2.Distance(tree.transform.position, worldPoint) > 0.65f)
                continue;

            return tree.TryActivate();
        }

        return false;
    }

    bool TryPlaceHunterTrap(Vector3 worldPoint)
    {
        if (!CanPlaceTrap)
            return false;

        var cell = FindTrapPlacementCell(worldPoint);
        if (cell == null)
            return false;

        if (GameManager.Instance == null || !GameManager.Instance.TrySpendGold(TrapGoldCost))
            return false;

        activeTrap = HunterTrap.Create(environmentRoot, cell.Value, OnTrapTriggered);
        return true;
    }

    Vector2Int? FindTrapPlacementCell(Vector3 worldPoint)
    {
        foreach (var slot in trapSlots)
        {
            if (slot == null)
                continue;

            if (Vector2.Distance(slot.WorldPosition, worldPoint) > CellClickRadius)
                continue;

            return slot.GridCell;
        }

        return null;
    }

    void OnTrapTriggered()
    {
        activeTrap = null;
        trapCooldownRemaining = TrapCooldownSeconds;
        RefreshTrapSlotVisuals();
    }

    sealed class TrapPlacementSlot
    {
        static readonly Color ReadyColor = new Color(0.85f, 0.55f, 0.15f, 0.45f);
        static readonly Color CooldownColor = new Color(0.35f, 0.35f, 0.35f, 0.35f);
        static readonly Color OccupiedColor = new Color(0.85f, 0.55f, 0.15f, 0.15f);

        SpriteRenderer renderer;

        public Vector2Int GridCell { get; private set; }
        public Vector3 WorldPosition { get; private set; }

        public static TrapPlacementSlot Create(Transform parent, Vector2Int gridCell)
        {
            var slotObject = new GameObject($"TrapSlot_{gridCell.x}_{gridCell.y}");
            slotObject.transform.SetParent(parent, false);
            slotObject.transform.position = MapGridSettings.GridToWorld(gridCell.x, gridCell.y);
            slotObject.transform.localScale = Vector3.one * 0.5f;

            var renderer = slotObject.AddComponent<SpriteRenderer>();
            renderer.sprite = MapGridControllerShared.GetWhiteSprite();
            renderer.sortingOrder = 2;

            return new TrapPlacementSlot
            {
                GridCell = gridCell,
                WorldPosition = slotObject.transform.position,
                renderer = renderer
            };
        }

        public void SetVisualState(bool readyToPlace, bool onCooldown, bool occupied)
        {
            if (renderer == null)
                return;

            if (occupied)
                renderer.color = OccupiedColor;
            else if (readyToPlace)
                renderer.color = ReadyColor;
            else if (onCooldown)
                renderer.color = CooldownColor;
            else
                renderer.color = new Color(0.55f, 0.4f, 0.15f, 0.3f);
        }
    }
}
