using UnityEngine;

/// <summary>
/// 共享白色 Sprite，供地图块与敌人占位图使用。
/// </summary>
public static class MapGridControllerShared
{
    static Sprite whiteSprite;

    public static Sprite GetWhiteSprite()
    {
        if (whiteSprite != null) return whiteSprite;

        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        whiteSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return whiteSprite;
    }
}
