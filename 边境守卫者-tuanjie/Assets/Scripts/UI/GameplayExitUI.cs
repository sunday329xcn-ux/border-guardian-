using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayExitUI : MonoBehaviour
{
    const float ScreenPadding = UiDisplaySettings.ScreenPadding;

    GameObject exitButtonObject;
    GameObject confirmOverlayRoot;
    bool frozenForExitConfirm;
    bool wasPausedBeforeExitConfirm;

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());
        CreateExitButton();
        CreateConfirmOverlay();
        SetConfirmVisible(false);
        RefreshExitButton();
    }

    void Update()
    {
        RefreshExitButton();
    }

    void OnDisable()
    {
        if (confirmOverlayRoot != null && confirmOverlayRoot.activeSelf)
            OnExitCancelled();

        if (exitButtonObject != null)
            exitButtonObject.SetActive(false);
    }

    void CreateExitButton()
    {
        exitButtonObject = CreateUiObject("ExitButton", transform);
        var rect = exitButtonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(UiDisplaySettings.ExitButtonWidth, UiDisplaySettings.ControlButtonHeight);
        rect.anchoredPosition = UiDisplaySettings.ExitButtonAnchoredPosition;
        UiDisplaySettings.SnapRectToPixels(rect);

        var image = exitButtonObject.AddComponent<Image>();
        UiDisplaySettings.ApplyDangerButton(image);

        var button = exitButtonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(OnExitClicked);

        var label = CreateLabel(exitButtonObject.transform, "Exit", 17f, TextAlignmentOptions.Center);
        StretchLabel(label.rectTransform);
    }

    void CreateConfirmOverlay()
    {
        confirmOverlayRoot = CreateUiObject("ExitConfirmOverlay", transform);
        var overlayRect = confirmOverlayRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        var dim = confirmOverlayRoot.AddComponent<Image>();
        UiDisplaySettings.ApplyDimOverlay(dim);

        var panel = CreateUiObject("ExitConfirmPanel", confirmOverlayRoot.transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(420f, 220f);
        panelRect.anchoredPosition = Vector2.zero;
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var panelBackground = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(panelBackground, 0.98f);

        var title = CreateLabel(panel.transform, "Exit to Main Menu?", 28f, TextAlignmentOptions.Center);
        LayoutTop(title.rectTransform, -24f, 40f);

        var message = CreateLabel(panel.transform, "Unsaved progress in this run will be lost.", 16f, TextAlignmentOptions.Center);
        message.color = new Color(0.82f, 0.88f, 0.82f);
        LayoutTop(message.rectTransform, -72f, 48f);

        CreateDialogButton(panel.transform, "Yes", -148f, new Vector2(-92f, 0f), new Color(0.35f, 0.18f, 0.18f, 0.95f),
            OnExitConfirmed);
        CreateDialogButton(panel.transform, "No", -148f, new Vector2(92f, 0f), new Color(0.28f, 0.48f, 0.28f, 0.95f),
            OnExitCancelled);
    }

    void OnExitClicked()
    {
        if (!CanShowExit())
            return;

        wasPausedBeforeExitConfirm = GamePauseController.Instance != null && GamePauseController.Instance.IsPaused;
        frozenForExitConfirm = false;

        if (!wasPausedBeforeExitConfirm)
        {
            Time.timeScale = 0f;
            frozenForExitConfirm = true;
        }

        SetConfirmVisible(true);
    }

    void OnExitConfirmed()
    {
        SetConfirmVisible(false);
        frozenForExitConfirm = false;
        MainMenuUI.ReturnToFrontEnd();
    }

    void OnExitCancelled()
    {
        SetConfirmVisible(false);

        if (frozenForExitConfirm)
        {
            frozenForExitConfirm = false;
            if (GameSpeedController.Instance != null)
                GameSpeedController.Instance.ApplySpeedIfRunning();
            else
                Time.timeScale = 1f;
        }
    }

    bool CanShowExit()
    {
        if (!MainMenuUI.IsSessionStarted)
            return false;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return false;

        var waveManager = FindObjectOfType<WaveManager>();
        return waveManager == null || waveManager.State != WaveState.Victory;
    }

    void RefreshExitButton()
    {
        if (exitButtonObject == null)
            return;

        exitButtonObject.SetActive(MainMenuUI.IsSessionStarted);

        var button = exitButtonObject.GetComponent<Button>();
        if (button != null)
            button.interactable = CanShowExit() && confirmOverlayRoot != null && !confirmOverlayRoot.activeSelf;
    }

    void SetConfirmVisible(bool visible)
    {
        if (confirmOverlayRoot != null)
            confirmOverlayRoot.SetActive(visible);

        RefreshExitButton();
    }

    void CreateDialogButton(Transform parent, string label, float y, Vector2 xOffset, Color color,
        UnityEngine.Events.UnityAction onClick)
    {
        var buttonObject = CreateUiObject($"{label}Button", parent);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(xOffset.x, y);
        rect.sizeDelta = new Vector2(140f, 44f);
        UiDisplaySettings.SnapRectToPixels(rect);

        var image = buttonObject.AddComponent<Image>();
        image.color = color;

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var text = CreateLabel(buttonObject.transform, label, 20f, TextAlignmentOptions.Center);
        StretchLabel(text.rectTransform);
    }

    static void LayoutTop(RectTransform rect, float y, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(-40f, height);
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
