using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    const float ScreenPadding = 24f;

    GameObject overlayRoot;
    TextMeshProUGUI overlayTitle;
    Button resumeButton;
    Button pauseToggleButton;
    TextMeshProUGUI pauseToggleText;

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());
        EnsurePauseController();

        CreatePauseToggleButton();
        CreateOverlay();

        if (GamePauseController.Instance != null)
            GamePauseController.Instance.OnPauseChanged += HandlePauseChanged;

        SetOverlayVisible(false);
    }

    void OnDestroy()
    {
        if (GamePauseController.Instance != null)
            GamePauseController.Instance.OnPauseChanged -= HandlePauseChanged;
    }

    void EnsurePauseController()
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

    void CreatePauseToggleButton()
    {
        var buttonObject = CreateUiObject("PauseToggleButton", transform);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(96f, 36f);
        rect.anchoredPosition = new Vector2(-ScreenPadding - 296f, -ScreenPadding);
        UiDisplaySettings.SnapRectToPixels(rect);

        var image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        pauseToggleButton = buttonObject.AddComponent<Button>();
        pauseToggleButton.targetGraphic = image;
        pauseToggleButton.onClick.AddListener(OnPauseToggleClicked);

        pauseToggleText = CreateLabel(buttonObject.transform, "Pause", 17f, TextAlignmentOptions.Center);
        StretchLabel(pauseToggleText.rectTransform);
    }

    void CreateOverlay()
    {
        overlayRoot = CreateUiObject("PauseOverlay", transform);
        var overlayRect = overlayRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        var dim = overlayRoot.AddComponent<Image>();
        dim.color = new Color(0.04f, 0.06f, 0.04f, 0.72f);
        dim.raycastTarget = true;

        var panel = CreateUiObject("PausePanel", overlayRoot.transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(320f, 180f);
        panelRect.anchoredPosition = Vector2.zero;
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var panelBackground = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(panelBackground, 0.95f);

        overlayTitle = CreateLabel(panel.transform, "Paused", 36f, TextAlignmentOptions.Center);
        var titleRect = overlayTitle.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -24f);
        titleRect.sizeDelta = new Vector2(-32f, 48f);

        var hint = CreateLabel(panel.transform, "Press Esc to resume", 16f, TextAlignmentOptions.Center);
        hint.color = new Color(0.8f, 0.85f, 0.8f);
        var hintRect = hint.rectTransform;
        hintRect.anchorMin = new Vector2(0f, 1f);
        hintRect.anchorMax = new Vector2(1f, 1f);
        hintRect.pivot = new Vector2(0.5f, 1f);
        hintRect.anchoredPosition = new Vector2(0f, -78f);
        hintRect.sizeDelta = new Vector2(-32f, 24f);

        var resumeObject = CreateUiObject("ResumeButton", panel.transform);
        var resumeRect = resumeObject.GetComponent<RectTransform>();
        resumeRect.anchorMin = new Vector2(0.5f, 0f);
        resumeRect.anchorMax = new Vector2(0.5f, 0f);
        resumeRect.pivot = new Vector2(0.5f, 0f);
        resumeRect.sizeDelta = new Vector2(180f, 44f);
        resumeRect.anchoredPosition = new Vector2(0f, 24f);
        UiDisplaySettings.SnapRectToPixels(resumeRect);

        var resumeImage = resumeObject.AddComponent<Image>();
        resumeImage.color = new Color(0.28f, 0.48f, 0.28f, 0.95f);

        resumeButton = resumeObject.AddComponent<Button>();
        resumeButton.targetGraphic = resumeImage;
        resumeButton.onClick.AddListener(OnResumeClicked);

        var resumeText = CreateLabel(resumeObject.transform, "Resume", 20f, TextAlignmentOptions.Center);
        StretchLabel(resumeText.rectTransform);
    }

    void OnPauseToggleClicked()
    {
        GamePauseController.Instance?.TogglePause();
    }

    void OnResumeClicked()
    {
        GamePauseController.Instance?.Resume();
    }

    void HandlePauseChanged(bool paused)
    {
        SetOverlayVisible(paused);

        if (pauseToggleText != null)
            pauseToggleText.text = paused ? "Resume" : "Pause";
    }

    void SetOverlayVisible(bool visible)
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(visible);
    }

    static void StretchLabel(RectTransform rect)
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
