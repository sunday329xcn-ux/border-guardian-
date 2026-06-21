using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    const float LoadingDuration = 1.4f;

    static bool sessionStarted;

    public static bool IsSessionStarted => sessionStarted;

    public static void MarkSessionStarted()
    {
        sessionStarted = true;
    }

    public static void ClearSessionStarted()
    {
        sessionStarted = false;
    }

    public static void ReturnToFrontEnd()
    {
        Time.timeScale = 1f;
        ClearSessionStarted();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    GameObject overlayRoot;
    GameObject homePanel;
    GameObject loadingPanel;
    RectTransform loadingFillRect;
    TextMeshProUGUI loadingStatusText;

    CodexMenuUI codexMenu;
    SettingsMenuUI settingsMenu;
    GameUiController gameUi;
    WaveManager waveManager;
    GameObject mapRoot;

    void Awake()
    {
        mapRoot = FindObjectOfType<MapGridController>()?.gameObject;
        gameUi = GetComponent<GameUiController>();
        codexMenu = GetComponent<CodexMenuUI>();
    }

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());
        EnsurePauseController();
        waveManager = FindObjectOfType<WaveManager>();

        settingsMenu = GetComponent<SettingsMenuUI>();
        if (settingsMenu == null)
            settingsMenu = gameObject.AddComponent<SettingsMenuUI>();

        CreateOverlay();

        if (sessionStarted)
        {
            HideFrontEnd();
            EnableGameplayImmediate();
            GamePauseController.Instance?.Resume();
            waveManager?.StartGameplay();
            return;
        }

        ShowFrontEnd();
        GamePauseController.Instance?.Pause();
    }

    void ShowFrontEnd()
    {
        overlayRoot.SetActive(true);
        homePanel.SetActive(true);
        loadingPanel.SetActive(false);
        settingsMenu?.Hide();
        codexMenu?.HideFromFrontEnd();

        if (mapRoot != null)
            mapRoot.SetActive(false);

        gameUi?.SetGameplayVisible(false);
        codexMenu?.SetInGameButtonVisible(false);
    }

    void HideFrontEnd()
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(false);
    }

    void EnableGameplayImmediate()
    {
        if (mapRoot != null)
            mapRoot.SetActive(true);

        gameUi?.SetGameplayVisible(true);
        codexMenu?.SetInGameButtonVisible(true);
    }

    void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        homePanel.SetActive(false);
        loadingPanel.SetActive(true);
        UpdateLoadingBar(0f, "Loading map...");

        var elapsed = 0f;
        while (elapsed < LoadingDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(elapsed / LoadingDuration);
            var status = progress < 0.45f
                ? "Loading map..."
                : progress < 0.85f
                    ? "Preparing waves..."
                    : "Ready!";
            UpdateLoadingBar(progress, status);
            yield return null;
        }

        UpdateLoadingBar(1f, "Ready!");
        sessionStarted = true;

        if (mapRoot != null)
            mapRoot.SetActive(true);

        gameUi?.SetGameplayVisible(true);
        codexMenu?.SetInGameButtonVisible(true);

        GameManager.Instance?.ApplySessionStart();
        CombatStatsTracker.Reset();
        GamePauseController.Instance?.Resume();
        waveManager?.StartGameplay();

        loadingPanel.SetActive(false);
        HideFrontEnd();
    }

    void UpdateLoadingBar(float progress, string status)
    {
        if (loadingFillRect != null)
            loadingFillRect.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);

        if (loadingStatusText != null)
            loadingStatusText.text = status;
    }

    void OpenCodex()
    {
        homePanel.SetActive(false);
        overlayRoot.SetActive(false);
        codexMenu?.OpenFromFrontEnd(ShowHome);
    }

    void OpenSettings()
    {
        homePanel.SetActive(false);
        overlayRoot.SetActive(false);
        settingsMenu?.Show(ShowHome);
    }

    void ShowHome()
    {
        settingsMenu?.Hide();
        codexMenu?.HideFromFrontEnd();
        overlayRoot.SetActive(true);
        homePanel.SetActive(true);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    static void EnsurePauseController()
    {
        if (GamePauseController.Instance != null)
            return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.gameObject.AddComponent<GamePauseController>();
            return;
        }

        var controllerObject = new GameObject("GamePauseController");
        controllerObject.AddComponent<GamePauseController>();
    }

    void CreateOverlay()
    {
        overlayRoot = CreateUiObject("MainMenuOverlay", transform);
        Stretch(overlayRoot.GetComponent<RectTransform>());

        var backdrop = overlayRoot.AddComponent<Image>();
        backdrop.color = Color.black;
        backdrop.raycastTarget = true;

        homePanel = CreateUiObject("HomePanel", overlayRoot.transform);
        Stretch(homePanel.GetComponent<RectTransform>());

        var panel = CreateUiObject("MainMenuPanel", homePanel.transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(420f, 360f);
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var panelBackground = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(panelBackground, 0.96f);

        var title = CreateLabel(panel.transform, "Border Guard", 42f, TextAlignmentOptions.Center);
        LayoutTop(title.rectTransform, -28f, 52f, stretch: true);

        var subtitle = CreateLabel(panel.transform, "Grimm Forest", 20f, TextAlignmentOptions.Center);
        subtitle.color = new Color(0.78f, 0.85f, 0.75f);
        LayoutTop(subtitle.rectTransform, -84f, 28f, stretch: true);

        CreateMenuButton(panel.transform, "Play", -132f, new Color(0.28f, 0.48f, 0.28f, 0.95f), StartGame);
        CreateMenuButton(panel.transform, "Codex", -188f, new Color(0.22f, 0.28f, 0.22f, 0.95f), OpenCodex);
        CreateMenuButton(panel.transform, "Settings", -244f, new Color(0.22f, 0.28f, 0.22f, 0.95f), OpenSettings);
        CreateMenuButton(panel.transform, "Quit", -300f, new Color(0.35f, 0.18f, 0.18f, 0.95f), QuitGame);

        loadingPanel = CreateUiObject("LoadingPanel", overlayRoot.transform);
        Stretch(loadingPanel.GetComponent<RectTransform>());

        var loadingTitle = CreateLabel(loadingPanel.transform, "Loading", 34f, TextAlignmentOptions.Center);
        var loadingTitleRect = loadingTitle.rectTransform;
        loadingTitleRect.anchorMin = new Vector2(0.5f, 0.5f);
        loadingTitleRect.anchorMax = new Vector2(0.5f, 0.5f);
        loadingTitleRect.pivot = new Vector2(0.5f, 0.5f);
        loadingTitleRect.anchoredPosition = new Vector2(0f, 48f);
        loadingTitleRect.sizeDelta = new Vector2(420f, 44f);

        var barRoot = CreateUiObject("LoadingBar", loadingPanel.transform);
        var barRect = barRoot.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 0.5f);
        barRect.anchorMax = new Vector2(0.5f, 0.5f);
        barRect.pivot = new Vector2(0.5f, 0.5f);
        barRect.anchoredPosition = new Vector2(0f, 0f);
        barRect.sizeDelta = new Vector2(420f, 18f);
        UiDisplaySettings.SnapRectToPixels(barRect);

        var barBackground = barRoot.AddComponent<Image>();
        barBackground.color = new Color(0.15f, 0.18f, 0.15f, 0.95f);

        var fillObject = CreateUiObject("Fill", barRoot.transform);
        loadingFillRect = fillObject.GetComponent<RectTransform>();
        loadingFillRect.anchorMin = Vector2.zero;
        loadingFillRect.anchorMax = Vector2.one;
        loadingFillRect.offsetMin = new Vector2(2f, 2f);
        loadingFillRect.offsetMax = new Vector2(-2f, -2f);

        var fillImage = fillObject.AddComponent<Image>();
        UiDisplaySettings.ApplyWhiteSprite(fillImage);
        fillImage.color = new Color(0.45f, 0.78f, 0.42f, 0.95f);

        loadingStatusText = CreateLabel(loadingPanel.transform, "Loading map...", 18f, TextAlignmentOptions.Center);
        var statusRect = loadingStatusText.rectTransform;
        statusRect.anchorMin = new Vector2(0.5f, 0.5f);
        statusRect.anchorMax = new Vector2(0.5f, 0.5f);
        statusRect.pivot = new Vector2(0.5f, 0.5f);
        statusRect.anchoredPosition = new Vector2(0f, -36f);
        statusRect.sizeDelta = new Vector2(420f, 28f);
        loadingStatusText.color = new Color(0.82f, 0.88f, 0.82f);

        loadingPanel.SetActive(false);
    }

    void CreateMenuButton(Transform parent, string label, float y, Color color, UnityEngine.Events.UnityAction onClick)
    {
        var buttonObject = CreateUiObject($"{label}Button", parent);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(240f, 42f);
        UiDisplaySettings.SnapRectToPixels(rect);

        var image = buttonObject.AddComponent<Image>();
        image.color = color;

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var text = CreateLabel(buttonObject.transform, label, 20f, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);
    }

    static void LayoutTop(RectTransform rect, float y, float height, bool stretch = false)
    {
        if (stretch)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(-32f, height);
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(240f, height);
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
}
