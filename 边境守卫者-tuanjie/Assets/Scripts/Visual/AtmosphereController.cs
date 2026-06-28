using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ambient post-processing substitute (P-A7) using a zero-dependency screen-space
/// vignette overlay (no URP/Volume required). Focuses the eye on the map centre
/// and unifies the forest mood. The overlay never blocks input and renders below
/// the gameplay HUD and floating-text canvases.
/// </summary>
public class AtmosphereController : MonoBehaviour
{
    void Start()
    {
        CreateVignette();
    }

    void CreateVignette()
    {
        // Independent overlay canvas so it sits above the world but below the
        // gameplay HUD (which renders at order >= 0) and never dims the HUD.
        var canvasObject = new GameObject("AtmosphereCanvas");

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -5;

        canvasObject.AddComponent<CanvasScaler>();
        UiDisplaySettings.ConfigureCanvas(canvas);

        var vignetteObject = new GameObject("Vignette", typeof(RectTransform));
        vignetteObject.transform.SetParent(canvasObject.transform, false);

        var rect = vignetteObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = vignetteObject.AddComponent<Image>();
        image.sprite = ProceduralSpriteFactory.GetVignetteSprite();
        image.color = new Color(1f, 1f, 1f, 0.7f);
        image.raycastTarget = false;
    }
}
