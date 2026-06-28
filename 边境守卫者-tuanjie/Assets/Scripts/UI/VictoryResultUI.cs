using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryResultUI : MonoBehaviour
{
    GameObject overlayRoot;
    TextMeshProUGUI titleText;
    TextMeshProUGUI starsText;
    TextMeshProUGUI livesText;
    TextMeshProUGUI keysText;
    TextMeshProUGUI unlockText;
    TextMeshProUGUI statsText;

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());
        CreateOverlay();
        Hide();
    }

    public void Show(LevelVictoryResult result, int livesRemaining, int maxLives)
    {
        if (overlayRoot == null)
            return;

        titleText.text = "Victory!";
        starsText.text = LevelStarRating.BuildStarText(result.StarsEarned);
        starsText.color = result.StarsEarned >= 3 ? UiDisplaySettings.StarGold : UiDisplaySettings.AccentText;

        livesText.text = $"Lives remaining: {livesRemaining} / {maxLives}";

        if (result.KeysGained > 0)
        {
            keysText.text = result.Improved && result.PreviousBestStars > 0
                ? $"+{result.KeysGained} Keys  (Best {LevelStarRating.BuildStarText(result.PreviousBestStars, false)} → {LevelStarRating.BuildStarText(result.StarsEarned, false)})\nTotal Keys: {result.TotalKeys}"
                : $"+{result.KeysGained} Keys earned\nTotal Keys: {result.TotalKeys}";
        }
        else
        {
            keysText.text = result.StarsEarned > 0
                ? $"No new keys (Best: {LevelStarRating.BuildStarText(result.StarsEarned, false)})\nTotal Keys: {result.TotalKeys}"
                : $"Total Keys: {result.TotalKeys}";
        }

        unlockText.text = BuildUnlockText(result.TotalKeys);
        statsText.text = CombatStatsTracker.BuildVictorySummaryText();
        overlayRoot.SetActive(true);
    }

    static string BuildUnlockText(int totalKeys)
    {
        var available = TalentService.AvailableKeys;
        var owned = 0;
        foreach (var id in TalentService.All)
        {
            if (TalentService.IsPurchased(id))
                owned++;
        }

        var total = TalentService.All.Count;
        if (owned >= total)
            return "All talents unlocked!  (Main Menu → Talents)";

        return available > 0
            ? $"Talents: {owned}/{total} unlocked · {available} key(s) ready to spend (Main Menu → Talents)"
            : $"Talents: {owned}/{total} unlocked · earn more stars for keys (Main Menu → Talents)";
    }

    void Hide()
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(false);
    }

    void OnResetClicked()
    {
        Time.timeScale = 1f;
        var waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.ResetLevel();
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnContinueClicked()
    {
        MainMenuUI.ReturnToFrontEnd();
    }

    void CreateOverlay()
    {
        overlayRoot = CreateUiObject("VictoryOverlay", transform);
        var overlayRect = overlayRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        var dim = overlayRoot.AddComponent<Image>();
        UiDisplaySettings.ApplyDimOverlay(dim);
        ProceduralUiSkin.AttachFade(overlayRoot);

        var panel = CreateUiObject("VictoryPanel", overlayRoot.transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(540f, 520f);
        panelRect.anchoredPosition = Vector2.zero;
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var panelBackground = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(panelBackground, UiDisplaySettings.PanelAlpha);

        titleText = CreateLabel(panel.transform, "Victory!", 42f, TextAlignmentOptions.Center);
        LayoutLine(titleText.rectTransform, -20f, 52f, stretch: true);

        starsText = CreateLabel(panel.transform, "***", 36f, TextAlignmentOptions.Center);
        starsText.color = UiDisplaySettings.StarGold;
        LayoutLine(starsText.rectTransform, -78f, 42f, stretch: true);

        livesText = CreateLabel(panel.transform, string.Empty, UiDisplaySettings.FontSizeBody, TextAlignmentOptions.Center);
        UiDisplaySettings.ApplyHudBodyText(livesText, UiDisplaySettings.FontSizeBody);
        LayoutLine(livesText.rectTransform, -124f, 28f, stretch: true);

        keysText = CreateLabel(panel.transform, string.Empty, UiDisplaySettings.FontSizeBody, TextAlignmentOptions.Center);
        keysText.color = UiDisplaySettings.AccentText;
        LayoutLine(keysText.rectTransform, -154f, 48f, stretch: true);

        unlockText = CreateLabel(panel.transform, string.Empty, UiDisplaySettings.FontSizeCaption, TextAlignmentOptions.Center);
        unlockText.color = UiDisplaySettings.MutedText;
        LayoutLine(unlockText.rectTransform, -206f, 24f, stretch: true);

        var statsHeader = CreateLabel(panel.transform, "Combat Stats", UiDisplaySettings.FontSizeBody, TextAlignmentOptions.TopLeft);
        statsHeader.color = UiDisplaySettings.AccentText;
        LayoutLine(statsHeader.rectTransform, -236f, 22f, stretch: true);

        CreateStatsScroll(panel.transform);

        var continueButton = CreateButton(panel.transform, "Continue", new Vector2(160f, 44f), OnContinueClicked);
        PlaceBottomButton(continueButton.GetComponent<RectTransform>(), -110f);

        var resetButton = CreateButton(panel.transform, "Reset", new Vector2(160f, 44f), OnResetClicked, accent: false);
        PlaceBottomButton(resetButton.GetComponent<RectTransform>(), 62f);
    }

    void CreateStatsScroll(Transform parent)
    {
        var scrollObject = CreateUiObject("StatsScroll", parent);
        var scrollRect = scrollObject.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0f, 0f);
        scrollRect.anchorMax = new Vector2(1f, 1f);
        scrollRect.offsetMin = new Vector2(24f, 84f);
        scrollRect.offsetMax = new Vector2(-24f, -268f);

        var scrollBackground = scrollObject.AddComponent<Image>();
        scrollBackground.color = new Color(0.05f, 0.07f, 0.05f, 0.88f);
        scrollBackground.raycastTarget = true;

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

        statsText = CreateLabel(content.transform, string.Empty, UiDisplaySettings.FontSizeCaption, TextAlignmentOptions.TopLeft);
        UiDisplaySettings.ApplyHudBodyText(statsText, UiDisplaySettings.FontSizeCaption);
        statsText.lineSpacing = -2f;
        var bodyRect = statsText.rectTransform;
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0f, 1f);
        bodyRect.anchoredPosition = Vector2.zero;
        bodyRect.sizeDelta = new Vector2(-12f, 0f);

        var bodyFitter = statsText.gameObject.AddComponent<ContentSizeFitter>();
        bodyFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        bodyFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void LayoutLine(RectTransform rect, float y, float height, bool stretch = false)
    {
        if (stretch)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(-40f, height);
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(440f, height);
    }

    static void PlaceBottomButton(RectTransform rect, float xOffset)
    {
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(xOffset, 24f);
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
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(6f, 4f);
        textRect.offsetMax = new Vector2(-6f, -4f);
        return go;
    }
}
