using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoblinMissileUI : MonoBehaviour
{
    const float PanelWidth = 220f;
    const float PanelHeight = 92f;

    GoblinMissileController missileController;
    Button launchButton;
    TextMeshProUGUI buttonLabel;
    TextMeshProUGUI hintLabel;

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());
        missileController = FindObjectOfType<GoblinMissileController>();
        CreatePanel();
    }

    void Update()
    {
        if (missileController == null)
            missileController = FindObjectOfType<GoblinMissileController>();

        Refresh();
    }

    void CreatePanel()
    {
        var panel = new GameObject("GoblinMissilePanel", typeof(RectTransform));
        panel.transform.SetParent(transform, false);

        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0.5f);
        panelRect.anchorMax = new Vector2(0f, 0.5f);
        panelRect.pivot = new Vector2(0f, 0.5f);
        panelRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
        panelRect.anchoredPosition = new Vector2(UiDisplaySettings.GoblinMissileLeftInset, 0f);
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var background = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(background, UiDisplaySettings.PanelAlpha);

        launchButton = CreateButton(panel.transform, "Goblin Missile", new Vector2(PanelWidth - 24f, 44f), 17f, OnLaunchClicked);
        var buttonRect = launchButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 1f);
        buttonRect.anchorMax = new Vector2(0.5f, 1f);
        buttonRect.pivot = new Vector2(0.5f, 1f);
        buttonRect.anchoredPosition = new Vector2(0f, -12f);
        buttonLabel = launchButton.GetComponentInChildren<TextMeshProUGUI>();

        hintLabel = CreateLabel(panel.transform, "3×3 true dmg + stun\n50% dmg to towers", 13f, TextAlignmentOptions.Center);
        hintLabel.color = UiDisplaySettings.MutedText;
        var hintRect = hintLabel.rectTransform;
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.anchoredPosition = new Vector2(0f, 8f);
        hintRect.sizeDelta = new Vector2(-16f, 34f);
    }

    void OnLaunchClicked()
    {
        if (missileController == null)
            return;

        if (missileController.IsTargeting)
        {
            missileController.CancelTargeting();
            return;
        }

        missileController.BeginTargeting();
    }

    void Refresh()
    {
        if (launchButton == null || buttonLabel == null)
            return;

        if (missileController == null)
        {
            launchButton.interactable = false;
            buttonLabel.text = "Goblin Missile\n(unavailable)";
            return;
        }

        if (missileController.IsTargeting)
        {
            launchButton.interactable = true;
            buttonLabel.text = "Cancel\n(select map tile)";
            return;
        }

        if (missileController.IsReady)
        {
            launchButton.interactable = true;
            buttonLabel.text = "Goblin Missile\nReady";
            return;
        }

        launchButton.interactable = false;
        var cooldown = missileController.CooldownRemaining;
        buttonLabel.text = cooldown > 0.5f
            ? $"Goblin Missile\nCD {Mathf.CeilToInt(cooldown)}s"
            : "Goblin Missile\nCD ...";
    }

    static TextMeshProUGUI CreateLabel(Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.alignment = alignment;
        label.color = Color.white;
        label.raycastTarget = false;
        UiDisplaySettings.ApplySharpText(label, fontSize);
        return label;
    }

    static Button CreateButton(Transform parent, string label, Vector2 size, float fontSize, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Button", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        UiDisplaySettings.SnapRectToPixels(rect);

        var image = go.AddComponent<Image>();
        UiDisplaySettings.ApplyBuildButton(image, selected: false);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var text = CreateLabel(go.transform, label, fontSize, TextAlignmentOptions.Center);
        UiDisplaySettings.ApplyButtonText(text, fontSize);
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(6f, 4f);
        textRect.offsetMax = new Vector2(-6f, -4f);

        return button;
    }
}
