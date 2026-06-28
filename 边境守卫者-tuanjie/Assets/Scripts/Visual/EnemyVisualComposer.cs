using UnityEngine;

/// <summary>
/// Enemy silhouette composer (P-A5). Adds a per-type geometric accent on top of
/// the plain coloured body created in <c>EnemyBase.Spawn</c>, plus elite / boss
/// rings and the shared shadow + idle bob (P-A1). Colour comes from the body so
/// it always matches the Codex; the composer only contributes shape.
/// </summary>
public static class EnemyVisualComposer
{
    const float AccentZ = -0.05f;
    const float RingZ = 0.05f;

    public static SpriteRenderer Compose(SpriteRenderer body, EnemyType type, bool isElite, bool isBoss)
    {
        if (body == null)
            return null;

        body.sortingOrder = VisualSorting.Enemies;

        var root = body.transform;
        UnitVisualDecorator.Attach(body.gameObject, new Vector2(0.92f, 0.4f), enableBob: true);

        var decor = VisualPrimitives.EnsureContainer(root, "Decor");
        var accent = Color.Lerp(body.color, Color.white, 0.4f);
        var dark = Color.Lerp(body.color, Color.black, 0.35f);

        BuildSilhouette(decor, type, accent, dark);

        if (isBoss)
        {
            AddRing(decor, "BossRingOuter", 1.55f, VisualPalette.EliteAccent, 0.85f);
            AddRing(decor, "BossRingInner", 1.25f, new Color(1f, 0.5f, 0.2f, 0.8f), 0.85f);
        }
        else if (isElite)
        {
            AddRing(decor, "EliteRing", 1.35f, VisualPalette.EliteAccent, 0.8f);
        }

        SetDepth(decor);
        return body;
    }

    static void BuildSilhouette(Transform decor, EnemyType type, Color accent, Color dark)
    {
        switch (type)
        {
            case EnemyType.Imp:
                VisualPrimitives.Add(decor, "EarL", VisualShape.Triangle, dark, new Vector2(0.25f, 0.3f), new Vector2(-0.22f, 0.32f), VisualSorting.Enemies);
                VisualPrimitives.Add(decor, "EarR", VisualShape.Triangle, dark, new Vector2(0.25f, 0.3f), new Vector2(0.22f, 0.32f), VisualSorting.Enemies);
                break;
            case EnemyType.Orc:
                VisualPrimitives.Add(decor, "LegL", VisualShape.Square, dark, new Vector2(0.2f, 0.25f), new Vector2(-0.2f, -0.4f), VisualSorting.Enemies);
                VisualPrimitives.Add(decor, "LegR", VisualShape.Square, dark, new Vector2(0.2f, 0.25f), new Vector2(0.2f, -0.4f), VisualSorting.Enemies);
                break;
            case EnemyType.GoblinRipper:
                VisualPrimitives.Add(decor, "Blade", VisualShape.Triangle, accent, new Vector2(0.3f, 0.4f), new Vector2(0.28f, 0.1f), VisualSorting.Enemies, -35f);
                break;
            case EnemyType.Wraith:
                VisualPrimitives.Add(decor, "Veil", VisualShape.Diamond, new Color(accent.r, accent.g, accent.b, 0.6f), new Vector2(0.7f, 0.9f), Vector2.zero, VisualSorting.Enemies);
                break;
            case EnemyType.RockGolem:
                VisualPrimitives.Add(decor, "HandL", VisualShape.Square, dark, new Vector2(0.3f, 0.3f), new Vector2(-0.5f, 0f), VisualSorting.Enemies);
                VisualPrimitives.Add(decor, "HandR", VisualShape.Square, dark, new Vector2(0.3f, 0.3f), new Vector2(0.5f, 0f), VisualSorting.Enemies);
                break;
            case EnemyType.FireBomber:
                VisualPrimitives.Add(decor, "Fuse", VisualShape.Triangle, new Color(1f, 0.55f, 0.2f), new Vector2(0.4f, 0.4f), new Vector2(0f, 0.38f), VisualSorting.Enemies);
                break;
            case EnemyType.ShadowPriest:
                VisualPrimitives.Add(decor, "Hat", VisualShape.Triangle, dark, new Vector2(0.5f, 0.55f), new Vector2(0f, 0.42f), VisualSorting.Enemies);
                break;
            case EnemyType.WolfRider:
                VisualPrimitives.Add(decor, "Head", VisualShape.Circle, accent, new Vector2(0.32f, 0.32f), new Vector2(0.34f, 0.12f), VisualSorting.Enemies);
                break;
            case EnemyType.TowerBreaker:
                VisualPrimitives.Add(decor, "Hammer", VisualShape.Square, dark, new Vector2(0.65f, 0.18f), new Vector2(0f, 0.42f), VisualSorting.Enemies);
                break;
            case EnemyType.AncientDragon:
                VisualPrimitives.Add(decor, "WingL", VisualShape.Triangle, accent, new Vector2(0.6f, 0.5f), new Vector2(-0.55f, 0.15f), VisualSorting.Enemies, 60f);
                VisualPrimitives.Add(decor, "WingR", VisualShape.Triangle, accent, new Vector2(0.6f, 0.5f), new Vector2(0.55f, 0.15f), VisualSorting.Enemies, -60f);
                break;
            case EnemyType.ShieldBearer:
                VisualPrimitives.Add(decor, "Shield", VisualShape.Square, accent, new Vector2(0.22f, 0.55f), new Vector2(-0.42f, 0f), VisualSorting.Enemies);
                break;
            case EnemyType.SplitSlime:
                VisualPrimitives.Add(decor, "Blob", VisualShape.Circle, new Color(accent.r, accent.g, accent.b, 0.8f), new Vector2(0.6f, 0.5f), new Vector2(0f, -0.1f), VisualSorting.Enemies);
                break;
            case EnemyType.Shade:
                VisualPrimitives.Add(decor, "Veil", VisualShape.Diamond, new Color(dark.r, dark.g, dark.b, 0.7f), new Vector2(0.6f, 0.85f), Vector2.zero, VisualSorting.Enemies);
                break;
            case EnemyType.WarDrummer:
                VisualPrimitives.Add(decor, "Drum", VisualShape.Circle, dark, new Vector2(0.5f, 0.5f), new Vector2(0f, -0.05f), VisualSorting.Enemies);
                break;
            case EnemyType.Nullifier:
                VisualPrimitives.Add(decor, "Rune", VisualShape.Ring, accent, new Vector2(0.7f, 0.7f), Vector2.zero, VisualSorting.Enemies);
                break;
            case EnemyType.BatSwarm:
                VisualPrimitives.Add(decor, "WingL", VisualShape.Triangle, dark, new Vector2(0.4f, 0.25f), new Vector2(-0.32f, 0.1f), VisualSorting.Enemies, 90f);
                VisualPrimitives.Add(decor, "WingR", VisualShape.Triangle, dark, new Vector2(0.4f, 0.25f), new Vector2(0.32f, 0.1f), VisualSorting.Enemies, -90f);
                break;
        }
    }

    static void AddRing(Transform decor, string name, float scale, Color color, float alpha)
    {
        var c = new Color(color.r, color.g, color.b, alpha);
        VisualPrimitives.Add(decor, name, VisualShape.Ring, c, new Vector2(scale, scale), Vector2.zero, VisualSorting.Enemies);
    }

    static void SetDepth(Transform decor)
    {
        for (int i = 0; i < decor.childCount; i++)
        {
            var child = decor.GetChild(i);
            var p = child.localPosition;
            var z = child.name.Contains("Ring") ? RingZ : AccentZ;
            child.localPosition = new Vector3(p.x, p.y, z);
        }
    }
}
