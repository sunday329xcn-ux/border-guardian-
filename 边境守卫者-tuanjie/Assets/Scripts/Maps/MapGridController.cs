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
    [SerializeField] float cameraOrthographicSize = 7f;

    readonly List<BuildSlot> buildSlots = new();
    readonly WaypointPath[] spawnRoutes = new WaypointPath[4];
    MapCellType[,] cells;
    MapEnvironmentController environmentController;

    public WaypointPath Path => spawnRoutes.Length > 0 ? spawnRoutes[0] : waypointPath;
    public IReadOnlyList<BuildSlot> BuildSlots => buildSlots;
    public MapCellType[,] Cells => cells;
    public MapEnvironmentController Environment => environmentController;

    void Awake()
    {
        EnsureRoots();
        cells = GrimmForestMapLayout.CreateCells();
        BuildTiles();
        BuildBuildSlots();
        BuildPath();
        BuildMarkers();
        BuildEnvironment();

        if (autoSetupCamera)
            SetupCamera();
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
            tile.transform.position = MapGridSettings.GridToWorld(x, y);
            tile.transform.localScale = Vector3.one * 0.96f;

            var renderer = tile.GetComponent<SpriteRenderer>();
            renderer.color = GetCellColor(cellType);
            renderer.sortingOrder = 0;
        }
    }

    void BuildBuildSlots()
    {
        buildSlots.Clear();

        for (int x = 0; x < MapGridSettings.Width; x++)
        for (int y = 0; y < MapGridSettings.Height; y++)
        {
            var cellType = cells[x, y];
            if (cellType != MapCellType.BuildPlatform)
                continue;

            var slotObject = CreateSpriteObject($"BuildSlot_{x}_{y}", buildSlotsRoot);
            slotObject.transform.position = MapGridSettings.GridToWorld(x, y);
            slotObject.transform.localScale = Vector3.one * 0.72f;

            var baseRenderer = slotObject.GetComponent<SpriteRenderer>();
            baseRenderer.color = new Color(0.55f, 0.58f, 0.62f);
            baseRenderer.sortingOrder = 1;

            var highlight = CreateSpriteObject("Highlight", slotObject.transform);
            highlight.transform.localPosition = Vector3.zero;
            highlight.transform.localScale = Vector3.one * 1.15f;
            var highlightRenderer = highlight.GetComponent<SpriteRenderer>();
            highlightRenderer.color = new Color(1f, 0.92f, 0.2f, 0.35f);
            highlightRenderer.sortingOrder = 2;
            highlightRenderer.enabled = false;

            var slot = slotObject.AddComponent<BuildSlot>();
            slot.Initialize(new Vector2Int(x, y), true, highlightRenderer);

            var collider = slotObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
            collider.isTrigger = true;

            buildSlots.Add(slot);
        }

        Debug.Log($"Grimm Forest build slots: {buildSlots.Count} / {GrimmForestMapLayout.BuildSlotCount}");
    }

    void BuildPath()
    {
        for (int spawnIndex = 0; spawnIndex < 2; spawnIndex++)
        {
            for (int forkIndex = 0; forkIndex < 2; forkIndex++)
            {
                var routeIndex = spawnIndex * 2 + forkIndex;
                var routeObject = new GameObject($"Route_S{spawnIndex}_F{forkIndex}");
                routeObject.transform.SetParent(transform);
                var route = routeObject.AddComponent<WaypointPath>();
                var gridPoints = GrimmForestMapLayout.CreatePathWaypoints(spawnIndex, forkIndex == 0);
                route.SetWaypoints(ToWorldPoints(gridPoints));
                spawnRoutes[routeIndex] = route;
            }
        }

        if (waypointPath != null)
        {
            waypointPath.SetWaypoints(spawnRoutes[0].Waypoints);
        }
        else
        {
            var pathObject = new GameObject("WaypointPath");
            pathObject.transform.SetParent(transform);
            waypointPath = pathObject.AddComponent<WaypointPath>();
            waypointPath.SetWaypoints(spawnRoutes[0].Waypoints);
        }
    }

    public WaypointPath GetSpawnRoute(int spawnIndex, bool takeUpperFork)
    {
        var clampedSpawn = Mathf.Clamp(spawnIndex, 0, 1);
        var routeIndex = clampedSpawn * 2 + (takeUpperFork ? 0 : 1);
        return spawnRoutes[routeIndex] ?? Path;
    }

    static IEnumerable<Vector3> ToWorldPoints(IEnumerable<Vector2Int> gridPoints)
    {
        foreach (var gridPoint in gridPoints)
            yield return MapGridSettings.GridToWorld(gridPoint.x, gridPoint.y);
    }

    void BuildEnvironment()
    {
        environmentController = GetComponent<MapEnvironmentController>();
        if (environmentController == null)
            environmentController = gameObject.AddComponent<MapEnvironmentController>();

        environmentController.BuildEnvironment(transform);
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
        marker.transform.position = MapGridSettings.GridToWorld(gridCell.x, gridCell.y);
        marker.transform.localScale = Vector3.one * 0.45f;

        var renderer = marker.GetComponent<SpriteRenderer>();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }

    void SetupCamera()
    {
        var camera = Camera.main;
        if (camera == null) return;

        camera.orthographic = true;
        camera.orthographicSize = cameraOrthographicSize;
        camera.transform.position = new Vector3(
            MapGridSettings.Width * 0.5f - 0.5f,
            MapGridSettings.Height * 0.5f - 0.5f,
            -10f);
        camera.backgroundColor = new Color(0.12f, 0.16f, 0.12f);
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
