using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player-controlled fork gate and temporary scenic-route blocks.
/// </summary>
public class MapRouteController : MonoBehaviour
{
    public const int ForkSwitchGoldCost = 30;
    public const int BlockGoldCost = 100;
    public const float BlockDurationSeconds = 20f;
    public const float BlockCooldownSeconds = 75f;

    [SerializeField] Transform routeRoot;

    readonly Dictionary<int, WaypointPath> routeCache = new();

    ForkRouteMode forkMode = ForkRouteMode.None;
    RouteBlockType activeBlock = RouteBlockType.None;
    float blockEndTime;
    float blockCooldownEndTime;

    SpriteRenderer forkGateRenderer;
    SpriteRenderer upperBlockRenderer;
    SpriteRenderer lowerBlockRenderer;

    const float MarkerClickRadius = 0.55f;

    public ForkRouteMode ForkMode => forkMode;
    public RouteBlockType ActiveBlock => activeBlock;
    public float BlockTimeRemaining => activeBlock == RouteBlockType.None ? 0f : Mathf.Max(0f, blockEndTime - Time.time);
    public float BlockCooldownRemaining => Mathf.Max(0f, blockCooldownEndTime - Time.time);
    public bool IsBlockReady => activeBlock == RouteBlockType.None && BlockCooldownRemaining <= 0f;

    public event Action OnRouteStateChanged;

    public void BuildRouteControls(Transform parent)
    {
        routeRoot = new GameObject("RouteControls").transform;
        routeRoot.SetParent(parent, false);

        forkGateRenderer = CreateMarker("ForkGate", GrimmForestMapLayout.ForkGateCell, new Color(0.35f, 0.85f, 0.95f, 0.9f), 4, 0.55f, routeRoot);
        upperBlockRenderer = CreateMarker("UpperBlockPoint", GrimmForestMapLayout.UpperBlockCell, new Color(0.75f, 0.45f, 0.2f, 0.55f), 3, 0.42f, routeRoot);
        lowerBlockRenderer = CreateMarker("LowerBlockPoint", GrimmForestMapLayout.LowerBlockCell, new Color(0.75f, 0.45f, 0.2f, 0.55f), 3, 0.42f, routeRoot);

        RefreshVisuals();
    }

    void Update()
    {
        if (activeBlock == RouteBlockType.None)
            return;

        if (Time.time < blockEndTime)
            return;

        activeBlock = RouteBlockType.None;
        blockCooldownEndTime = Time.time + BlockCooldownSeconds;
        routeCache.Clear();
        RefreshVisuals();
        OnRouteStateChanged?.Invoke();
    }

    public bool TrySetForkMode(ForkRouteMode mode)
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return false;

        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return false;

        if (mode == forkMode)
            return false;

        if (!GameManager.Instance.TrySpendGold(ForkSwitchGoldCost))
            return false;

        forkMode = mode;
        routeCache.Clear();
        RefreshVisuals();
        OnRouteStateChanged?.Invoke();
        return true;
    }

    public bool TryActivateBlock(RouteBlockType blockType)
    {
        if (blockType is RouteBlockType.None)
            return false;

        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return false;

        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return false;

        if (!IsBlockReady)
            return false;

        if (!GameManager.Instance.TrySpendGold(BlockGoldCost))
            return false;

        activeBlock = blockType;
        blockEndTime = Time.time + BlockDurationSeconds;
        routeCache.Clear();
        RefreshVisuals();
        OnRouteStateChanged?.Invoke();
        return true;
    }

    public bool TryHandleClick(Vector3 worldPoint)
    {
        if (IsNearCell(worldPoint, GrimmForestMapLayout.ForkGateCell))
        {
            RouteControlUI.Instance?.OpenForkPanel();
            return RouteControlUI.Instance != null;
        }

        if (IsNearCell(worldPoint, GrimmForestMapLayout.UpperBlockCell))
        {
            RouteControlUI.Instance?.OpenBlockPanel(RouteBlockType.UpperScenic);
            return RouteControlUI.Instance != null;
        }

        if (IsNearCell(worldPoint, GrimmForestMapLayout.LowerBlockCell))
        {
            RouteControlUI.Instance?.OpenBlockPanel(RouteBlockType.LowerScenic);
            return RouteControlUI.Instance != null;
        }

        return false;
    }

    static bool IsNearCell(Vector3 worldPoint, Vector2Int cell)
    {
        return Vector2.Distance(worldPoint, MapGridSettings.GridToWorld(cell.x, cell.y)) <= MarkerClickRadius;
    }

    public ForkBranchChoice ResolveForkBranch(int spawnCounter)
    {
        return forkMode switch
        {
            ForkRouteMode.None => ForkBranchChoice.Central,
            ForkRouteMode.ForceUpper => ResolveUpperBranch(),
            ForkRouteMode.ForceLower => ResolveLowerBranch(),
            _ => ForkBranchChoice.Central
        };
    }

    ForkBranchChoice ResolveUpperBranch()
    {
        return activeBlock == RouteBlockType.UpperScenic
            ? ForkBranchChoice.Central
            : ForkBranchChoice.UpperScenic;
    }

    ForkBranchChoice ResolveLowerBranch()
    {
        return activeBlock == RouteBlockType.LowerScenic
            ? ForkBranchChoice.Central
            : ForkBranchChoice.LowerScenic;
    }

    public WaypointPath GetRoute(int spawnIndex, ForkBranchChoice branch)
    {
        var clampedSpawn = Mathf.Clamp(spawnIndex, 0, 1);
        var routeKey = BuildRouteKey(clampedSpawn, branch, activeBlock, forkMode);

        if (routeCache.TryGetValue(routeKey, out var cached) && cached != null)
            return cached;

        var routeObject = new GameObject($"Route_S{clampedSpawn}_{branch}_{forkMode}_{activeBlock}");
        routeObject.transform.SetParent(routeRoot != null ? routeRoot : transform, false);
        var route = routeObject.AddComponent<WaypointPath>();
        var gridPoints = GrimmForestMapLayout.BuildRouteCells(clampedSpawn, branch, activeBlock);
        route.SetWaypoints(ToWorldPoints(gridPoints));
        routeCache[routeKey] = route;
        return route;
    }

    public WaypointPath GetRoute(int spawnIndex, bool takeUpperFork)
    {
        var branch = takeUpperFork ? ForkBranchChoice.UpperScenic : ForkBranchChoice.LowerScenic;
        return GetRoute(spawnIndex, branch);
    }

    static int BuildRouteKey(int spawnIndex, ForkBranchChoice branch, RouteBlockType blockType, ForkRouteMode fork)
    {
        return spawnIndex * 10000 + (int)fork * 100 + (int)branch * 10 + (int)blockType;
    }

    static IEnumerable<Vector3> ToWorldPoints(IEnumerable<Vector2Int> gridPoints)
    {
        foreach (var gridPoint in gridPoints)
            yield return MapGridSettings.GridToWorld(gridPoint.x, gridPoint.y);
    }

    public string GetForkStatusText()
    {
        var modeLabel = forkMode switch
        {
            ForkRouteMode.ForceUpper => "Upper",
            ForkRouteMode.ForceLower => "Lower",
            _ => "None (open path)"
        };

        return $"Fork: {modeLabel}";
    }

    public string GetBlockStatusText()
    {
        if (activeBlock != RouteBlockType.None)
        {
            var label = activeBlock == RouteBlockType.UpperScenic ? "North scenic" : "South scenic";
            return $"Block: {label} {Mathf.CeilToInt(BlockTimeRemaining)}s";
        }

        if (BlockCooldownRemaining > 0f)
            return $"Block CD: {Mathf.CeilToInt(BlockCooldownRemaining)}s";

        return "Block: ready";
    }

    void RefreshVisuals()
    {
        if (forkGateRenderer == null)
            return;

        forkGateRenderer.gameObject.SetActive(true);
        forkGateRenderer.color = forkMode switch
        {
            ForkRouteMode.ForceUpper => new Color(0.45f, 0.95f, 0.55f, 0.95f),
            ForkRouteMode.ForceLower => new Color(0.95f, 0.75f, 0.35f, 0.95f),
            _ => new Color(0.35f, 0.85f, 0.95f, 0.42f)
        };

        RefreshBlockMarker(upperBlockRenderer, RouteBlockType.UpperScenic);
        RefreshBlockMarker(lowerBlockRenderer, RouteBlockType.LowerScenic);
    }

    void RefreshBlockMarker(SpriteRenderer renderer, RouteBlockType blockType)
    {
        if (renderer == null)
            return;

        if (activeBlock == blockType)
            renderer.color = new Color(0.95f, 0.35f, 0.25f, 0.95f);
        else if (IsBlockReady)
            renderer.color = new Color(0.85f, 0.55f, 0.15f, 0.75f);
        else
            renderer.color = new Color(0.35f, 0.35f, 0.35f, 0.45f);
    }

    static SpriteRenderer CreateMarker(
        string objectName,
        Vector2Int gridCell,
        Color color,
        int sortingOrder,
        float scale,
        Transform parent)
    {
        var marker = new GameObject(objectName);
        marker.transform.SetParent(parent, false);
        marker.transform.position = MapGridSettings.GridToWorld(gridCell.x, gridCell.y);
        marker.transform.localScale = Vector3.one * scale;

        var renderer = marker.AddComponent<SpriteRenderer>();
        renderer.sprite = MapGridControllerShared.GetWhiteSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }
}
