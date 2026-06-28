using System.Collections.Generic;
using UnityEngine;

public enum ProjectileKind
{
    None,
    Arrow,
    Frost,
    Cannon,
    Arcane
}

/// <summary>
/// Visual-only projectile (P-A2). Flies from the tower to the target position
/// and releases back to the pool on arrival. Carries no damage (the gameplay
/// damage is applied instantly by the tower); this is purely a tracer for game
/// feel. Uses scaled time so it respects pause and battle-speed.
/// </summary>
public class ProjectileVisual : MonoBehaviour
{
    SpriteRenderer body;
    TrailRenderer trail;
    ProjectileKind kind;
    Vector3 from;
    Vector3 to;
    float age;
    float duration;
    bool active;

    public bool IsAvailable => !active;

    public void Launch(ProjectileKind projectileKind, Vector3 origin, Vector3 target)
    {
        EnsureRenderer();
        kind = projectileKind;
        from = origin;
        to = target;
        age = 0f;

        var distance = Vector3.Distance(origin, target);
        var speed = kind == ProjectileKind.Cannon ? 9f : 14f;
        duration = Mathf.Clamp(distance / speed, 0.05f, 0.6f);

        ConfigureAppearance();
        transform.position = origin;
        active = true;
        gameObject.SetActive(true);

        if (trail != null)
        {
            trail.Clear();
            trail.emitting = kind == ProjectileKind.Frost;
        }
    }

    void EnsureRenderer()
    {
        if (body != null)
            return;

        body = gameObject.AddComponent<SpriteRenderer>();
        body.sortingOrder = VisualSorting.Projectiles;

        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.12f;
        trail.startWidth = 0.12f;
        trail.endWidth = 0f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.emitting = false;
        trail.sortingOrder = VisualSorting.Projectiles - 1;
    }

    void ConfigureAppearance()
    {
        switch (kind)
        {
            case ProjectileKind.Arrow:
                body.sprite = ProceduralSpriteFactory.GetWhiteSprite();
                body.color = new Color(0.95f, 0.9f, 0.7f);
                transform.localScale = new Vector3(0.28f, 0.08f, 1f);
                break;
            case ProjectileKind.Frost:
                body.sprite = ProceduralSpriteFactory.GetCircleSprite();
                body.color = new Color(0.7f, 0.9f, 1f);
                transform.localScale = Vector3.one * 0.18f;
                if (trail != null)
                {
                    trail.startColor = new Color(0.7f, 0.9f, 1f, 0.7f);
                    trail.endColor = new Color(0.7f, 0.9f, 1f, 0f);
                }
                break;
            case ProjectileKind.Cannon:
                body.sprite = ProceduralSpriteFactory.GetCircleSprite();
                body.color = new Color(0.2f, 0.2f, 0.2f);
                transform.localScale = Vector3.one * 0.26f;
                break;
            case ProjectileKind.Arcane:
                body.sprite = ProceduralSpriteFactory.GetDiamondSprite();
                body.color = new Color(0.72f, 0.45f, 0.95f);
                transform.localScale = Vector3.one * 0.24f;
                break;
        }
    }

    void Update()
    {
        if (!active)
            return;

        age += Time.deltaTime;
        var t = Mathf.Clamp01(age / duration);

        var pos = Vector3.Lerp(from, to, t);
        if (kind == ProjectileKind.Cannon)
            pos.y += Mathf.Sin(t * Mathf.PI) * 0.6f;

        transform.position = pos;

        if (kind == ProjectileKind.Arrow)
        {
            var dir = (to - from);
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }
        else if (kind == ProjectileKind.Arcane)
        {
            transform.Rotate(0f, 0f, 540f * Time.deltaTime);
        }

        if (t >= 1f)
            Arrive();
    }

    void Arrive()
    {
        if (kind == ProjectileKind.Cannon)
            ProjectileVisualFactory.SpawnImpact(transform.position);

        Release();
    }

    public void Release()
    {
        active = false;
        if (trail != null)
            trail.emitting = false;
        gameObject.SetActive(false);
    }
}

/// <summary>Pooled factory for <see cref="ProjectileVisual"/> tracers (P-A2).</summary>
public static class ProjectileVisualFactory
{
    const int PoolSize = 32;
    static readonly List<ProjectileVisual> pool = new();
    static Transform root;

    public static ProjectileKind KindForTower(TowerType type) => type switch
    {
        TowerType.Arrow => ProjectileKind.Arrow,
        TowerType.Frost => ProjectileKind.Frost,
        TowerType.Cannon => ProjectileKind.Cannon,
        TowerType.Arcane => ProjectileKind.Arcane,
        _ => ProjectileKind.None
    };

    public static void Fire(TowerType type, Vector3 from, Vector3 to)
    {
        var kind = KindForTower(type);
        if (kind == ProjectileKind.None)
            return;

        var projectile = Rent();
        projectile?.Launch(kind, from, to);
    }

    public static void SpawnImpact(Vector3 position)
    {
        var go = new GameObject("CannonImpact");
        go.transform.position = position;
        go.AddComponent<ProjectileImpactFx>();
    }

    static ProjectileVisual Rent()
    {
        EnsureRoot();

        foreach (var projectile in pool)
        {
            if (projectile != null && projectile.IsAvailable)
                return projectile;
        }

        if (pool.Count >= PoolSize)
            return null;

        var go = new GameObject("Projectile");
        go.transform.SetParent(root, false);
        go.SetActive(false);
        var created = go.AddComponent<ProjectileVisual>();
        pool.Add(created);
        return created;
    }

    static void EnsureRoot()
    {
        if (root != null)
            return;

        pool.Clear();
        root = new GameObject("ProjectilePool").transform;
    }
}

/// <summary>Short orange expanding burst for cannon impacts (P-A2).</summary>
public class ProjectileImpactFx : MonoBehaviour
{
    SpriteRenderer renderer2d;
    float age;
    const float Lifetime = 0.3f;

    void Start()
    {
        renderer2d = gameObject.AddComponent<SpriteRenderer>();
        renderer2d.sprite = ProceduralSpriteFactory.GetCircleSprite();
        renderer2d.color = new Color(1f, 0.55f, 0.15f, 0.85f);
        renderer2d.sortingOrder = VisualSorting.Vfx;
        transform.localScale = Vector3.one * 0.3f;
    }

    void Update()
    {
        age += Time.deltaTime;
        var t = Mathf.Clamp01(age / Lifetime);
        transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.1f, t);
        if (renderer2d != null)
            renderer2d.color = new Color(1f, 0.55f, 0.15f, 0.85f * (1f - t));

        if (t >= 1f)
            Destroy(gameObject);
    }
}
