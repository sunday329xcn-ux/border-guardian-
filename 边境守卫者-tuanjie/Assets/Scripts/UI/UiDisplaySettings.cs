using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class UiDisplaySettings
{
    public const float ScreenPadding = 24f;
    /// <summary>Extra inset for top-left HUD cluster so labels are not clipped at screen edge.</summary>
    public const float HudLeftInset = 48f;
    public const float HudTopPadding = 28f;
    public const float HudLineHeight = 42f;
    public const float PanelAlpha = 0.92f;
    public const float FontSizeTitle = 24f;
    public const float FontSizeBody = 16f;
    public const float FontSizeCaption = 14f;

    static float uiScale = 1f;

    public static readonly Color DimOverlay = new(0.04f, 0.06f, 0.04f, 0.78f);
    public static readonly Color BodyText = new(0.85f, 0.9f, 0.85f);
    public static readonly Color AccentText = new(0.95f, 0.9f, 0.65f);
    public static readonly Color MutedText = new(0.75f, 0.82f, 0.75f);
    public static readonly Color ButtonDefault = new(0.2f, 0.2f, 0.2f, 0.88f);
    public static readonly Color ButtonSelected = new(0.28f, 0.48f, 0.28f, 0.95f);
    public static readonly Color ButtonDanger = new(0.35f, 0.18f, 0.18f, 0.92f);
    public static readonly Color StarGold = new(1f, 0.88f, 0.35f);

    public static float UiScale => uiScale;

    public static void SetUiScale(float scale)
    {
        uiScale = Mathf.Clamp(scale, 0.75f, 1.25f);
    }

    public static void ApplyUiScale(Canvas canvas)
    {
        if (canvas == null)
            return;

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            return;

        scaler.referenceResolution = new Vector2(1920f / uiScale, 1080f / uiScale);
    }

    public static void ConfigureCanvas(Canvas canvas)
    {
        if (canvas == null)
            return;

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f / uiScale, 1080f / uiScale);
        scaler.matchWidthOrHeight = 0.5f;
    }

    public static void ApplySharpText(TextMeshProUGUI text, float fontSize)
    {
        if (text == null)
            return;

        text.fontSize = fontSize;
        text.enableAutoSizing = false;
        text.richText = false;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        SnapRectToPixels(text.rectTransform);
    }

    public static void ApplyButtonText(TextMeshProUGUI text, float fontSize)
    {
        ApplySharpText(text, fontSize);
        if (text == null)
            return;

        text.lineSpacing = -4f;
        text.margin = new Vector4(4f, 2f, 4f, 2f);
    }

    public static void ApplyHudBodyText(TextMeshProUGUI text, float fontSize = FontSizeBody)
    {
        ApplySharpText(text, fontSize);
        if (text != null)
            text.color = BodyText;
    }

    public static void ApplyPanelBackground(Image image, float alpha = PanelAlpha)
    {
        if (image == null)
            return;

        image.color = new Color(0.08f, 0.1f, 0.08f, alpha);
        image.raycastTarget = true;
    }

    public static void ApplyDimOverlay(Image image)
    {
        if (image == null)
            return;

        image.color = DimOverlay;
        image.raycastTarget = true;
    }

    public static void ApplyBuildButton(Image image, bool selected)
    {
        if (image == null)
            return;

        image.color = selected ? ButtonSelected : ButtonDefault;
    }

    public static void ApplyAccentButton(Image image)
    {
        if (image == null)
            return;

        image.color = ButtonSelected;
    }

    public static void ApplyDangerButton(Image image)
    {
        if (image == null)
            return;

        image.color = ButtonDanger;
    }

    public static void ApplyWhiteSprite(Image image)
    {
        if (image == null)
            return;

        image.sprite = MapGridControllerShared.GetWhiteSprite();
        image.type = Image.Type.Simple;
    }

    public static void SnapRectToPixels(RectTransform rect)
    {
        if (rect == null)
            return;

        var pos = rect.anchoredPosition;
        rect.anchoredPosition = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }
}
