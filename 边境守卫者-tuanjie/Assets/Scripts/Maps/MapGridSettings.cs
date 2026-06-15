using UnityEngine;

public static class MapGridSettings
{
    public const int Width = 16;
    public const int Height = 12;
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
}
