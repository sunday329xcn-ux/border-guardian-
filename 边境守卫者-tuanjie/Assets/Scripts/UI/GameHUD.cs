using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Top HUD: gold, diamonds, lives.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] TextMeshProUGUI diamondText;
    [SerializeField] TextMeshProUGUI livesText;
    [SerializeField] TextMeshProUGUI statusText;

    [Header("Layout")]
    [SerializeField] bool autoLayoutTopLeft = true;
    [SerializeField] float leftPadding = UiDisplaySettings.HudLeftInset;
    [SerializeField] float topPadding = UiDisplaySettings.HudTopPadding;
    [SerializeField] float lineHeight = UiDisplaySettings.HudLineHeight;

    Image resourceBackground;

    bool statusCentered;
    bool resourcesBound;
    Coroutine transientStatusRoutine;

    void Start()
    {
        DisableHudRaycasts();

        if (autoLayoutTopLeft)
        {
            leftPadding = UiDisplaySettings.HudLeftInset;
            topPadding = UiDisplaySettings.HudTopPadding;
            lineHeight = UiDisplaySettings.HudLineHeight;
            EnsureResourceBackground();
            ApplyTopLeftLayout();
        }

        ApplySharpHudText();
        TryBindGameManager();
    }

    void Update()
    {
        if (!resourcesBound)
            TryBindGameManager();
    }

    void TryBindGameManager()
    {
        if (resourcesBound || GameManager.Instance == null)
            return;

        GameManager.Instance.OnResourcesChanged += Refresh;
        GameManager.Instance.OnGameOver += ShowGameOver;
        resourcesBound = true;
        Refresh();
    }

    void OnDestroy()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnResourcesChanged -= Refresh;
        GameManager.Instance.OnGameOver -= ShowGameOver;
    }

    void EnsureResourceBackground()
    {
        if (resourceBackground != null)
            return;

        var panelObject = new GameObject("ResourcePanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(transform, false);
        panelObject.transform.SetAsFirstSibling();

        var rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(leftPadding - 12f, -topPadding + 8f);
        rect.sizeDelta = new Vector2(260f, lineHeight * 3f + 22f);
        UiDisplaySettings.SnapRectToPixels(rect);

        resourceBackground = panelObject.GetComponent<Image>();
        resourceBackground.raycastTarget = false;
        UiDisplaySettings.ApplyPanelBackground(resourceBackground, UiDisplaySettings.PanelAlpha);
    }

    void ApplyTopLeftLayout()
    {
        LayoutLine(goldText, 0);
        LayoutLine(diamondText, 1);
        LayoutLine(livesText, 2);
    }

    void LayoutLine(TextMeshProUGUI text, int lineIndex)
    {
        if (text == null) return;

        var rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(leftPadding, -topPadding - lineIndex * lineHeight);
        rect.sizeDelta = new Vector2(240f, lineHeight - 4f);

        text.alignment = TextAlignmentOptions.TopLeft;
        text.margin = new Vector4(4f, 0f, 8f, 0f);
        text.raycastTarget = false;
        UiDisplaySettings.SnapRectToPixels(rect);
    }

    void ApplySharpHudText()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());
        UiDisplaySettings.ApplyHudBodyText(goldText, UiDisplaySettings.FontSizeTitle);
        UiDisplaySettings.ApplyHudBodyText(diamondText, UiDisplaySettings.FontSizeTitle);
        UiDisplaySettings.ApplyHudBodyText(livesText, UiDisplaySettings.FontSizeTitle);
        UiDisplaySettings.ApplySharpText(statusText, 32f);
    }

    void DisableHudRaycasts()
    {
        SetRaycastOff(goldText);
        SetRaycastOff(diamondText);
        SetRaycastOff(livesText);
        SetRaycastOff(statusText);
    }

    static void SetRaycastOff(TextMeshProUGUI text)
    {
        if (text != null)
            text.raycastTarget = false;
    }

    void Refresh()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (goldText != null) goldText.text = $"Gold: {gm.Gold}";
        if (diamondText != null) diamondText.text = $"Diamonds: {gm.Diamonds}";
        if (livesText != null) livesText.text = $"Lives: {gm.Lives}";

        if (statusText != null && !gm.IsGameOver && !statusCentered)
            statusText.text = string.Empty;
    }

    void ShowGameOver()
    {
        SetStatus("Game Over", true);
    }

    public void ShowTransientMessage(string message, float durationSeconds = 2.5f)
    {
        if (statusText == null)
            return;

        if (transientStatusRoutine != null)
            StopCoroutine(transientStatusRoutine);

        SetStatus(message);
        transientStatusRoutine = StartCoroutine(ClearTransientStatusAfter(durationSeconds));
    }

    IEnumerator ClearTransientStatusAfter(float durationSeconds)
    {
        yield return new WaitForSeconds(durationSeconds);
        transientStatusRoutine = null;

        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
            SetStatus(string.Empty);
    }

    public void SetStatus(string message, bool centerScreen = false)
    {
        if (statusText == null)
            return;

        statusCentered = centerScreen && !string.IsNullOrEmpty(message);

        if (statusCentered)
        {
            var rect = statusText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(800f, 120f);
            statusText.alignment = TextAlignmentOptions.Center;
            UiDisplaySettings.ApplySharpText(statusText, 72f);
        }
        else if (autoLayoutTopLeft)
        {
            var rect = statusText.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(leftPadding, -topPadding - 3 * lineHeight - 8f);
            rect.sizeDelta = new Vector2(320f, 40f);
            statusText.alignment = TextAlignmentOptions.TopLeft;
            UiDisplaySettings.ApplySharpText(statusText, 36f);
        }

        statusText.text = message;
    }
}
