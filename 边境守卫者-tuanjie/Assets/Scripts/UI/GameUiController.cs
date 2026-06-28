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
    const float ScreenPadding = UiDisplaySettings.ScreenPadding;
    const float BuildButtonHeight = 52f;

    static readonly TowerType[] AllTowerOrder =
    {
        TowerType.Arrow,
        TowerType.Frost,
        TowerType.Cannon,
        TowerType.Arcane,
        TowerType.Barracks,
        TowerType.DiamondMine,
        TowerType.Spotter,
        TowerType.Beacon,
        TowerType.BountyShrine
    };

    [SerializeField] WaveManager waveManager;
    [SerializeField] TowerBuildSelector buildSelector;

    TextMeshProUGUI waveTitleText;
    TextMeshProUGUI waveDetailText;
    Button callEarlyButton;
    Button resetButton;
    GameObject buildPanelRoot;
    GameObject wavePanelRoot;
    readonly Dictionary<TowerType, Image> towerButtonImages = new();
    GamePauseController boundPauseController;

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponent<Canvas>());

        if (waveManager == null)
            waveManager = FindObjectOfType<WaveManager>();

        if (buildSelector == null)
            buildSelector = FindObjectOfType<TowerBuildSelector>();

        CreateBuildPanel();
        CreateWavePanel();

        if (GetComponent<WaveSpawnHintUI>() == null)
            gameObject.AddComponent<WaveSpawnHintUI>();

        if (GetComponent<PauseMenuUI>() == null)
            gameObject.AddComponent<PauseMenuUI>();

        if (GetComponent<GameplayExitUI>() == null)
            gameObject.AddComponent<GameplayExitUI>();

        if (GetComponent<CombatPresentationUI>() == null)
            gameObject.AddComponent<CombatPresentationUI>();

        if (GetComponent<CombatFeedbackService>() == null)
            gameObject.AddComponent<CombatFeedbackService>();

        if (GetComponent<VictoryResultUI>() == null)
            gameObject.AddComponent<VictoryResultUI>();

        if (GetComponent<LevelProgressHudUI>() == null)
            gameObject.AddComponent<LevelProgressHudUI>();

        if (GetComponent<CodexMenuUI>() == null)
            gameObject.AddComponent<CodexMenuUI>();

        if (GetComponent<RouteControlUI>() == null)
            gameObject.AddComponent<RouteControlUI>();

        if (GetComponent<PlatformTerrainInfoUI>() == null)
            gameObject.AddComponent<PlatformTerrainInfoUI>();

        if (GetComponent<TowerInfoPanelUI>() == null)
            gameObject.AddComponent<TowerInfoPanelUI>();

        if (GetComponent<EnemyInfoPanelUI>() == null)
            gameObject.AddComponent<EnemyInfoPanelUI>();

        if (GetComponent<MainMenuUI>() == null)
            gameObject.AddComponent<MainMenuUI>();

        if (GetComponent<WaveTimelineUI>() == null)
            gameObject.AddComponent<WaveTimelineUI>();

        if (GetComponent<TowerBuildHotkeys>() == null)
            gameObject.AddComponent<TowerBuildHotkeys>();

        if (GetComponent<GoblinMissileUI>() == null)
            gameObject.AddComponent<GoblinMissileUI>();

        if (GetComponent<HeroController>() == null)
            gameObject.AddComponent<HeroController>();

        if (GetComponent<HeroSkillBarUI>() == null)
            gameObject.AddComponent<HeroSkillBarUI>();

        if (GetComponent<MapModifierController>() == null)
            gameObject.AddComponent<MapModifierController>();

        if (GetComponent<MapModifierHudUI>() == null)
            gameObject.AddComponent<MapModifierHudUI>();

        if (GetComponent<WaveBuffSelectionUI>() == null)
            gameObject.AddComponent<WaveBuffSelectionUI>();

        if (GetComponent<ScoreHudUI>() == null)
            gameObject.AddComponent<ScoreHudUI>();

        if (GetComponent<AtmosphereController>() == null)
            gameObject.AddComponent<AtmosphereController>();

        if (GetComponent<EasterEggCelebrationUI>() == null)
            gameObject.AddComponent<EasterEggCelebrationUI>();

        if (GetComponent<EasterEggHintBannerUI>() == null)
            gameObject.AddComponent<EasterEggHintBannerUI>();

        EnsureRangePreviewController();

        if (waveManager != null)
            waveManager.OnWaveStateChanged += RefreshWaveUi;

        TryBindPauseEvents();

        if (buildSelector != null)
        {
            buildSelector.OnSelectionChanged += HandleBuildSelectionChanged;
            RefreshTowerButtons();
        }

        BuildSlotSelectionController.OnSelectionChanged += HandleBuildSlotSelectionChanged;
        RefreshWaveUi();
    }

    void OnDestroy()
    {
        if (waveManager != null)
            waveManager.OnWaveStateChanged -= RefreshWaveUi;

        if (boundPauseController != null)
            boundPauseController.OnPauseChanged -= HandlePauseChanged;

        if (buildSelector != null)
            buildSelector.OnSelectionChanged -= HandleBuildSelectionChanged;

        BuildSlotSelectionController.OnSelectionChanged -= HandleBuildSlotSelectionChanged;
    }

    void HandleBuildSelectionChanged(TowerType _)
    {
        RefreshTowerButtons();
    }

    void HandleBuildSlotSelectionChanged(BuildSlot _)
    {
        RefreshTowerButtons();
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
        buildPanelRoot = panel;
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.offsetMin = new Vector2(-UiDisplaySettings.BuildRailInsetFromRight, UiDisplaySettings.ScreenPadding);
        panelRect.offsetMax = new Vector2(-UiDisplaySettings.RightEdgeInset, -UiDisplaySettings.BuildPanelTop);
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var background = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(background);

        var scrollObject = CreateUiObject("BuildScroll", panel.transform);
        var scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 32f;
        Stretch(scrollRect.GetComponent<RectTransform>());

        var viewport = CreateUiObject("BuildViewport", scrollObject.transform);
        var viewportRect = viewport.GetComponent<RectTransform>();
        Stretch(viewportRect);
        viewport.AddComponent<RectMask2D>();
        scrollRect.viewport = viewportRect;

        var content = CreateUiObject("BuildContent", viewport.transform);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);
        scrollRect.content = contentRect;

        var layout = content.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(6, 6, 8, 8);
        layout.spacing = 6f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var contentFitter = content.AddComponent<ContentSizeFitter>();
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        foreach (var towerType in AllTowerOrder)
            CreateTowerBuildButton(content.transform, towerType);
    }

    void CreateTowerBuildButton(Transform parent, TowerType towerType)
    {
        var cost = TowerBuildCatalog.GetBuildCost(towerType);
        var name = TowerBuildCatalog.GetDisplayName(towerType);
        var label = $"{name}\n{cost}g";

        var capturedType = towerType;
        var buttonObject = CreateButton(parent, label, new Vector2(0f, BuildButtonHeight), 15f, null);
        ConfigureLayoutElement(buttonObject.GetComponent<RectTransform>(), BuildButtonHeight);

        var dragHandler = buttonObject.AddComponent<TowerBuildDragHandler>();
        dragHandler.Initialize(capturedType, buildSelector);

        var hoverHandler = buttonObject.AddComponent<TowerBuildButtonHover>();
        hoverHandler.Initialize(capturedType);

        var button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            if (dragHandler.ConsumeClickSuppression())
                return;

            if (buildSelector != null)
                buildSelector.Select(capturedType);

            if (BuildSlotSelectionController.Selected != null)
                TowerBuildService.TryBuild(capturedType, BuildSlotSelectionController.Selected);
        });

        towerButtonImages[towerType] = buttonObject.GetComponent<Image>();
        ProceduralUiSkin.AddTowerIcon(buttonObject.transform, towerType);
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    void CreateWavePanel()
    {
        var panel = CreateUiObject("WavePanel", transform);
        wavePanelRoot = panel;
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.sizeDelta = new Vector2(UiDisplaySettings.WavePanelWidth, UiDisplaySettings.WavePanelHeight);
        panelRect.anchoredPosition = UiDisplaySettings.WavePanelAnchoredPosition;
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var background = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(background, UiDisplaySettings.PanelAlpha);

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 14, 14);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        waveTitleText = CreateLabel(panel.transform, "Wave 1 / 10", UiDisplaySettings.FontSizeTitle, TextAlignmentOptions.TopLeft);
        ConfigureLayoutElement(waveTitleText.rectTransform, 28f);

        waveDetailText = CreateLabel(panel.transform, "Next in 15s", UiDisplaySettings.FontSizeBody, TextAlignmentOptions.TopLeft);
        UiDisplaySettings.ApplyHudBodyText(waveDetailText, UiDisplaySettings.FontSizeBody);
        ConfigureLayoutElement(waveDetailText.rectTransform, 24f);

        callEarlyButton = CreateButton(panel.transform, "Call Early (+gold)", new Vector2(0f, 40f), 18f, () =>
        {
            if (waveManager != null)
                waveManager.CallWaveEarly();
        }).GetComponent<Button>();

        ConfigureLayoutElement(callEarlyButton.GetComponent<RectTransform>(), 40f);

        resetButton = CreateButton(panel.transform, "Reset", new Vector2(0f, 40f), 18f, () =>
        {
            if (waveManager != null)
                waveManager.ResetLevel();
        }).GetComponent<Button>();

        ConfigureLayoutElement(resetButton.GetComponent<RectTransform>(), 40f);
        resetButton.gameObject.SetActive(false);
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
        {
            var isPreparation = waveManager.State == WaveState.Preparation;
            callEarlyButton.gameObject.SetActive(isPreparation);
            callEarlyButton.interactable = isPreparation
                                           && waveManager.PreparationTimeLeft > 0.5f
                                           && (GamePauseController.Instance == null || !GamePauseController.Instance.IsPaused);
        }

        if (resetButton != null)
        {
            var isVictory = waveManager.State == WaveState.Victory;
            resetButton.gameObject.SetActive(isVictory);
            resetButton.interactable = isVictory;
        }
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

            UiDisplaySettings.ApplyBuildButton(
                pair.Value,
                buildSelector.IsBuildBarActive && pair.Key == buildSelector.SelectedType);
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
        UiDisplaySettings.ApplyBuildButton(image, selected: false);

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

    public void SetGameplayVisible(bool visible)
    {
        if (buildPanelRoot != null)
            buildPanelRoot.SetActive(visible);

        if (wavePanelRoot != null)
            wavePanelRoot.SetActive(visible);

        SetChildComponentVisible<WaveTimelineUI>(visible);
        SetChildComponentVisible<LevelProgressHudUI>(visible);
        SetChildComponentVisible<PauseMenuUI>(visible);
        SetChildComponentVisible<GameplayExitUI>(visible);
        SetChildComponentVisible<CombatPresentationUI>(visible);
        SetChildComponentVisible<WaveSpawnHintUI>(visible);
        SetChildComponentVisible<TowerInfoPanelUI>(visible);
        SetChildComponentVisible<EnemyInfoPanelUI>(visible);
        SetChildComponentVisible<TowerBuildHotkeys>(visible);
        SetChildComponentVisible<GoblinMissileUI>(visible);
        SetChildComponentVisible<EasterEggCelebrationUI>(visible);

        if (!visible)
            EasterEggHintBannerUI.HideHint();

        SetChildComponentVisible<VictoryResultUI>(visible);
    }

    void SetChildComponentVisible<T>(bool visible) where T : Behaviour
    {
        var component = GetComponent<T>();
        if (component != null)
            component.enabled = visible;
    }

    void EnsureRangePreviewController()
    {
        var map = FindObjectOfType<MapGridController>();
        if (map == null || map.GetComponent<TowerRangePreviewController>() != null)
            return;

        map.gameObject.AddComponent<TowerRangePreviewController>();
    }
}
