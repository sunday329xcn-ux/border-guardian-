using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup UI for fork gate and temporary scenic-route blocks (opened from map clicks).
/// </summary>
public class RouteControlUI : MonoBehaviour
{
    public static RouteControlUI Instance { get; private set; }

    enum PopupMode
    {
        None,
        Fork,
        Block
    }

    const float PanelWidth = 320f;
    const float ForkPanelHeight = 300f;
    const float BlockPanelHeight = 228f;

    MapRouteController routeController;
    WaveManager waveManager;
    PopupMode activePopup = PopupMode.None;
    RouteBlockType blockPopupType = RouteBlockType.None;

    GameObject overlayRoot;
    GameObject panelRoot;
    RectTransform panelRect;
    TextMeshProUGUI titleText;
    TextMeshProUGUI statusText;
    TextMeshProUGUI hintText;
    GameObject forkButtonGroup;
    Button upperForkButton;
    Button lowerForkButton;
    Button noneForkButton;
    GameObject blockButtonGroup;
    Button activateBlockButton;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());

        var map = FindObjectOfType<MapGridController>();
        if (map != null)
            routeController = map.Route;

        waveManager = FindObjectOfType<WaveManager>();
        CreatePopup();
        ClosePopup();

        if (routeController != null)
            routeController.OnRouteStateChanged += Refresh;

        if (waveManager != null)
            waveManager.OnWaveStateChanged += Refresh;

        TryBindGameManager();
    }

    bool resourcesBound;

    void TryBindGameManager()
    {
        if (resourcesBound || GameManager.Instance == null)
            return;

        GameManager.Instance.OnResourcesChanged += Refresh;
        resourcesBound = true;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (routeController != null)
            routeController.OnRouteStateChanged -= Refresh;

        if (waveManager != null)
            waveManager.OnWaveStateChanged -= Refresh;

        if (GameManager.Instance != null)
            GameManager.Instance.OnResourcesChanged -= Refresh;
    }

    void Update()
    {
        if (!resourcesBound)
            TryBindGameManager();

        if (activePopup == PopupMode.None || routeController == null)
            return;

        if (routeController.ActiveBlock != RouteBlockType.None)
            Refresh();
    }

    public void OpenForkPanel()
    {
        activePopup = PopupMode.Fork;
        ShowPopup("Fork Gate", ForkPanelHeight);
        if (titleText != null)
            titleText.text = "Fork Gate";

        SetGroupVisible(forkButtonGroup, true);
        SetGroupVisible(blockButtonGroup, false);
        Refresh();
    }

    public void OpenBlockPanel(RouteBlockType blockType)
    {
        if (blockType is RouteBlockType.None)
            return;

        activePopup = PopupMode.Block;
        blockPopupType = blockType;
        ShowPopup(blockType == RouteBlockType.UpperScenic ? "Block North Scenic" : "Block South Scenic", BlockPanelHeight);

        SetGroupVisible(forkButtonGroup, false);
        SetGroupVisible(blockButtonGroup, true);
        Refresh();
    }

    public void ClosePopup()
    {
        activePopup = PopupMode.None;
        blockPopupType = RouteBlockType.None;

        if (overlayRoot != null)
            overlayRoot.SetActive(false);
    }

    void ShowPopup(string title, float height)
    {
        if (overlayRoot == null)
            return;

        overlayRoot.SetActive(true);
        overlayRoot.transform.SetAsLastSibling();

        if (titleText != null)
            titleText.text = title;

        if (panelRect != null)
            panelRect.sizeDelta = new Vector2(PanelWidth, height);
    }

    void CreatePopup()
    {
        overlayRoot = new GameObject("RouteControlOverlay", typeof(RectTransform), typeof(Image));
        overlayRoot.transform.SetParent(transform, false);

        var overlayRect = overlayRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        var dim = overlayRoot.GetComponent<Image>();
        dim.color = new Color(0.03f, 0.05f, 0.03f, 0.55f);
        dim.raycastTarget = true;

        var dimButton = overlayRoot.AddComponent<Button>();
        dimButton.targetGraphic = dim;
        dimButton.onClick.AddListener(ClosePopup);

        panelRoot = new GameObject("RouteControlPanel", typeof(RectTransform), typeof(Image));
        panelRoot.transform.SetParent(overlayRoot.transform, false);

        panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(PanelWidth, ForkPanelHeight);
        panelRect.anchoredPosition = Vector2.zero;
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var panelBackground = panelRoot.GetComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(panelBackground, UiDisplaySettings.PanelAlpha);
        panelBackground.raycastTarget = true;

        titleText = CreateLine(panelRoot.transform, "Fork Gate", 0f, UiDisplaySettings.FontSizeBody, UiDisplaySettings.AccentText);
        statusText = CreateLine(panelRoot.transform, "Fork: None", -28f, UiDisplaySettings.FontSizeBody, UiDisplaySettings.BodyText);
        hintText = CreateLine(panelRoot.transform, "Select fork · 30g per change", -56f, UiDisplaySettings.FontSizeCaption, UiDisplaySettings.MutedText);

        forkButtonGroup = new GameObject("ForkButtons", typeof(RectTransform));
        forkButtonGroup.transform.SetParent(panelRoot.transform, false);
        var forkGroupRect = forkButtonGroup.GetComponent<RectTransform>();
        forkGroupRect.anchorMin = Vector2.zero;
        forkGroupRect.anchorMax = Vector2.one;
        forkGroupRect.offsetMin = Vector2.zero;
        forkGroupRect.offsetMax = Vector2.zero;

        upperForkButton = CreateForkButton(forkButtonGroup.transform, "Upper", -84f, ForkRouteMode.ForceUpper);
        lowerForkButton = CreateForkButton(forkButtonGroup.transform, "Lower", -124f, ForkRouteMode.ForceLower);
        noneForkButton = CreateForkButton(forkButtonGroup.transform, "None", -164f, ForkRouteMode.None);

        blockButtonGroup = new GameObject("BlockButtons", typeof(RectTransform));
        blockButtonGroup.transform.SetParent(panelRoot.transform, false);
        var blockGroupRect = blockButtonGroup.GetComponent<RectTransform>();
        blockGroupRect.anchorMin = Vector2.zero;
        blockGroupRect.anchorMax = Vector2.one;
        blockGroupRect.offsetMin = Vector2.zero;
        blockGroupRect.offsetMax = Vector2.zero;

        activateBlockButton = CreateActionButton(
            blockButtonGroup.transform,
            "Activate Block (100g)",
            -84f,
            TryActivateBlockFromPopup);

        CreateCloseButton(panelRoot.transform);
    }

    void CreateCloseButton(Transform parent)
    {
        var buttonObject = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.offsetMin = new Vector2(14f, 12f);
        rect.offsetMax = new Vector2(-14f, 46f);

        var image = buttonObject.GetComponent<Image>();
        UiDisplaySettings.ApplyBuildButton(image, selected: false);

        var button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(ClosePopup);

        var textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(buttonObject.transform, false);
        var textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 2f);
        textRect.offsetMax = new Vector2(-8f, -2f);

        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = "Close";
        text.alignment = TextAlignmentOptions.Center;
        UiDisplaySettings.ApplyButtonText(text, UiDisplaySettings.FontSizeCaption);
    }

    TextMeshProUGUI CreateLine(Transform parent, string text, float yOffset, float fontSize, Color color)
    {
        var labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(parent, false);

        var rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.offsetMin = new Vector2(14f, yOffset - 22f);
        rect.offsetMax = new Vector2(-14f, yOffset);

        var label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.alignment = TextAlignmentOptions.TopLeft;
        label.color = color;
        label.raycastTarget = false;
        UiDisplaySettings.ApplySharpText(label, fontSize);
        return label;
    }

    Button CreateForkButton(Transform parent, string label, float yOffset, ForkRouteMode mode)
    {
        var button = CreateActionButton(parent, label, yOffset, () => SelectFork(mode));
        button.name = $"Fork_{mode}";
        return button;
    }

    Button CreateActionButton(Transform parent, string label, float yOffset, UnityEngine.Events.UnityAction onClick)
    {
        var buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.offsetMin = new Vector2(14f, yOffset - 34f);
        rect.offsetMax = new Vector2(-14f, yOffset);

        var image = buttonObject.GetComponent<Image>();
        UiDisplaySettings.ApplyBuildButton(image, selected: false);

        var button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(buttonObject.transform, false);
        var textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 2f);
        textRect.offsetMax = new Vector2(-8f, -2f);

        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        UiDisplaySettings.ApplyButtonText(text, UiDisplaySettings.FontSizeCaption);
        return button;
    }

    void SelectFork(ForkRouteMode mode)
    {
        if (routeController == null)
            return;

        if (routeController.TrySetForkMode(mode))
            Refresh();
    }

    void TryActivateBlockFromPopup()
    {
        if (routeController == null || blockPopupType is RouteBlockType.None)
            return;

        if (routeController.TryActivateBlock(blockPopupType))
            Refresh();
    }

    void Refresh()
    {
        if (routeController == null)
        {
            if (statusText != null)
                statusText.text = "Route controls unavailable";

            SetButtonsInteractable(false);
            return;
        }

        if (activePopup == PopupMode.Fork && statusText != null)
            statusText.text = routeController.GetForkStatusText();

        if (activePopup == PopupMode.Block && statusText != null)
            statusText.text = routeController.GetBlockStatusText();

        if (hintText != null)
        {
            hintText.text = GetHintText();
        }

        var canControl = CanControlRoutes();
        var gold = GameManager.Instance != null ? GameManager.Instance.Gold : 0;
        var forkCost = MapRouteController.ForkSwitchGoldCost;

        if (activePopup == PopupMode.Fork)
        {
            RefreshForkButton(upperForkButton, ForkRouteMode.ForceUpper, canControl, gold, forkCost);
            RefreshForkButton(lowerForkButton, ForkRouteMode.ForceLower, canControl, gold, forkCost);
            RefreshForkButton(noneForkButton, ForkRouteMode.None, canControl, gold, forkCost);
        }

        if (activePopup == PopupMode.Block && activateBlockButton != null)
        {
            var blockReady = routeController.IsBlockReady && gold >= MapRouteController.BlockGoldCost;
            activateBlockButton.interactable = canControl && blockReady;

            var label = activateBlockButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = routeController.ActiveBlock == blockPopupType
                    ? $"Blocking {Mathf.CeilToInt(routeController.BlockTimeRemaining)}s"
                    : $"Activate Block ({MapRouteController.BlockGoldCost}g)";
            }
        }
    }

    string GetHintText()
    {
        if (waveManager != null && waveManager.State == WaveState.Victory)
            return "Route controls disabled.";

        return activePopup switch
        {
            PopupMode.Fork => "Fork applies instantly · 30g if mode changes.",
            PopupMode.Block => $"Block lasts {MapRouteController.BlockDurationSeconds:0.#}s · CD {MapRouteController.BlockCooldownSeconds:0.#}s.",
            _ => string.Empty
        };
    }

    bool CanControlRoutes()
    {
        if (waveManager != null && waveManager.State == WaveState.Victory)
            return false;

        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return false;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return false;

        return waveManager == null
               || waveManager.State is WaveState.Preparation or WaveState.Spawning or WaveState.Combat;
    }

    void RefreshForkButton(Button button, ForkRouteMode mode, bool interactable, int gold, int forkCost)
    {
        if (button == null)
            return;

        var selected = routeController.ForkMode == mode;
        var image = button.GetComponent<Image>();
        UiDisplaySettings.ApplyBuildButton(image, selected);

        var label = button.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = selected ? $"{GetForkLabel(mode)} ✓" : $"{GetForkLabel(mode)} ({forkCost}g)";

        button.interactable = interactable && (selected || gold >= forkCost);
    }

    static string GetForkLabel(ForkRouteMode mode)
    {
        return mode switch
        {
            ForkRouteMode.ForceUpper => "Upper",
            ForkRouteMode.ForceLower => "Lower",
            _ => "None"
        };
    }

    void SetButtonsInteractable(bool value)
    {
        if (upperForkButton != null)
            upperForkButton.interactable = value;

        if (lowerForkButton != null)
            lowerForkButton.interactable = value;

        if (noneForkButton != null)
            noneForkButton.interactable = value;

        if (activateBlockButton != null)
            activateBlockButton.interactable = value;
    }

    static void SetGroupVisible(GameObject group, bool visible)
    {
        if (group != null)
            group.SetActive(visible);
    }
}
