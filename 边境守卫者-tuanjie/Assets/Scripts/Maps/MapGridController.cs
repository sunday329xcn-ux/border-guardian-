using System.Collections.Generic;
using UnityEngine;

public class MapGridController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform tilesRoot;
    [SerializeField] Transform buildSlotsRoot;
    [SerializeField] Transform markersRoot;
    [SerializeField] WaypointPath waypointPath;

    [Header("Camera")]
    [SerializeField] bool autoSetupCamera = true;
    [SerializeField] float cameraOrthographicSize = 8.5f;
    [SerializeField] float cameraVerticalUiOffset = 0.35f;

    readonly List<BuildSlot> buildSlots = new();
    MapCellType[,] cells;
    MapEnvironmentController environmentController;
    MapRouteController routeController;

    public WaypointPath Path => routeController != null
        ? routeController.GetRoute(0, true)
        : waypointPath;
    public IReadOnlyList<BuildSlot> BuildSlots => buildSlots;
    public MapCellType[,] Cells => cells;
    public MapEnvironmentController Environment => environmentController;
    public MapRouteController Route => routeController;

    void Awake()
    {
        EnsureRoots();
        cells = GrimmForestMapLayout.CreateCells();
        BuildTiles();
        BuildBuildSlots();
        BuildRouteController();
        BuildPathPreview();
        BuildMarkers();
        BuildEnvironment();
        ApplyCameraFraming();
        SnapAllBuildSlotPositions();
    }

    void OnEnable()
    {
        if (autoSetupCamera && cells != null)
            ApplyCameraFraming(repositionChildren: false);
    }

    void EnsureRoots()
    {
        if (tilesRoot == null)
        {
            var root = new GameObject("Tiles");
            root.transform.SetParent(transform);
            tilesRoot = root.transform;
        }

        if (buildSlotsRoot == null)
        {
            var root = new GameObject("BuildSlots");
            root.transform.SetParent(transform);
            buildSlotsRoot = root.transform;
        }

        if (markersRoot == null)
        {
            var root = new GameObject("Markers");
            root.transform.SetParent(transform);
            markersRoot = root.transform;
        }

        if (waypointPath == null)
        {
            var pathObject = new GameObject("WaypointPath");
            pathObject.transform.SetParent(transform);
            waypointPath = pathObject.AddComponent<WaypointPath>();
        }
    }

    void BuildTiles()
    {
        for (int x = 0; x < MapGridSettings.Width; x++)
        for (int y = 0; y < MapGridSettings.Height; y++)
        {
            var cellType = cells[x, y];
            var tile = CreateSpriteObject($"Tile_{x}_{y}", tilesRoot);
            MapGridLayoutUtility.SnapTransformToCell(tile.transform, new Vector2Int(x, y));
            tile.transform.localScale = Vector3.one * 0.96f;

            var renderer = tile.GetComponent<SpriteRenderer>();
            renderer.color = GetCellColor(cellType);
            renderer.sortingOrder = 0;
        }
    }

    void BuildBuildSlots()
    {
        buildSlots.Clear();

        foreach (var gridPos in GrimmForestMapLayout.GetInitialBuildPlatformCells())
            buildSlots.Add(CreateBuildSlot(gridPos, GrimmForestMapLayout.GetPlatformTerrain(gridPos)));

        Debug.Log($"Grimm Forest build slots: {buildSlots.Count} / {GrimmForestMapLayout.TotalBuildSlotCount} (latent unlocks pending)");
    }

    public void UnlockBuildPlatform(Vector2Int gridPos, PlatformTerrainType terrainType)
    {
        if (cells == null || !MapGridSettings.IsInsideGrid(gridPos.x, gridPos.y))
            return;

        foreach (var existing in buildSlots)
        {
            if (existing.GridPosition == gridPos)
                return;
        }

        cells[gridPos.x, gridPos.y] = MapCellType.BuildPlatform;
        UpdateTileVisual(gridPos.x, gridPos.y, MapCellType.BuildPlatform);
        buildSlots.Add(CreateBuildSlot(gridPos, terrainType));
        SnapAllBuildSlotPositions();
    }

    public void SnapAllBuildSlotPositions()
    {
        MapGridLayoutUtility.SnapOccupiedTowers(buildSlots);
    }

    BuildSlot CreateBuildSlot(Vector2Int gridPos, PlatformTerrainType terrainType)
    {
        var x = gridPos.x;
        var y = gridPos.y;
        var slotObject = CreateSpriteObject($"BuildSlot_{x}_{y}", buildSlotsRoot);
        MapGridLayoutUtility.SnapTransformToCell(slotObject.transform, gridPos);
        slotObject.transform.localScale = Vector3.one * 0.72f;

        var baseRenderer = slotObject.GetComponent<SpriteRenderer>();
        baseRenderer.sortingOrder = 1;

        var highlight = CreateSpriteObject("Highlight", slotObject.transform);
        highlight.transform.localPosition = Vector3.zero;
        highlight.transform.localScale = Vector3.one * 1.15f;
        var highlightRenderer = highlight.GetComponent<SpriteRenderer>();
        highlightRenderer.color = new Color(1f, 0.92f, 0.2f, 0.35f);
        highlightRenderer.sortingOrder = 2;
        highlightRenderer.enabled = false;

        var selectionHighlight = CreateSpriteObject("SelectionHighlight", slotObject.transform);
        selectionHighlight.transform.localPosition = Vector3.zero;
        selectionHighlight.transform.localScale = Vector3.one * 1.22f;
        var selectionRenderer = selectionHighlight.GetComponent<SpriteRenderer>();
        selectionRenderer.color = new Color(0.35f, 0.85f, 0.95f, 0.55f);
        selectionRenderer.sortingOrder = 3;
        selectionRenderer.enabled = false;

        var slot = slotObject.AddComponent<BuildSlot>();
        slot.Initialize(gridPos, true, baseRenderer, highlightRenderer, selectionRenderer, terrainType);

        var collider = slotObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;
        collider.isTrigger = true;

        return slot;
    }

    void UpdateTileVisual(int x, int y, MapCellType cellType)
    {
        if (tilesRoot == null)
            return;

        var tile = tilesRoot.Find($"Tile_{x}_{y}");
        if (tile == null)
            return;

        var renderer = tile.GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.color = GetCellColor(cellType);
    }

    void BuildRouteController()
    {
        routeController = GetComponent<MapRouteController>();
        if (routeController == null)
            routeController = gameObject.AddComponent<MapRouteController>();

        routeController.BuildRouteControls(transform);
    }

    void BuildPathPreview()
    {
        if (waypointPath != null && routeController != null)
            waypointPath.SetWaypoints(routeController.GetRoute(0, true).Waypoints);
    }

    public WaypointPath GetSpawnRoute(int spawnIndex, bool takeUpperFork)
    {
        if (routeController != null)
            return routeController.GetRoute(spawnIndex, takeUpperFork);

        var clampedSpawn = Mathf.Clamp(spawnIndex, 0, 1);
        var routeIndex = clampedSpawn * 2 + (takeUpperFork ? 0 : 1);
        return waypointPath;
    }

    void BuildEnvironment()
    {
        environmentController = GetComponent<MapEnvironmentController>();
        if (environmentController == null)
            environmentController = gameObject.AddComponent<MapEnvironmentController>();

        environmentController.BuildEnvironment(transform);

        if (GetComponent<GoblinMissileController>() == null)
            gameObject.AddComponent<GoblinMissileController>();

        if (GetComponent<EasterEggController>() == null)
            gameObject.AddComponent<EasterEggController>();
    }

    void BuildMarkers()
    {
        CreateMarker("SpawnMarker_Upper", GrimmForestMapLayout.UpperSpawnCell, new Color(0.3f, 0.6f, 1f), 3);
        CreateMarker("SpawnMarker_Lower", GrimmForestMapLayout.LowerSpawnCell, new Color(0.35f, 0.55f, 0.95f), 3);
        CreateMarker("GoalMarker", GrimmForestMapLayout.GoalCell, new Color(1f, 0.35f, 0.35f), 3);
    }

    void CreateMarker(string markerName, Vector2Int gridCell, Color color, int sortingOrder)
    {
        var marker = CreateSpriteObject(markerName, markersRoot);
        MapGridLayoutUtility.SnapTransformToCell(marker.transform, gridCell);
        marker.transform.localScale = Vector3.one * 0.45f;

        var renderer = marker.GetComponent<SpriteRenderer>();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }

    void ApplyCameraFraming(bool repositionChildren = true)
    {
        if (repositionChildren)
            transform.localPosition = Vector3.zero;

        var camera = Camera.main;
        if (camera == null)
            return;

        camera.orthographic = true;
        camera.orthographicSize = cameraOrthographicSize;
        var mapCenterX = MapGridSettings.Width * MapGridSettings.CellSize * 0.5f;
        camera.transform.position = new Vector3(
            mapCenterX - UiDisplaySettings.MapCameraHorizontalShift,
            MapGridSettings.Height * MapGridSettings.CellSize * 0.5f + cameraVerticalUiOffset,
            -10f);
        camera.backgroundColor = new Color(0.12f, 0.16f, 0.12f);

        var shake = camera.GetComponent<CameraShakeController>();
        if (shake != null)
            shake.SyncOrigin();
    }

    static GameObject CreateSpriteObject(string objectName, Transform parent)
    {
        var go = new GameObject(objectName);
        go.transform.SetParent(parent);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = MapGridControllerShared.GetWhiteSprite();
        return go;
    }

    static Color GetCellColor(MapCellType cellType)
    {
        return cellType switch
        {
            MapCellType.Path => new Color(0.72f, 0.58f, 0.36f),
            MapCellType.BuildGround => new Color(0.28f, 0.48f, 0.28f),
            MapCellType.BuildPlatform => new Color(0.42f, 0.45f, 0.48f),
            MapCellType.Blocked => new Color(0.18f, 0.24f, 0.18f),
            _ => new Color(0.22f, 0.34f, 0.22f)
        };
    }
}
