using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Creates build + wave UI at runtime. Attach to Canvas.
/// </summary>
public class GameUiController : MonoBehaviour
{
    const float ScreenPadding = 24f;
    const float BuildBarHeight = 104f;

    [SerializeField] WaveManager waveManager;
    [SerializeField] TowerBuildSelector buildSelector;

    TextMeshProUGUI waveTitleText;
    TextMeshProUGUI waveDetailText;
    TextMeshProUGUI waveIntelTitleText;
    TextMeshProUGUI waveIntelEnemyText;
    TextMeshProUGUI waveIntelHintText;
    GameObject waveIntelPanel;
    Button callEarlyButton;
    readonly Dictionary<TowerType, Image> towerButtonImages = new();
    GamePauseController boundPauseController;

    static readonly TowerType[] TowerOrder =
    {
        TowerType.Arrow,
        TowerType.Frost,
        TowerType.Cannon,
        TowerType.Arcane,
        TowerType.Barracks,
        TowerType.DiamondMine
    };

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponent<Canvas>());

        if (waveManager == null)
            waveManager = FindObjectOfType<WaveManager>();

        if (buildSelector == null)
            buildSelector = FindObjectOfType<TowerBuildSelector>();

        CreateBuildPanel();
        CreateWavePanel();
        CreateWaveIntelPanel();

        if (GetComponent<PauseMenuUI>() == null)
            gameObject.AddComponent<PauseMenuUI>();

        if (GetComponent<CombatPresentationUI>() == null)
            gameObject.AddComponent<CombatPresentationUI>();

        if (GetComponent<VictoryResultUI>() == null)
            gameObject.AddComponent<VictoryResultUI>();

        if (GetComponent<LevelProgressHudUI>() == null)
            gameObject.AddComponent<LevelProgressHudUI>();

        if (GetComponent<CodexMenuUI>() == null)
            gameObject.AddComponent<CodexMenuUI>();

        if (waveManager != null)
            waveManager.OnWaveStateChanged += RefreshWaveUi;

        TryBindPauseEvents();

        if (buildSelector != null)
        {
            buildSelector.OnSelectionChanged += _ => RefreshTowerButtons();
            RefreshTowerButtons();
        }

        RefreshWaveUi();
    }

    void OnDestroy()
    {
        if (waveManager != null)
            waveManager.OnWaveStateChanged -= RefreshWaveUi;

        if (boundPauseController != null)
            boundPauseController.OnPauseChanged -= HandlePauseChanged;
    }

    void TryBindPauseEvents()
    {
        if (boundPauseController != null)
            return;

        if (GamePauseController.Instance == null)
            return;

        boundPauseController = GamePauseController.Instance;
        boundPauseController.OnPauseChanged += HandlePauseChanged;
    }

    void HandlePauseChanged(bool paused)
    {
        RefreshWaveUi();
    }

    void Update()
    {
        TryBindPauseEvents();

        if (waveManager != null && waveManager.State == WaveState.Preparation)
            RefreshWaveUi();

        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            RefreshWaveUi();
    }

    void CreateBuildPanel()
    {
        var panel = CreateUiObject("BuildPanel", transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(0f, BuildBarHeight);
        panelRect.anchoredPosition = Vector2.zero;
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var background = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(background);

        var layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 14, 14);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        foreach (var towerType in TowerOrder)
        {
            var cost = TowerBuildCatalog.GetBuildCost(towerType);
            var name = TowerBuildCatalog.GetDisplayName(towerType);
            var label = $"{name}\n{cost}g";

            var capturedType = towerType;
            var buttonObject = CreateButton(panel.transform, label, new Vector2(104f, 72f), 16f, () =>
            {
                if (buildSelector != null)
                    buildSelector.Select(capturedType);
            });

            towerButtonImages[towerType] = buttonObject.GetComponent<Image>();
        }
    }

    void CreateWavePanel()
    {
        var panel = CreateUiObject("WavePanel", transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.sizeDelta = new Vector2(280f, 148f);
        panelRect.anchoredPosition = new Vector2(-ScreenPadding, -ScreenPadding);
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var background = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(background, 0.9f);

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 14, 14);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        waveTitleText = CreateLabel(panel.transform, "Wave 1 / 10", 24f, TextAlignmentOptions.TopLeft);
        ConfigureLayoutElement(waveTitleText.rectTransform, 28f);

        waveDetailText = CreateLabel(panel.transform, "Next in 15s", 20f, TextAlignmentOptions.TopLeft);
        waveDetailText.color = new Color(0.85f, 0.9f, 0.85f);
        ConfigureLayoutElement(waveDetailText.rectTransform, 24f);

        callEarlyButton = CreateButton(panel.transform, "Call Early (+gold)", new Vector2(0f, 40f), 18f, () =>
        {
            if (waveManager != null)
                waveManager.CallWaveEarly();
        }).GetComponent<Button>();

        ConfigureLayoutElement(callEarlyButton.GetComponent<RectTransform>(), 40f);
    }

    void CreateWaveIntelPanel()
    {
        waveIntelPanel = CreateUiObject("WaveIntelPanel", transform);
        var panelRect = waveIntelPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.sizeDelta = new Vector2(520f, 118f);
        panelRect.anchoredPosition = new Vector2(0f, -ScreenPadding);
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var background = waveIntelPanel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(background, 0.9f);

        waveIntelTitleText = CreateLabel(waveIntelPanel.transform, "Incoming Wave", 20f, TextAlignmentOptions.TopLeft);
        LayoutIntelLine(waveIntelTitleText.rectTransform, -12f, 24f);

        waveIntelEnemyText = CreateLabel(waveIntelPanel.transform, string.Empty, 18f, TextAlignmentOptions.TopLeft);
        waveIntelEnemyText.color = new Color(0.95f, 0.92f, 0.75f);
        LayoutIntelLine(waveIntelEnemyText.rectTransform, -38f, 28f);

        waveIntelHintText = CreateLabel(waveIntelPanel.transform, string.Empty, 16f, TextAlignmentOptions.TopLeft);
        waveIntelHintText.color = new Color(0.75f, 0.9f, 0.78f);
        LayoutIntelLine(waveIntelHintText.rectTransform, -70f, 36f);
    }

    static void LayoutIntelLine(RectTransform rect, float y, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(16f, y);
        rect.sizeDelta = new Vector2(-32f, height);
    }

    void RefreshWaveUi()
    {
        if (waveManager == null)
            return;

        if (waveTitleText != null)
            waveTitleText.text = waveManager.GetWaveCounterText();

        if (waveDetailText != null)
        {
            waveDetailText.text = GamePauseController.Instance != null && GamePauseController.Instance.IsPaused
                ? "Paused"
                : waveManager.GetWaveDetailText();
        }

        if (callEarlyButton != null)
            callEarlyButton.interactable = waveManager.State == WaveState.Preparation
                                           && waveManager.PreparationTimeLeft > 0.5f
                                           && (GamePauseController.Instance == null || !GamePauseController.Instance.IsPaused);

        RefreshWaveIntelPanel();
    }

    void RefreshWaveIntelPanel()
    {
        if (waveIntelPanel == null || waveManager == null)
            return;

        var showIntel = waveManager.State == WaveState.Preparation && waveManager.UpcomingWaveDefinition != null;
        waveIntelPanel.SetActive(showIntel);

        if (!showIntel)
            return;

        if (waveIntelTitleText != null)
            waveIntelTitleText.text = $"Incoming · Wave {waveManager.CurrentWaveNumber}";

        if (waveIntelEnemyText != null)
            waveIntelEnemyText.text = waveManager.GetUpcomingEnemySummary();

        if (waveIntelHintText != null)
            waveIntelHintText.text = waveManager.GetUpcomingHint();
    }

    void RefreshTowerButtons()
    {
        if (buildSelector == null)
            return;

        foreach (var pair in towerButtonImages)
        {
            if (pair.Value == null)
                continue;

            if (!TowerBuildCatalog.IsImplemented(pair.Key))
            {
                pair.Value.color = new Color(0.15f, 0.15f, 0.15f, 0.5f);
                continue;
            }

            pair.Value.color = pair.Key == buildSelector.SelectedType
                ? new Color(0.28f, 0.48f, 0.28f, 0.95f)
                : new Color(0.2f, 0.2f, 0.2f, 0.85f);
        }
    }

    static void ConfigureLayoutElement(RectTransform rect, float height)
    {
        var element = rect.gameObject.GetComponent<LayoutElement>();
        if (element == null)
            element = rect.gameObject.AddComponent<LayoutElement>();

        element.minHeight = height;
        element.preferredHeight = height;
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

    static GameObject CreateButton(Transform parent, string label, Vector2 size, float fontSize, UnityAction onClick)
    {
        var go = CreateUiObject("Button", parent);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        UiDisplaySettings.SnapRectToPixels(rect);

        var image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.85f);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        if (onClick != null)
            button.onClick.AddListener(onClick);

        var text = CreateLabel(go.transform, label, fontSize, TextAlignmentOptions.Center);
        UiDisplaySettings.ApplyButtonText(text, fontSize);
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(6f, 4f);
        textRect.offsetMax = new Vector2(-6f, -4f);

        return go;
    }
}
