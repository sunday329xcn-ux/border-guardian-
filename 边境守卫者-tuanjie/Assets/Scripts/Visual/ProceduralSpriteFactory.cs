using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedural sprite source for the zero-art visual pass (P-A0). Wraps the
/// existing 1x1 white sprite and adds cached circle / soft-shadow / value-noise
/// textures generated entirely at runtime (no external art). All results are
/// cached so repeated calls are cheap.
/// </summary>
public static class ProceduralSpriteFactory
{
    static Sprite circleSprite;
    static Sprite softShadowSprite;
    static readonly Dictionary<int, Sprite> noiseCache = new();

    /// <summary>Shared 1x1 opaque white sprite (delegates to the legacy source).</summary>
    public static Sprite GetWhiteSprite() => MapGridControllerShared.GetWhiteSprite();

    /// <summary>Anti-aliased filled circle, pivot centre, 1 world-unit diameter.</summary>
    public static Sprite GetCircleSprite(int resolution = 64)
    {
        if (circleSprite != null)
            return circleSprite;

        var tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        var center = (resolution - 1) * 0.5f;
        var radius = resolution * 0.5f;

        for (int y = 0; y < resolution; y++)
        for (int x = 0; x < resolution; x++)
        {
            var dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
            var alpha = Mathf.Clamp01((radius - dist) / 1.5f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }

        tex.Apply();
        circleSprite = Sprite.Create(tex, new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f), resolution);
        return circleSprite;
    }

    /// <summary>Soft radial drop used for unit ground shadows (flattened, dark).</summary>
    public static Sprite GetSoftShadowSprite(int resolution = 48)
    {
        if (softShadowSprite != null)
            return softShadowSprite;

        var tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        var center = (resolution - 1) * 0.5f;
        var radius = resolution * 0.5f;

        for (int y = 0; y < resolution; y++)
        for (int x = 0; x < resolution; x++)
        {
            var dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / radius;
            var alpha = Mathf.Clamp01(1f - dist);
            alpha *= alpha; // softer falloff
            tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
        }

        tex.Apply();
        softShadowSprite = Sprite.Create(tex, new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f), resolution);
        return softShadowSprite;
    }

    /// <summary>
    /// Value-noise tile blended between two colours. Cached by a key derived
    /// from the arguments so identical tiles are generated only once.
    /// </summary>
    public static Sprite GetNoiseSprite(Color baseColor, Color highlightColor, int size = 64,
        float frequency = 6f, int seed = 0)
    {
        var key = HashKey(baseColor, highlightColor, size, frequency, seed);
        if (noiseCache.TryGetValue(key, out var cached) && cached != null)
            return cached;

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear
        };

        var offset = seed * 13.37f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            var nx = (float)x / size * frequency + offset;
            var ny = (float)y / size * frequency + offset;
            var n = Mathf.PerlinNoise(nx, ny);
            tex.SetPixel(x, y, Color.Lerp(baseColor, highlightColor, n));
        }

        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        noiseCache[key] = sprite;
        return sprite;
    }

    static Sprite triangleSprite;
    static Sprite diamondSprite;
    static readonly Dictionary<int, Sprite> ringCache = new();
    static readonly Dictionary<int, Sprite> roundedCache = new();

    /// <summary>Upward-pointing filled triangle, pivot centre, 1 world-unit.</summary>
    public static Sprite GetTriangleSprite(int resolution = 64)
    {
        if (triangleSprite != null)
            return triangleSprite;

        var tex = NewTex(resolution);
        for (int y = 0; y < resolution; y++)
        for (int x = 0; x < resolution; x++)
        {
            var u = (float)x / (resolution - 1);
            var v = (float)y / (resolution - 1);
            // Triangle with apex at top-centre, base along the bottom.
            var halfWidth = v * 0.5f;
            var inside = u >= 0.5f - halfWidth && u <= 0.5f + halfWidth;
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, inside ? 1f : 0f));
        }

        tex.Apply();
        triangleSprite = ToSprite(tex, resolution);
        return triangleSprite;
    }

    /// <summary>Filled diamond (rotated square), pivot centre.</summary>
    public static Sprite GetDiamondSprite(int resolution = 64)
    {
        if (diamondSprite != null)
            return diamondSprite;

        var tex = NewTex(resolution);
        var c = (resolution - 1) * 0.5f;
        for (int y = 0; y < resolution; y++)
        for (int x = 0; x < resolution; x++)
        {
            var manhattan = (Mathf.Abs(x - c) + Mathf.Abs(y - c)) / c;
            var alpha = Mathf.Clamp01((1f - manhattan) * resolution * 0.25f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }

        tex.Apply();
        diamondSprite = ToSprite(tex, resolution);
        return diamondSprite;
    }

    /// <summary>Hollow ring used for elite / boss / range emphasis. thickness in 0..1 of radius.</summary>
    public static Sprite GetRingSprite(float thickness = 0.16f, int resolution = 64)
    {
        var key = Mathf.RoundToInt(thickness * 100f) * 1000 + resolution;
        if (ringCache.TryGetValue(key, out var cached) && cached != null)
            return cached;

        var tex = NewTex(resolution);
        var center = (resolution - 1) * 0.5f;
        var radius = resolution * 0.5f;
        var inner = radius * (1f - Mathf.Clamp01(thickness));

        for (int y = 0; y < resolution; y++)
        for (int x = 0; x < resolution; x++)
        {
            var dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
            var outerA = Mathf.Clamp01((radius - dist) / 1.5f);
            var innerA = Mathf.Clamp01((dist - inner) / 1.5f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Min(outerA, innerA)));
        }

        tex.Apply();
        var sprite = ToSprite(tex, resolution);
        ringCache[key] = sprite;
        return sprite;
    }

    /// <summary>
    /// Rounded-rectangle alpha sprite for procedural 9-slice panels (P-A8).
    /// Border is set so the corners stay crisp when stretched.
    /// </summary>
    public static Sprite GetRoundedRectSprite(int size = 48, int cornerRadius = 12, int border = 14)
    {
        var key = size * 10000 + cornerRadius * 100 + border;
        if (roundedCache.TryGetValue(key, out var cached) && cached != null)
            return cached;

        var tex = NewTex(size);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            var alpha = RoundedRectAlpha(x, y, size, cornerRadius);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }

        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
            size, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        roundedCache[key] = sprite;
        return sprite;
    }

    static float RoundedRectAlpha(int x, int y, int size, int cornerRadius)
    {
        var r = Mathf.Max(1, cornerRadius);
        var minX = r;
        var maxX = size - 1 - r;
        var minY = r;
        var maxY = size - 1 - r;

        var cx = Mathf.Clamp(x, minX, maxX);
        var cy = Mathf.Clamp(y, minY, maxY);
        var dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
        return Mathf.Clamp01((r - dist) / 1.5f);
    }

    static Sprite vignetteSprite;

    /// <summary>Radial vignette: transparent centre fading to dark edges (P-A7).</summary>
    public static Sprite GetVignetteSprite(int resolution = 256)
    {
        if (vignetteSprite != null)
            return vignetteSprite;

        var tex = NewTex(resolution);
        var center = (resolution - 1) * 0.5f;
        var maxDist = center * 1.42f;

        for (int y = 0; y < resolution; y++)
        for (int x = 0; x < resolution; x++)
        {
            var dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / maxDist;
            var alpha = Mathf.SmoothStep(0f, 0.55f, Mathf.Clamp01((dist - 0.55f) / 0.45f));
            tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
        }

        tex.Apply();
        vignetteSprite = ToSprite(tex, resolution);
        return vignetteSprite;
    }

    static Texture2D NewTex(int resolution) => new(resolution, resolution, TextureFormat.RGBA32, false)
    {
        wrapMode = TextureWrapMode.Clamp,
        filterMode = FilterMode.Bilinear
    };

    static Sprite ToSprite(Texture2D tex, int resolution) =>
        Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);

    static int HashKey(Color a, Color b, int size, float frequency, int seed)
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + a.GetHashCode();
            h = h * 31 + b.GetHashCode();
            h = h * 31 + size;
            h = h * 31 + Mathf.RoundToInt(frequency * 100f);
            h = h * 31 + seed;
            return h;
        }
    }
}
