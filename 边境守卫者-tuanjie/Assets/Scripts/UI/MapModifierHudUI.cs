using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Top-centre toggle for the active map modifier (P4.2). Top-centre is free
/// during gameplay (controls are top-right, missile top-left, hero bar
/// bottom-centre), so this respects the no-overlap UI rule.
/// </summary>
public class MapModifierHudUI : MonoBehaviour
{
    Button button;
    TextMeshProUGUI label;

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());
        CreatePanel();
        Refresh();
    }

    void CreatePanel()
    {
        var panel = new GameObject("MapModifierHud", typeof(RectTransform));
        panel.transform.SetParent(transform, false);

        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(190f, 34f);
        rect.anchoredPosition = new Vector2(0f, -UiDisplaySettings.ScreenPadding);
        UiDisplaySettings.SnapRectToPixels(rect);

        var image = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyBuildButton(image, selected: false);

        button = panel.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(OnCycle);

        label = CreateLabel(panel.transform, "Weather: Clear", 14f, TextAlignmentOptions.Center);
        var labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 2f);
        labelRect.offsetMax = new Vector2(-8f, -2f);
    }

    void OnCycle()
    {
        MapModifierService.Cycle();
        Refresh();
    }

    void Refresh()
    {
        if (label != null)
            label.text = $"Weather: {MapModifierService.GetName(MapModifierService.Active)}";
    }

    static TextMeshProUGUI CreateLabel(Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var l = go.AddComponent<TextMeshProUGUI>();
        l.text = text;
        l.alignment = alignment;
        l.color = Color.white;
        l.raycastTarget = false;
        UiDisplaySettings.ApplySharpText(l, fontSize);
        return l;
    }
}
