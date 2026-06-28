using UnityEngine;

/// <summary>Geometric primitive kinds available to the procedural visual layer.</summary>
public enum VisualShape
{
    Square,
    Circle,
    Triangle,
    Diamond,
    Ring,
    SoftShadow
}

/// <summary>
/// Helper for spawning child decoration SpriteRenderers used by the zero-art
/// composers (P-A1/P-A4/P-A5/P-A6). Keeps logic scripts free of GameObject
/// plumbing so a future real-art pass only needs to swap sprites here.
/// </summary>
public static class VisualPrimitives
{
    public static Sprite SpriteFor(VisualShape shape) => shape switch
    {
        VisualShape.Circle => ProceduralSpriteFactory.GetCircleSprite(),
        VisualShape.Triangle => ProceduralSpriteFactory.GetTriangleSprite(),
        VisualShape.Diamond => ProceduralSpriteFactory.GetDiamondSprite(),
        VisualShape.Ring => ProceduralSpriteFactory.GetRingSprite(),
        VisualShape.SoftShadow => ProceduralSpriteFactory.GetSoftShadowSprite(),
        _ => ProceduralSpriteFactory.GetWhiteSprite()
    };

    public static SpriteRenderer Add(Transform parent, string name, VisualShape shape, Color color,
        Vector2 localScale, Vector2 localPosition, int sortingOrder, float rotationZ = 0f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = new Vector3(localScale.x, localScale.y, 1f);
        go.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = SpriteFor(shape);
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    /// <summary>Creates (or clears + reuses) a named child container under a unit root.</summary>
    public static Transform EnsureContainer(Transform parent, string name)
    {
        var existing = parent.Find(name);
        if (existing != null)
        {
            for (int i = existing.childCount - 1; i >= 0; i--)
                Object.Destroy(existing.GetChild(i).gameObject);
            return existing;
        }

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;
        return go.transform;
    }
}
