using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    const float ScreenPadding = 24f;

    Action onBack;
    GameObject overlayRoot;
    Canvas rootCanvas;
    Slider sfxSlider;
    Slider uiScaleSlider;
    Toggle reduceMotionToggle;
    Toggle fullscreenToggle;
    TextMeshProUGUI difficultyValueText;
    GameDifficulty selectedDifficulty = GameDifficulty.Normal;

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void Show(Action backCallback)
    {
        onBack = backCallback;
        EnsureBuilt();
        selectedDifficulty = GameDifficultyService.Current;
        RefreshDifficultyLabel();
        sfxSlider.value = CombatFeedbackService.MasterVolume;
        uiScaleSlider.value = UiDisplaySettings.UiScale;
        reduceMotionToggle.isOn = CombatFeedbackService.ReduceMotion;
        fullscreenToggle.isOn = Screen.fullScreen;
        overlayRoot.SetActive(true);
    }

    public void Hide()
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(false);
    }

    void EnsureBuilt()
    {
        if (overlayRoot != null)
            return;

        overlayRoot = CreateUiObject("SettingsOverlay", transform);
        Stretch(overlayRoot.GetComponent<RectTransform>());

        var backdrop = overlayRoot.AddComponent<Image>();
        backdrop.color = Color.black;
        backdrop.raycastTarget = true;

        var panel = CreateUiObject("SettingsPanel", overlayRoot.transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(520f, 560f);
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var panelBackground = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(panelBackground, 0.98f);

        var title = CreateLabel(panel.transform, "Settings", 34f, TextAlignmentOptions.Center);
        LayoutTop(title.rectTransform, -24f, 44f, true);

        var subtitle = CreateLabel(panel.transform, "Audio, display and difficulty", 16f, TextAlignmentOptions.Center);
        subtitle.color = new Color(0.78f, 0.85f, 0.75f);
        LayoutTop(subtitle.rectTransform, -68f, 24f, true);

        CreateSectionLabel(panel.transform, "Difficulty", -108f);
        CreateDifficultyRow(panel.transform, -142f);
        CreateSectionLabel(panel.transform, "Audio", -196f);
        sfxSlider = CreateSliderRow(panel.transform, "SFX Volume", -230f, CombatFeedbackService.MasterVolume,
            value => CombatFeedbackService.SetMasterVolume(value));
        CreateSectionLabel(panel.transform, "Display", -284f);
        uiScaleSlider = CreateSliderRow(panel.transform, "UI Scale", -318f, UiDisplaySettings.UiScale,
            value =>
            {
                UiDisplaySettings.SetUiScale(value);
                UiDisplaySettings.ApplyUiScale(rootCanvas);
            }, 0.75f, 1.25f);
        reduceMotionToggle = CreateToggleRow(panel.transform, "Reduce Motion", -372f,
            CombatFeedbackService.ReduceMotion,
            value => CombatFeedbackService.SetReduceMotion(value));
        fullscreenToggle = CreateToggleRow(panel.transform, "Fullscreen", -426f, Screen.fullScreen,
            value => Screen.fullScreen = value);

        CreateActionButton(panel.transform, "Back", -492f, new Color(0.28f, 0.48f, 0.28f, 0.95f), Close);
        overlayRoot.SetActive(false);
    }

    void CreateDifficultyRow(Transform parent, float y)
    {
        var row = CreateUiObject("DifficultyRow", parent);
        var rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 1f);
        rowRect.anchorMax = new Vector2(0.5f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.anchoredPosition = new Vector2(0f, y);
        rowRect.sizeDelta = new Vector2(420f, 40f);
        UiDisplaySettings.SnapRectToPixels(rowRect);

        CreateSmallButton(row.transform, "<", new Vector2(-170f, 0f), CycleDifficultyPrevious);
        difficultyValueText = CreateLabel(row.transform, GameDifficultyService.GetDisplayName(selectedDifficulty), 20f,
            TextAlignmentOptions.Center);
        var valueRect = difficultyValueText.rectTransform;
        valueRect.anchorMin = new Vector2(0.5f, 0.5f);
        valueRect.anchorMax = new Vector2(0.5f, 0.5f);
        valueRect.pivot = new Vector2(0.5f, 0.5f);
        valueRect.sizeDelta = new Vector2(180f, 32f);
        CreateSmallButton(row.transform, ">", new Vector2(170f, 0f), CycleDifficultyNext);
    }

    void CycleDifficultyPrevious()
    {
        selectedDifficulty = selectedDifficulty switch
        {
            GameDifficulty.Normal => GameDifficulty.Easy,
            GameDifficulty.Heroic => GameDifficulty.Normal,
            _ => GameDifficulty.Heroic
        };
        GameDifficultyService.SetDifficulty(selectedDifficulty);
        RefreshDifficultyLabel();
    }

    void CycleDifficultyNext()
    {
        selectedDifficulty = selectedDifficulty switch
        {
            GameDifficulty.Easy => GameDifficulty.Normal,
            GameDifficulty.Normal => GameDifficulty.Heroic,
            _ => GameDifficulty.Easy
        };
        GameDifficultyService.SetDifficulty(selectedDifficulty);
        RefreshDifficultyLabel();
    }

    void RefreshDifficultyLabel()
    {
        if (difficultyValueText != null)
            difficultyValueText.text = GameDifficultyService.GetDisplayName(selectedDifficulty);
    }

    void Close()
    {
        Hide();
        onBack?.Invoke();
    }

    void CreateSectionLabel(Transform parent, string text, float y)
    {
        var label = CreateLabel(parent, text, 18f, TextAlignmentOptions.TopLeft);
        LayoutTop(label.rectTransform, y, 24f, true);
        label.color = new Color(0.95f, 0.9f, 0.65f);
    }

    Slider CreateSliderRow(Transform parent, string labelText, float y, float initialValue, Action<float> onChanged,
        float min = 0f, float max = 1f)
    {
        var label = CreateLabel(parent, labelText, 16f, TextAlignmentOptions.TopLeft);
        LayoutTop(label.rectTransform, y, 20f, true);

        var sliderObject = CreateUiObject($"{labelText}Slider", parent);
        var sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 1f);
        sliderRect.anchorMax = new Vector2(0.5f, 1f);
        sliderRect.pivot = new Vector2(0.5f, 1f);
        sliderRect.anchoredPosition = new Vector2(0f, y - 28f);
        sliderRect.sizeDelta = new Vector2(420f, 22f);
        UiDisplaySettings.SnapRectToPixels(sliderRect);

        var background = sliderObject.AddComponent<Image>();
        background.color = new Color(0.15f, 0.18f, 0.15f, 0.95f);

        var slider = sliderObject.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = initialValue;
        slider.onValueChanged.AddListener(value => onChanged?.Invoke(value));
        return slider;
    }

    Toggle CreateToggleRow(Transform parent, string labelText, float y, bool initialValue, Action<bool> onChanged)
    {
        var row = CreateUiObject($"{labelText}Row", parent);
        var rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 1f);
        rowRect.anchorMax = new Vector2(0.5f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.anchoredPosition = new Vector2(0f, y);
        rowRect.sizeDelta = new Vector2(420f, 32f);
        UiDisplaySettings.SnapRectToPixels(rowRect);

        var label = CreateLabel(row.transform, labelText, 16f, TextAlignmentOptions.MidlineLeft);
        var labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 0f);
        labelRect.offsetMax = new Vector2(-72f, 0f);

        var toggleObject = CreateUiObject("Toggle", row.transform);
        var toggleRect = toggleObject.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(1f, 0.5f);
        toggleRect.anchorMax = new Vector2(1f, 0.5f);
        toggleRect.pivot = new Vector2(1f, 0.5f);
        toggleRect.sizeDelta = new Vector2(52f, 28f);
        toggleRect.anchoredPosition = new Vector2(-8f, 0f);

        var toggleBackground = toggleObject.AddComponent<Image>();
        toggleBackground.color = new Color(0.15f, 0.18f, 0.15f, 0.95f);

        var checkObject = CreateUiObject("Checkmark", toggleObject.transform);
        Stretch(checkObject.GetComponent<RectTransform>());
        var checkImage = checkObject.AddComponent<Image>();
        checkImage.color = new Color(0.45f, 0.78f, 0.42f, 0.95f);

        var toggle = toggleObject.AddComponent<Toggle>();
        toggle.targetGraphic = toggleBackground;
        toggle.graphic = checkImage;
        toggle.isOn = initialValue;
        toggle.onValueChanged.AddListener(value => onChanged?.Invoke(value));
        return toggle;
    }

    void CreateActionButton(Transform parent, string label, float y, Color color, Action onClick)
    {
        var buttonObject = CreateUiObject($"{label}Button", parent);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(220f, 44f);
        UiDisplaySettings.SnapRectToPixels(rect);

        var image = buttonObject.AddComponent<Image>();
        image.color = color;

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => onClick?.Invoke());

        var text = CreateLabel(buttonObject.transform, label, 20f, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);
    }

    void CreateSmallButton(Transform parent, string label, Vector2 position, Action onClick)
    {
        var buttonObject = CreateUiObject($"{label}Button", parent);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(44f, 36f);
        rect.anchoredPosition = position;
        UiDisplaySettings.SnapRectToPixels(rect);

        var image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.22f, 0.28f, 0.22f, 0.95f);

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => onClick?.Invoke());

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
            rect.sizeDelta = new Vector2(-48f, height);
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
