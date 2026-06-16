using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodexMenuUI : MonoBehaviour
{
    enum CodexTab
    {
        Towers,
        Enemies
    }

    const float ScreenPadding = 24f;

    GameObject overlayRoot;
    GameObject listPanel;
    TextMeshProUGUI detailTitleText;
    TextMeshProUGUI detailSubtitleText;
    TextMeshProUGUI detailBodyText;
    Button towersTabButton;
    Button enemiesTabButton;
    Image towersTabImage;
    Image enemiesTabImage;

    CodexTab activeTab = CodexTab.Towers;
    int selectedIndex;
    bool causedPause;
    readonly List<Button> listButtons = new();

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());
        CreateCodexButton();
        CreateOverlay();
        Hide();
    }

    void CreateCodexButton()
    {
        var buttonObject = CreateUiObject("CodexButton", transform);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(96f, 36f);
        rect.anchoredPosition = new Vector2(ScreenPadding, -188f);
        UiDisplaySettings.SnapRectToPixels(rect);

        var image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.24f, 0.2f, 0.92f);

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(Open);

        var label = CreateLabel(buttonObject.transform, "Codex", 17f, TextAlignmentOptions.Center);
        Stretch(label.rectTransform);
    }

    void CreateOverlay()
    {
        overlayRoot = CreateUiObject("CodexOverlay", transform);
        var overlayRect = overlayRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        var dim = overlayRoot.AddComponent<Image>();
        dim.color = new Color(0.03f, 0.05f, 0.03f, 0.82f);
        dim.raycastTarget = true;

        var panel = CreateUiObject("CodexPanel", overlayRoot.transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(920f, 620f);
        panelRect.anchoredPosition = Vector2.zero;
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var panelBackground = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(panelBackground, 0.97f);

        var title = CreateLabel(panel.transform, "Tower & Enemy Codex", 30f, TextAlignmentOptions.TopLeft);
        LayoutTop(title.rectTransform, -16f, 36f, 24f, -24f);

        var closeObject = CreateUiObject("CloseButton", panel.transform);
        var closeRect = closeObject.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(96f, 36f);
        closeRect.anchoredPosition = new Vector2(-20f, -16f);
        UiDisplaySettings.SnapRectToPixels(closeRect);

        var closeImage = closeObject.AddComponent<Image>();
        closeImage.color = new Color(0.28f, 0.28f, 0.28f, 0.95f);

        var closeButton = closeObject.AddComponent<Button>();
        closeButton.targetGraphic = closeImage;
        closeButton.onClick.AddListener(Close);

        var closeText = CreateLabel(closeObject.transform, "Close", 17f, TextAlignmentOptions.Center);
        Stretch(closeText.rectTransform);

        towersTabButton = CreateTabButton(panel.transform, "Towers", -58f, true, () => SwitchTab(CodexTab.Towers));
        enemiesTabButton = CreateTabButton(panel.transform, "Enemies", 86f, false, () => SwitchTab(CodexTab.Enemies));
        towersTabImage = towersTabButton.GetComponent<Image>();
        enemiesTabImage = enemiesTabButton.GetComponent<Image>();

        listPanel = CreateUiObject("ListPanel", panel.transform);
        var listRect = listPanel.GetComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0f, 0f);
        listRect.anchorMax = new Vector2(0f, 1f);
        listRect.pivot = new Vector2(0f, 1f);
        listRect.anchoredPosition = new Vector2(20f, -96f);
        listRect.sizeDelta = new Vector2(220f, -116f);

        var listBackground = listPanel.AddComponent<Image>();
        listBackground.color = new Color(0.06f, 0.08f, 0.06f, 0.92f);

        var detailPanel = CreateUiObject("DetailPanel", panel.transform);
        var detailRect = detailPanel.GetComponent<RectTransform>();
        detailRect.anchorMin = new Vector2(0f, 0f);
        detailRect.anchorMax = new Vector2(1f, 1f);
        detailRect.offsetMin = new Vector2(256f, 20f);
        detailRect.offsetMax = new Vector2(-20f, -96f);

        var detailBackground = detailPanel.AddComponent<Image>();
        detailBackground.color = new Color(0.05f, 0.07f, 0.05f, 0.88f);

        detailTitleText = CreateLabel(detailPanel.transform, string.Empty, 24f, TextAlignmentOptions.TopLeft);
        LayoutDetailHeader(detailTitleText.rectTransform, -12f, 30f);

        detailSubtitleText = CreateLabel(detailPanel.transform, string.Empty, 16f, TextAlignmentOptions.TopLeft);
        detailSubtitleText.color = new Color(0.95f, 0.9f, 0.65f);
        LayoutDetailHeader(detailSubtitleText.rectTransform, -44f, 22f);

        var scrollObject = CreateUiObject("DetailScroll", detailPanel.transform);
        var scrollRect = scrollObject.GetComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = new Vector2(16f, 16f);
        scrollRect.offsetMax = new Vector2(-16f, -72f);

        var scroll = scrollObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        var viewport = CreateUiObject("Viewport", scrollObject.transform);
        var viewportRect = viewport.GetComponent<RectTransform>();
        Stretch(viewportRect);
        viewport.AddComponent<RectMask2D>();
        scroll.viewport = viewportRect;

        var content = CreateUiObject("Content", viewport.transform);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 800f);
        scroll.content = contentRect;

        detailBodyText = CreateLabel(content.transform, string.Empty, 17f, TextAlignmentOptions.TopLeft);
        detailBodyText.color = new Color(0.88f, 0.92f, 0.88f);
        var bodyRect = detailBodyText.rectTransform;
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0f, 1f);
        bodyRect.anchoredPosition = Vector2.zero;
        bodyRect.sizeDelta = new Vector2(-8f, 800f);
    }

    Button CreateTabButton(Transform parent, string label, float x, bool isFirst, UnityEngine.Events.UnityAction onClick)
    {
        var buttonObject = CreateUiObject($"{label}Tab", parent);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(120f, 34f);
        rect.anchoredPosition = new Vector2(24f + x, -58f);
        UiDisplaySettings.SnapRectToPixels(rect);

        var image = buttonObject.AddComponent<Image>();
        image.color = isFirst
            ? new Color(0.28f, 0.48f, 0.28f, 0.95f)
            : new Color(0.2f, 0.2f, 0.2f, 0.9f);

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var text = CreateLabel(buttonObject.transform, label, 17f, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);
        return button;
    }

    void SwitchTab(CodexTab tab)
    {
        activeTab = tab;
        selectedIndex = 0;
        RefreshTabVisuals();
        RebuildList();
        ShowSelectedEntry();
    }

    void RefreshTabVisuals()
    {
        var towersActive = activeTab == CodexTab.Towers;
        towersTabImage.color = towersActive
            ? new Color(0.28f, 0.48f, 0.28f, 0.95f)
            : new Color(0.2f, 0.2f, 0.2f, 0.9f);
        enemiesTabImage.color = !towersActive
            ? new Color(0.28f, 0.48f, 0.28f, 0.95f)
            : new Color(0.2f, 0.2f, 0.2f, 0.9f);
    }

    void RebuildList()
    {
        foreach (var button in listButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }

        listButtons.Clear();

        var entries = GetActiveEntries();
        const float itemHeight = 38f;
        const float topPadding = 8f;

        for (int i = 0; i < entries.Count; i++)
        {
            var index = i;
            var entry = entries[i];
            var itemObject = CreateUiObject($"ListItem_{entry.Id}", listPanel.transform);
            var rect = itemObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -(topPadding + i * (itemHeight + 6f)));
            rect.sizeDelta = new Vector2(-12f, itemHeight);

            var image = itemObject.AddComponent<Image>();
            image.color = i == selectedIndex
                ? new Color(0.28f, 0.48f, 0.28f, 0.95f)
                : new Color(0.16f, 0.18f, 0.16f, 0.95f);

            var button = itemObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => SelectEntry(index));
            listButtons.Add(button);

            var label = CreateLabel(itemObject.transform, entry.Title, 16f, TextAlignmentOptions.MidlineLeft);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10f, 0f);
            labelRect.offsetMax = new Vector2(-8f, 0f);
        }
    }

    IReadOnlyList<CodexEntry> GetActiveEntries()
    {
        return activeTab == CodexTab.Towers
            ? GameCodexCatalog.GetTowerEntries()
            : GameCodexCatalog.GetEnemyEntries();
    }

    void SelectEntry(int index)
    {
        selectedIndex = index;
        RebuildList();
        ShowSelectedEntry();
    }

    void ShowSelectedEntry()
    {
        var entries = GetActiveEntries();
        if (entries.Count == 0 || selectedIndex < 0 || selectedIndex >= entries.Count)
            return;

        var entry = entries[selectedIndex];
        detailTitleText.text = entry.Title;
        detailSubtitleText.text = entry.Subtitle;
        detailBodyText.text = entry.Body;
    }

    public void Open()
    {
        causedPause = false;

        if (GamePauseController.Instance != null &&
            GamePauseController.Instance.CanPause() &&
            !GamePauseController.Instance.IsPaused)
        {
            GamePauseController.Instance.Pause();
            causedPause = true;
        }

        activeTab = CodexTab.Towers;
        selectedIndex = 0;
        RefreshTabVisuals();
        RebuildList();
        ShowSelectedEntry();
        overlayRoot.SetActive(true);
    }

    public void Close()
    {
        Hide();

        if (causedPause && GamePauseController.Instance != null)
            GamePauseController.Instance.Resume();

        causedPause = false;
    }

    void Hide()
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(false);
    }

    static void LayoutTop(RectTransform rect, float y, float height, float left, float right)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(left, y);
        rect.sizeDelta = new Vector2(left + right, height);
    }

    static void LayoutDetailHeader(RectTransform rect, float y, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(16f, y);
        rect.sizeDelta = new Vector2(-32f, height);
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
        label.enableWordWrapping = true;
        UiDisplaySettings.ApplySharpText(label, fontSize);
        return label;
    }
}
