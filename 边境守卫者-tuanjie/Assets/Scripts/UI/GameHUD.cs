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
    [SerializeField] float leftPadding = 28f;
    [SerializeField] float topPadding = 28f;
    [SerializeField] float lineHeight = 42f;

    Image resourceBackground;

    bool statusCentered;

    void Start()
    {
        DisableHudRaycasts();

        if (autoLayoutTopLeft)
        {
            EnsureResourceBackground();
            ApplyTopLeftLayout();
        }

        ApplySharpHudText();

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager is missing from the scene.");
            return;
        }

        GameManager.Instance.OnResourcesChanged += Refresh;
        GameManager.Instance.OnGameOver += ShowGameOver;
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
        rect.anchoredPosition = new Vector2(leftPadding - 14f, -topPadding + 10f);
        rect.sizeDelta = new Vector2(240f, lineHeight * 3f + 22f);
        UiDisplaySettings.SnapRectToPixels(rect);

        resourceBackground = panelObject.GetComponent<Image>();
        resourceBackground.raycastTarget = false;
        UiDisplaySettings.ApplyPanelBackground(resourceBackground, 0.88f);
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
        rect.sizeDelta = new Vector2(220f, lineHeight - 4f);

        text.alignment = TextAlignmentOptions.TopLeft;
        text.raycastTarget = false;
        UiDisplaySettings.SnapRectToPixels(rect);
    }

    void ApplySharpHudText()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());
        UiDisplaySettings.ApplySharpText(goldText, 28f);
        UiDisplaySettings.ApplySharpText(diamondText, 28f);
        UiDisplaySettings.ApplySharpText(livesText, 28f);
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
