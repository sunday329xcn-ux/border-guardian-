using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Front-end talent tree page (P4.4). Lets the player spend keys earned from
/// star ratings on permanent run-wide passives. Mirrors the Settings / Codex
/// page pattern: a self-contained overlay shown over the main menu with a Back
/// button. Placed centre-screen so it never overlaps the gameplay HUD or map.
/// </summary>
public class TalentMenuUI : MonoBehaviour
{
    GameObject overlayRoot;
    TextMeshProUGUI keysText;
    Action onBack;

    readonly Dictionary<TalentId, Button> buyButtons = new();
    readonly Dictionary<TalentId, TextMeshProUGUI> buyLabels = new();

    public void Show(Action backCallback)
    {
        onBack = backCallback;
        EnsureBuilt();
        TalentService.EnsureLoaded();
        overlayRoot.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(false);
    }

    void EnsureBuilt()
    {
        if (overlayRoot != null)
            return;

        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());

        overlayRoot = CreateUiObject("TalentOverlay", transform);
        Stretch(overlayRoot.GetComponent<RectTransform>());

        var backdrop = overlayRoot.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.92f);
        backdrop.raycastTarget = true;

        var panel = CreateUiObject("TalentPanel", overlayRoot.transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(520f, 560f);
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var panelBackground = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(panelBackground, 0.96f);

        var title = CreateLabel(panel.transform, "Talents", 36f, TextAlignmentOptions.Center);
        LayoutTop(title.rectTransform, -20f, 44f);

        var subtitle = CreateLabel(panel.transform, "Spend keys on permanent upgrades", 16f, TextAlignmentOptions.Center);
        subtitle.color = new Color(0.78f, 0.85f, 0.75f);
        LayoutTop(subtitle.rectTransform, -62f, 24f);

        keysText = CreateLabel(panel.transform, string.Empty, 18f, TextAlignmentOptions.Center);
        keysText.color = UiDisplaySettings.AccentText;
        LayoutTop(keysText.rectTransform, -90f, 26f);

        var y = -130f;
        foreach (var id in TalentService.All)
        {
            CreateTalentRow(panel.transform, id, y);
            y -= 92f;
        }

        var back = CreateButton(panel.transform, "Back", new Vector2(180f, 44f), () => onBack?.Invoke(), accent: false);
        var backRect = back.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.5f, 0f);
        backRect.anchorMax = new Vector2(0.5f, 0f);
        backRect.pivot = new Vector2(0.5f, 0f);
        backRect.anchoredPosition = new Vector2(0f, 20f);
    }

    void CreateTalentRow(Transform parent, TalentId id, float y)
    {
        var row = CreateUiObject($"Row_{id}", parent);
        var rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 1f);
        rowRect.anchorMax = new Vector2(0.5f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.anchoredPosition = new Vector2(0f, y);
        rowRect.sizeDelta = new Vector2(480f, 82f);

        var rowBg = row.AddComponent<Image>();
        rowBg.color = new Color(0.16f, 0.2f, 0.16f, 0.95f);

        var title = CreateLabel(row.transform, $"{TalentService.GetTitle(id)}  ({TalentService.GetCost(id)} key)",
            18f, TextAlignmentOptions.TopLeft);
        var titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(14f, -10f);
        titleRect.sizeDelta = new Vector2(-150f, 26f);

        var desc = CreateLabel(row.transform, TalentService.GetDescription(id), 14f, TextAlignmentOptions.TopLeft);
        desc.color = UiDisplaySettings.MutedText;
        var descRect = desc.rectTransform;
        descRect.anchorMin = new Vector2(0f, 1f);
        descRect.anchorMax = new Vector2(1f, 1f);
        descRect.pivot = new Vector2(0f, 1f);
        descRect.anchoredPosition = new Vector2(14f, -40f);
        descRect.sizeDelta = new Vector2(-150f, 34f);

        var buy = CreateButton(row.transform, "Buy", new Vector2(110f, 44f), () =>
        {
            TalentService.Purchase(id);
            Refresh();
        });
        var buyRect = buy.GetComponent<RectTransform>();
        buyRect.anchorMin = new Vector2(1f, 0.5f);
        buyRect.anchorMax = new Vector2(1f, 0.5f);
        buyRect.pivot = new Vector2(1f, 0.5f);
        buyRect.anchoredPosition = new Vector2(-12f, 0f);

        buyButtons[id] = buy.GetComponent<Button>();
        buyLabels[id] = buy.GetComponentInChildren<TextMeshProUGUI>();
    }

    void Refresh()
    {
        if (keysText != null)
            keysText.text = $"Keys available: {TalentService.AvailableKeys}  (Total earned: {LevelProgressService.TotalKeys})";

        foreach (var id in TalentService.All)
        {
            if (!buyButtons.TryGetValue(id, out var button) || button == null)
                continue;

            var label = buyLabels[id];
            var image = button.targetGraphic as Image;

            if (TalentService.IsPurchased(id))
            {
                button.interactable = false;
                if (label != null)
                    label.text = "Owned";
                if (image != null)
                    image.color = new Color(0.28f, 0.5f, 0.3f, 0.95f);
            }
            else if (TalentService.CanPurchase(id))
            {
                button.interactable = true;
                if (label != null)
                    label.text = "Buy";
                if (image != null)
                    UiDisplaySettings.ApplyAccentButton(image);
            }
            else
            {
                button.interactable = false;
                if (label != null)
                    label.text = "Need keys";
                if (image != null)
                    image.color = new Color(0.22f, 0.24f, 0.22f, 0.9f);
            }
        }
    }

    static void LayoutTop(RectTransform rect, float y, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(-40f, height);
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static GameObject CreateUiObject(string objectName, Transform parent)
    {
        var go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = parent.gameObject.layer;
        return go;
    }

    static TextMeshProUGUI CreateLabel(Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
    {
        var go = CreateUiObject("Label", parent);
        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.alignment = alignment;
        label.color = Color.white;
        label.raycastTarget = false;
        UiDisplaySettings.ApplySharpText(label, fontSize);
        return label;
    }

    static GameObject CreateButton(Transform parent, string label, Vector2 size, UnityEngine.Events.UnityAction onClick,
        bool accent = true)
    {
        var go = CreateUiObject("Button", parent);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        UiDisplaySettings.SnapRectToPixels(rect);

        var image = go.AddComponent<Image>();
        if (accent)
            UiDisplaySettings.ApplyAccentButton(image);
        else
            UiDisplaySettings.ApplyBuildButton(image, selected: false);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var text = CreateLabel(go.transform, label, UiDisplaySettings.FontSizeBody, TextAlignmentOptions.Center);
        UiDisplaySettings.ApplyButtonText(text, UiDisplaySettings.FontSizeBody);
        Stretch(text.rectTransform);
        text.rectTransform.offsetMin = new Vector2(4f, 4f);
        text.rectTransform.offsetMax = new Vector2(-4f, -4f);
        return go;
    }
}
