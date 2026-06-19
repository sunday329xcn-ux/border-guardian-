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
        starsText.color = result.StarsEarned >= 3
            ? new Color(1f, 0.88f, 0.35f)
            : new Color(0.95f, 0.82f, 0.45f);

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
        overlayRoot.SetActive(true);
    }

    static string BuildUnlockText(int totalKeys)
    {
        if (LevelProgressService.IsLevelUnlocked(LevelId.LavafallRift))
            return "Lavafall Rift unlocked! (Coming soon)";

        var needed = LevelProgressService.LavafallRiftKeyCost - totalKeys;
        return needed > 0
            ? $"Lavafall Rift locked · Need {needed} more key(s) ({totalKeys}/{LevelProgressService.LavafallRiftKeyCost})"
            : "Lavafall Rift unlocked! (Coming soon)";
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
        Hide();
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
        dim.color = new Color(0.03f, 0.05f, 0.03f, 0.78f);
        dim.raycastTarget = true;

        var panel = CreateUiObject("VictoryPanel", overlayRoot.transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(520f, 360f);
        panelRect.anchoredPosition = Vector2.zero;
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var panelBackground = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(panelBackground, 0.96f);

        titleText = CreateLabel(panel.transform, "Victory!", 42f, TextAlignmentOptions.Center);
        LayoutLine(titleText.rectTransform, -20f, 52f, stretch: true);

        starsText = CreateLabel(panel.transform, "★★★", 36f, TextAlignmentOptions.Center);
        starsText.color = new Color(1f, 0.88f, 0.35f);
        LayoutLine(starsText.rectTransform, -78f, 42f, stretch: true);

        livesText = CreateLabel(panel.transform, string.Empty, 20f, TextAlignmentOptions.Center);
        livesText.color = new Color(0.88f, 0.92f, 0.88f);
        LayoutLine(livesText.rectTransform, -124f, 28f, stretch: true);

        keysText = CreateLabel(panel.transform, string.Empty, 18f, TextAlignmentOptions.Center);
        keysText.color = new Color(0.95f, 0.92f, 0.75f);
        LayoutLine(keysText.rectTransform, -158f, 52f, stretch: true);

        unlockText = CreateLabel(panel.transform, string.Empty, 16f, TextAlignmentOptions.Center);
        unlockText.color = new Color(0.75f, 0.85f, 0.95f);
        LayoutLine(unlockText.rectTransform, -220f, 28f, stretch: true);

        var continueButton = CreateButton(panel.transform, "Continue", new Vector2(160f, 44f), 18f, OnContinueClicked);
        PlaceBottomButton(continueButton.GetComponent<RectTransform>(), -110f);

        var resetButton = CreateButton(panel.transform, "Reset", new Vector2(160f, 44f), 18f, OnResetClicked);
        PlaceBottomButton(resetButton.GetComponent<RectTransform>(), 62f);
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

    static GameObject CreateButton(Transform parent, string label, Vector2 size, float fontSize, UnityEngine.Events.UnityAction onClick)
    {
        var go = CreateUiObject("Button", parent);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        UiDisplaySettings.SnapRectToPixels(rect);

        var image = go.AddComponent<Image>();
        image.color = new Color(0.28f, 0.48f, 0.28f, 0.95f);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
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
