using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EasterEggCelebrationUI : MonoBehaviour
{
    public static EasterEggCelebrationUI Instance { get; private set; }

    GameObject overlayRoot;
    TextMeshProUGUI titleText;
    TextMeshProUGUI rewardText;

    public bool IsShowing => overlayRoot != null && overlayRoot.activeSelf;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());
        CreateOverlay();
        Hide();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void ShowCelebration(string title, string rewardDescription)
    {
        var ui = Instance ?? FindObjectOfType<EasterEggCelebrationUI>();
        if (ui == null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                ui = canvas.gameObject.AddComponent<EasterEggCelebrationUI>();
        }

        ui?.Display(title, rewardDescription);
    }

    void Display(string title, string rewardDescription)
    {
        if (overlayRoot == null)
            CreateOverlay();

        titleText.text = title;
        rewardText.text = rewardDescription;
        overlayRoot.SetActive(true);
        GamePauseController.Instance?.Pause();
    }

    void Hide()
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(false);
    }

    void OnContinueClicked()
    {
        Hide();
        GamePauseController.Instance?.Resume();
    }

    void CreateOverlay()
    {
        overlayRoot = CreateUiObject("EasterEggOverlay", transform);
        var overlayRect = overlayRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        var dim = overlayRoot.AddComponent<Image>();
        UiDisplaySettings.ApplyDimOverlay(dim);

        var panel = CreateUiObject("EasterEggPanel", overlayRoot.transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(520f, 320f);
        panelRect.anchoredPosition = Vector2.zero;
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var panelBackground = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(panelBackground, UiDisplaySettings.PanelAlpha);

        titleText = CreateLabel(panel.transform, "Congratulations!", 40f, TextAlignmentOptions.Center);
        titleText.color = UiDisplaySettings.StarGold;
        LayoutLine(titleText.rectTransform, -24f, 52f);

        rewardText = CreateLabel(panel.transform, string.Empty, UiDisplaySettings.FontSizeBody, TextAlignmentOptions.Center);
        UiDisplaySettings.ApplyHudBodyText(rewardText, UiDisplaySettings.FontSizeBody);
        LayoutLine(rewardText.rectTransform, -92f, 96f);

        var continueButton = CreateButton(panel.transform, "Continue", new Vector2(180f, 44f), 18f, OnContinueClicked);
        var buttonRect = continueButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 24f);
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
        UiDisplaySettings.ApplyBuildButton(image, selected: false);

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

    static void LayoutLine(RectTransform rect, float anchoredY, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, anchoredY);
        rect.sizeDelta = new Vector2(-48f, height);
    }
}
