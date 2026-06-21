using UnityEngine;

public static class MapGridSettings
{
    public const int Width = 20;
    public const int Height = 14;
    public const float CellSize = 1f;

    public static Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(
            x * CellSize + CellSize * 0.5f,
            y * CellSize + CellSize * 0.5f,
            0f);
    }

    public static bool IsInsideGrid(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public static Vector2Int WorldToGrid(Vector3 worldPoint)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPoint.x / CellSize),
            Mathf.FloorToInt(worldPoint.y / CellSize));
    }

    public static bool IsPointInCell(Vector3 worldPoint, Vector2Int cell, float radius = 0.55f)
    {
        var cellCenter = GridToWorld(cell.x, cell.y);
        return Vector2.Distance(cellCenter, worldPoint) <= radius;
    }
}
