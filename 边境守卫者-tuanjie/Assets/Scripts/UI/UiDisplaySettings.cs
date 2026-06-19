using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class UiDisplaySettings
{
    public static void ConfigureCanvas(Canvas canvas)
    {
        if (canvas == null)
            return;

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
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

    public static void ApplyPanelBackground(Image image, float alpha = 0.9f)
    {
        if (image == null)
            return;

        image.color = new Color(0.08f, 0.1f, 0.08f, alpha);
        image.raycastTarget = true;
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
