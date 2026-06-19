using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodexMenuUI : MonoBehaviour
{
    enum CodexTab
    {
        Towers,
        Enemies,
        Synergies
    }

    const float ScreenPadding = 24f;

    GameObject overlayRoot;
    GameObject listPanel;
    RectTransform listContentRect;
    TextMeshProUGUI detailTitleText;
    TextMeshProUGUI detailSubtitleText;
    TextMeshProUGUI detailBodyText;
    Button towersTabButton;
    Button enemiesTabButton;
    Button synergiesTabButton;
    Image towersTabImage;
    Image enemiesTabImage;
    Image synergiesTabImage;

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
        rect.anchoredPosition = new Vector2(ScreenPadding, -212f);
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
        panelRect.sizeDelta = new Vector2(960f, 640f);
        panelRect.anchoredPosition = Vector2.zero;
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var panelBackground = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(panelBackground, 0.97f);

        var title = CreateLabel(panel.transform, "Game Codex", 30f, TextAlignmentOptions.TopLeft);
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

        towersTabButton = CreateTabButton(panel.transform, "Towers", 24f, 100f, () => SwitchTab(CodexTab.Towers));
        enemiesTabButton = CreateTabButton(panel.transform, "Enemies", 130f, 100f, () => SwitchTab(CodexTab.Enemies));
        synergiesTabButton = CreateTabButton(panel.transform, "Synergy", 236f, 100f, () => SwitchTab(CodexTab.Synergies));
        towersTabImage = towersTabButton.GetComponent<Image>();
        enemiesTabImage = enemiesTabButton.GetComponent<Image>();
        synergiesTabImage = synergiesTabButton.GetComponent<Image>();

        listPanel = CreateUiObject("ListPanel", panel.transform);
        var listRect = listPanel.GetComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0f, 0f);
        listRect.anchorMax = new Vector2(0f, 1f);
        listRect.pivot = new Vector2(0f, 1f);
        listRect.anchoredPosition = new Vector2(20f, -104f);
        listRect.sizeDelta = new Vector2(232f, -124f);

        var listBackground = listPanel.AddComponent<Image>();
        listBackground.color = new Color(0.06f, 0.08f, 0.06f, 0.92f);

        var listScroll = listPanel.AddComponent<ScrollRect>();
        listScroll.horizontal = false;
        listScroll.vertical = true;
        listScroll.movementType = ScrollRect.MovementType.Clamped;

        var listViewport = CreateUiObject("ListViewport", listPanel.transform);
        var listViewportRect = listViewport.GetComponent<RectTransform>();
        Stretch(listViewportRect);
        listViewport.AddComponent<RectMask2D>();
        listScroll.viewport = listViewportRect;

        var listContent = CreateUiObject("ListContent", listViewport.transform);
        listContentRect = listContent.GetComponent<RectTransform>();
        listContentRect.anchorMin = new Vector2(0f, 1f);
        listContentRect.anchorMax = new Vector2(1f, 1f);
        listContentRect.pivot = new Vector2(0.5f, 1f);
        listContentRect.anchoredPosition = Vector2.zero;
        listContentRect.sizeDelta = new Vector2(0f, 0f);
        listScroll.content = listContentRect;

        var listLayout = listContent.AddComponent<VerticalLayoutGroup>();
        listLayout.padding = new RectOffset(8, 8, 8, 8);
        listLayout.spacing = 6f;
        listLayout.childAlignment = TextAnchor.UpperCenter;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = true;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = false;

        var listContentFitter = listContent.AddComponent<ContentSizeFitter>();
        listContentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        listContentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var detailPanel = CreateUiObject("DetailPanel", panel.transform);
        var detailRect = detailPanel.GetComponent<RectTransform>();
        detailRect.anchorMin = new Vector2(0f, 0f);
        detailRect.anchorMax = new Vector2(1f, 1f);
        detailRect.offsetMin = new Vector2(268f, 20f);
        detailRect.offsetMax = new Vector2(-20f, -104f);

        var detailBackground = detailPanel.AddComponent<Image>();
        detailBackground.color = new Color(0.05f, 0.07f, 0.05f, 0.88f);

        detailTitleText = CreateLabel(detailPanel.transform, string.Empty, 26f, TextAlignmentOptions.TopLeft);
        detailTitleText.enableWordWrapping = false;
        LayoutDetailHeader(detailTitleText.rectTransform, -12f, 32f);

        detailSubtitleText = CreateLabel(detailPanel.transform, string.Empty, 15f, TextAlignmentOptions.TopLeft);
        detailSubtitleText.color = new Color(0.95f, 0.9f, 0.65f);
        LayoutDetailHeader(detailSubtitleText.rectTransform, -48f, 24f);

        var scrollObject = CreateUiObject("DetailScroll", detailPanel.transform);
        var scrollRect = scrollObject.GetComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = new Vector2(16f, 16f);
        scrollRect.offsetMax = new Vector2(-16f, -80f);

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
        contentRect.sizeDelta = new Vector2(0f, 0f);
        scroll.content = contentRect;

        var contentFitter = content.AddComponent<ContentSizeFitter>();
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        detailBodyText = CreateLabel(content.transform, string.Empty, 16f, TextAlignmentOptions.TopLeft);
        detailBodyText.color = new Color(0.88f, 0.92f, 0.88f);
        detailBodyText.lineSpacing = -2f;
        var bodyRect = detailBodyText.rectTransform;
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0f, 1f);
        bodyRect.anchoredPosition = Vector2.zero;
        bodyRect.sizeDelta = new Vector2(-8f, 0f);

        var bodyFitter = detailBodyText.gameObject.AddComponent<ContentSizeFitter>();
        bodyFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        bodyFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    Button CreateTabButton(Transform parent, string label, float x, float width, UnityEngine.Events.UnityAction onClick)
    {
        var buttonObject = CreateUiObject($"{label}Tab", parent);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(width, 34f);
        rect.anchoredPosition = new Vector2(x, -58f);
        UiDisplaySettings.SnapRectToPixels(rect);

        var image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

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
        SetTabActive(towersTabImage, activeTab == CodexTab.Towers);
        SetTabActive(enemiesTabImage, activeTab == CodexTab.Enemies);
        SetTabActive(synergiesTabImage, activeTab == CodexTab.Synergies);
    }

    static void SetTabActive(Image image, bool active)
    {
        image.color = active
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

        for (int i = 0; i < entries.Count; i++)
        {
            var index = i;
            var entry = entries[i];
            var itemObject = CreateUiObject($"ListItem_{entry.Id}", listContentRect);
            var rect = itemObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 38f);

            var layoutElement = itemObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = 38f;
            layoutElement.preferredHeight = 38f;

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
        return activeTab switch
        {
            CodexTab.Towers => GameCodexCatalog.GetTowerEntries(),
            CodexTab.Enemies => GameCodexCatalog.GetEnemyEntries(),
            CodexTab.Synergies => GameCodexCatalog.GetSynergyEntries(),
            _ => GameCodexCatalog.GetTowerEntries()
        };
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
