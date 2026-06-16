using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grimm Forest: 16x12 grid, dual spawns, merge trunk, fork junction, platform slots on both road sides.
/// </summary>
public static class GrimmForestMapLayout
{
    public static readonly Vector2Int UpperSpawnCell = new(0, 9);
    public static readonly Vector2Int LowerSpawnCell = new(0, 3);
    public static readonly Vector2Int GoalCell = new(15, 6);

    public static Vector2Int SpawnCell => UpperSpawnCell;

    public static IReadOnlyList<Vector2Int> SpawnCells { get; } = new[]
    {
        UpperSpawnCell,
        LowerSpawnCell
    };

    static readonly Vector2Int[] UpperEntryPath =
    {
        new(0, 9), new(1, 9), new(2, 9), new(3, 9), new(4, 9), new(5, 9), new(6, 9),
        new(6, 8), new(6, 7), new(6, 6)
    };

    static readonly Vector2Int[] LowerEntryPath =
    {
        new(0, 3), new(1, 3), new(2, 3), new(3, 3), new(4, 3), new(5, 3), new(6, 3),
        new(6, 4), new(6, 5), new(6, 6)
    };

    static readonly Vector2Int[] SharedTrunkPath =
    {
        new(7, 6), new(8, 6), new(9, 6)
    };

    static readonly Vector2Int[] UpperForkBranch =
    {
        new(9, 7), new(10, 7), new(11, 7), new(12, 7), new(12, 6)
    };

    static readonly Vector2Int[] LowerForkBranch =
    {
        new(9, 5), new(10, 5), new(11, 5), new(12, 5), new(12, 6)
    };

    static readonly Vector2Int[] ExitPath =
    {
        new(13, 6), new(14, 6), new(15, 6)
    };

    static readonly Vector2Int[] BuildPlatformCells =
    {
        new(1, 10), new(3, 10), new(5, 10),
        new(1, 8), new(3, 8),
        new(1, 2), new(3, 2), new(5, 2),
        new(1, 4), new(3, 4),
        new(5, 7), new(7, 7), new(8, 7),
        new(7, 5), new(8, 5),
        new(10, 8), new(11, 8),
        new(10, 4), new(11, 4),
        new(13, 7), new(14, 5)
    };

    static readonly Vector2Int[] BlockedCells =
    {
        new(0, 11), new(1, 11), new(15, 11), new(14, 11),
        new(0, 0), new(1, 0), new(15, 0), new(14, 0)
    };

    public static int BuildSlotCount => BuildPlatformCells.Length;

    public static readonly Vector2Int[] AncientTreeCells =
    {
        new(4, 10),
        new(12, 10)
    };

    public static readonly Vector2Int[] AncientTreeEffectCells =
    {
        new(5, 9),
        new(10, 7)
    };

    public static readonly Vector2Int[] HunterTrapPlacementCells =
    {
        new(8, 6),
        new(10, 7),
        new(10, 5)
    };

    public static MapCellType[,] CreateCells()
    {
        var cells = new MapCellType[MapGridSettings.Width, MapGridSettings.Height];

        for (int x = 0; x < MapGridSettings.Width; x++)
        for (int y = 0; y < MapGridSettings.Height; y++)
            cells[x, y] = MapCellType.Grass;

        foreach (var point in GetAllPathCells())
            cells[point.x, point.y] = MapCellType.Path;

        foreach (var point in BuildPlatformCells)
            cells[point.x, point.y] = MapCellType.BuildPlatform;

        foreach (var point in BlockedCells)
            cells[point.x, point.y] = MapCellType.Blocked;

        return cells;
    }

    public static Vector2Int[] CreatePathWaypoints(int spawnIndex, bool takeUpperFork)
    {
        var entry = spawnIndex == 0 ? UpperEntryPath : LowerEntryPath;
        var fork = takeUpperFork ? UpperForkBranch : LowerForkBranch;
        return CombineSegments(entry, SharedTrunkPath, fork, ExitPath);
    }

    public static Vector2Int[] CreatePathWaypoints()
    {
        return CreatePathWaypoints(0, true);
    }

    public static IEnumerable<Vector2Int> GetAllPathCells()
    {
        var seen = new HashSet<Vector2Int>();
        AddUnique(seen, UpperEntryPath);
        AddUnique(seen, LowerEntryPath);
        AddUnique(seen, SharedTrunkPath);
        AddUnique(seen, UpperForkBranch);
        AddUnique(seen, LowerForkBranch);
        AddUnique(seen, ExitPath);
        return seen;
    }

    static void AddUnique(HashSet<Vector2Int> seen, IEnumerable<Vector2Int> cells)
    {
        foreach (var cell in cells)
            seen.Add(cell);
    }

    static Vector2Int[] CombineSegments(params Vector2Int[][] segments)
    {
        var combined = new List<Vector2Int>();

        foreach (var segment in segments)
        {
            foreach (var point in segment)
            {
                if (combined.Count > 0 && combined[^1] == point)
                    continue;

                combined.Add(point);
            }
        }

        return combined.ToArray();
    }
}
