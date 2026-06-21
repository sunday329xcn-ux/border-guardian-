using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grimm Forest v2: 20×14 asymmetric dual-spawn map with fork gate and scenic detours.
/// </summary>
public static class GrimmForestMapLayout
{
    public static readonly Vector2Int UpperSpawnCell = new(0, 12);
    public static readonly Vector2Int LowerSpawnCell = new(0, 1);
    public static readonly Vector2Int GoalCell = new(19, 7);
    public static readonly Vector2Int EasterEggCornerCell = new(19, 0);
    public static readonly Vector2Int ForkGateCell = new(10, 7);
    public static readonly Vector2Int UpperBlockCell = new(13, 9);
    public static readonly Vector2Int LowerBlockCell = new(13, 5);

    public static Vector2Int SpawnCell => UpperSpawnCell;

    public static IReadOnlyList<Vector2Int> SpawnCells { get; } = new[]
    {
        UpperSpawnCell,
        LowerSpawnCell
    };

    /// <summary>Longer northern route — favors fast / air waves from upper spawn.</summary>
    static readonly Vector2Int[] UpperEntryPath =
    {
        new(0, 12), new(1, 12), new(2, 12), new(3, 12), new(4, 12), new(5, 12),
        new(6, 12), new(7, 12), new(7, 11), new(7, 10), new(8, 10), new(9, 10),
        new(9, 9), new(10, 8), new(10, 7)
    };

    /// <summary>Shorter southern route with early choke — heavier ground pressure.</summary>
    static readonly Vector2Int[] LowerEntryPath =
    {
        new(0, 1), new(1, 1), new(2, 1), new(3, 1), new(4, 1), new(5, 1),
        new(6, 1), new(7, 1), new(8, 1), new(8, 2), new(8, 3), new(9, 3),
        new(9, 4), new(9, 5), new(9, 6), new(10, 6), new(10, 7)
    };

    static readonly Vector2Int[] UpperForkPrimary =
    {
        new(11, 8), new(12, 9), new(13, 9), new(14, 9), new(15, 8), new(16, 8), new(17, 7)
    };

    static readonly Vector2Int[] UpperForkDetour =
    {
        new(11, 7), new(12, 7), new(13, 7), new(14, 7), new(15, 7), new(16, 7), new(17, 7)
    };

    static readonly Vector2Int[] LowerForkPrimary =
    {
        new(11, 6), new(12, 5), new(13, 5), new(14, 5), new(15, 6), new(16, 6), new(17, 7)
    };

    static readonly Vector2Int[] LowerForkDetour =
    {
        new(11, 6), new(12, 6), new(13, 6), new(14, 6), new(15, 6), new(16, 7), new(17, 7)
    };

    static readonly Vector2Int[] CentralTrunkPath =
    {
        new(11, 7), new(12, 7), new(13, 7), new(14, 7), new(15, 7), new(16, 7), new(17, 7)
    };

    static readonly Vector2Int[] ExitPath =
    {
        new(18, 7), new(19, 7)
    };

    static readonly Vector2Int[] BuildPlatformCells =
    {
        new(2, 13), new(4, 13), new(6, 13), new(8, 11),
        new(2, 0), new(4, 0), new(6, 0), new(5, 2),
        new(7, 8), new(9, 8), new(11, 10), new(11, 3), new(11, 4),
        new(12, 8), new(12, 6), new(14, 8), new(14, 4),
        new(16, 9), new(16, 4), new(18, 8), new(18, 6), new(15, 10)
    };

    public static readonly LatentPlatformDefinition[] LatentPlatforms =
    {
        new(6, new Vector2Int(10, 9), PlatformTerrainType.RuneRange),
        new(6, new Vector2Int(10, 5), PlatformTerrainType.RuneAttackSpeed),
        new(11, new Vector2Int(17, 9), PlatformTerrainType.Highland),
        new(11, new Vector2Int(17, 5), PlatformTerrainType.RuneSynergy)
    };

    public static int BuildSlotCount => BuildPlatformCells.Length;

    public static int TotalBuildSlotCount => BuildPlatformCells.Length + LatentPlatforms.Length;

    static readonly Vector2Int[] BlockedCells =
    {
        new(0, 13), new(1, 13), new(19, 13), new(18, 13),
        new(0, 0), new(1, 0), new(19, 0), new(18, 0)
    };

    public static PlatformTerrainType GetPlatformTerrain(Vector2Int cell)
    {
        foreach (var assignment in PlatformTerrainCatalog.GetGrimmForestAssignments())
        {
            if (assignment.Cell == cell)
                return assignment.Terrain;
        }

        return PlatformTerrainType.Standard;
    }

    public static readonly Vector2Int[] AncientTreeCells =
    {
        new(5, 13),
        new(15, 10)
    };

    public static readonly Vector2Int[] AncientTreeEffectCells =
    {
        new(6, 12),
        new(14, 9)
    };

    public static readonly Vector2Int[] HunterTrapPlacementCells =
    {
        new(9, 7),
        new(13, 9),
        new(13, 5)
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

    public static Vector2Int[] GetInitialBuildPlatformCells() => BuildPlatformCells;

    public static Vector2Int[] BuildRouteCells(int spawnIndex, ForkBranchChoice branch, RouteBlockType blockType)
    {
        var entry = spawnIndex == 0 ? UpperEntryPath : LowerEntryPath;
        var fork = ResolveForkSegment(branch, blockType);
        return CombineSegments(entry, fork, ExitPath);
    }

    public static Vector2Int[] BuildRouteCells(int spawnIndex, bool takeUpperFork, RouteBlockType blockType)
    {
        var branch = takeUpperFork ? ForkBranchChoice.UpperScenic : ForkBranchChoice.LowerScenic;
        return BuildRouteCells(spawnIndex, branch, blockType);
    }

    public static Vector2Int[] CreatePathWaypoints(int spawnIndex, bool takeUpperFork)
    {
        return BuildRouteCells(spawnIndex, takeUpperFork, RouteBlockType.None);
    }

    public static Vector2Int[] CreatePathWaypoints()
    {
        return CreatePathWaypoints(0, true);
    }

    static Vector2Int[] ResolveForkSegment(ForkBranchChoice branch, RouteBlockType blockType)
    {
        if (branch == ForkBranchChoice.Central)
            return PrependJunction(CentralTrunkPath);

        if (branch == ForkBranchChoice.UpperScenic)
        {
            return blockType == RouteBlockType.UpperScenic
                ? PrependJunction(UpperForkDetour)
                : PrependJunction(UpperForkPrimary);
        }

        return blockType == RouteBlockType.LowerScenic
            ? PrependJunction(LowerForkDetour)
            : PrependJunction(LowerForkPrimary);
    }

    static Vector2Int[] ResolveForkSegment(bool takeUpperFork, RouteBlockType blockType)
    {
        return ResolveForkSegment(
            takeUpperFork ? ForkBranchChoice.UpperScenic : ForkBranchChoice.LowerScenic,
            blockType);
    }

    static Vector2Int[] PrependJunction(Vector2Int[] forkSegment)
    {
        var combined = new Vector2Int[forkSegment.Length + 1];
        combined[0] = ForkGateCell;
        for (int i = 0; i < forkSegment.Length; i++)
            combined[i + 1] = forkSegment[i];

        return combined;
    }

    public static IEnumerable<Vector2Int> GetAllPathCells()
    {
        var seen = new HashSet<Vector2Int>();
        AddUnique(seen, UpperEntryPath);
        AddUnique(seen, LowerEntryPath);
        AddUnique(seen, CentralTrunkPath);
        AddUnique(seen, UpperForkPrimary);
        AddUnique(seen, UpperForkDetour);
        AddUnique(seen, LowerForkPrimary);
        AddUnique(seen, LowerForkDetour);
        AddUnique(seen, ExitPath);
        seen.Add(ForkGateCell);
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

public readonly struct LatentPlatformDefinition
{
    public LatentPlatformDefinition(int unlockAtWave, Vector2Int cell, PlatformTerrainType terrain)
    {
        UnlockAtWave = unlockAtWave;
        Cell = cell;
        Terrain = terrain;
    }

    public int UnlockAtWave { get; }
    public Vector2Int Cell { get; }
    public PlatformTerrainType Terrain { get; }
}
