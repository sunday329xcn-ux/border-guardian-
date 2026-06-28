using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Roguelike pick-1-of-3 buff selection (P4.2). Pops up centred after each
/// cleared wave (not after final victory), modal-freezing time until the player
/// chooses. Placement: screen centre with a full-screen dim that blocks input,
/// so it never overlaps the combat HUD clusters.
/// </summary>
public class WaveBuffSelectionUI : MonoBehaviour
{
    WaveManager waveManager;
    bool bound;

    GameObject overlayRoot;
    Transform cardsRoot;
    TextMeshProUGUI titleLabel;
    bool frozenByThis;

    readonly List<Button> cardButtons = new();

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());
        CreateOverlay();
        HideOverlay();
        TryBind();
    }

    void Update()
    {
        if (!bound)
            TryBind();
    }

    void TryBind()
    {
        if (bound)
            return;

        waveManager = FindObjectOfType<WaveManager>();
        if (waveManager == null)
            return;

        waveManager.OnWaveCleared += HandleWaveCleared;
        bound = true;
    }

    void OnDestroy()
    {
        if (waveManager != null)
            waveManager.OnWaveCleared -= HandleWaveCleared;
    }

    void HandleWaveCleared(int clearedWave)
    {
        if (!RoguelikeModifierService.Enabled)
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        ShowChoices(clearedWave);
    }

    void ShowChoices(int clearedWave)
    {
        var choices = RoguelikeModifierService.RollChoices(3);
        if (choices.Count == 0)
            return;

        PopulateCards(choices);
        if (titleLabel != null)
            titleLabel.text = $"Wave {clearedWave} cleared — choose a buff";

        overlayRoot.SetActive(true);

        if (GamePauseController.Instance != null)
        {
            GamePauseController.Instance.BeginModalFreeze();
            frozenByThis = true;
        }
        else
        {
            Time.timeScale = 0f;
            frozenByThis = true;
        }
    }

    void OnChoicePicked(RoguelikeBuffId buff)
    {
        RoguelikeModifierService.Apply(buff);
        HideOverlay();

        if (frozenByThis)
        {
            frozenByThis = false;
            if (GamePauseController.Instance != null)
                GamePauseController.Instance.EndModalFreeze();
            else if (GameSpeedController.Instance != null)
                GameSpeedController.Instance.ApplySpeedIfRunning();
            else
                Time.timeScale = 1f;
        }
    }

    void HideOverlay()
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(false);
    }

    void CreateOverlay()
    {
        overlayRoot = new GameObject("WaveBuffOverlay", typeof(RectTransform));
        overlayRoot.transform.SetParent(transform, false);
        var rootRect = overlayRoot.GetComponent<RectTransform>();
        Stretch(rootRect);

        var dim = overlayRoot.AddComponent<Image>();
        dim.color = UiDisplaySettings.DimOverlay;
        dim.raycastTarget = true;

        var panel = new GameObject("Panel", typeof(RectTransform));
        panel.transform.SetParent(overlayRoot.transform, false);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(540f, 340f);
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var background = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(background, 0.97f);

        titleLabel = CreateLabel(panel.transform, "Choose a buff", 22f, TextAlignmentOptions.Center);
        titleLabel.color = UiDisplaySettings.AccentText;
        var titleRect = titleLabel.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -16f);
        titleRect.sizeDelta = new Vector2(-32f, 36f);

        cardsRoot = new GameObject("Cards", typeof(RectTransform)).transform;
        cardsRoot.SetParent(panel.transform, false);
        var cardsRect = (RectTransform)cardsRoot;
        Stretch(cardsRect);
        cardsRect.offsetMin = new Vector2(20f, 20f);
        cardsRect.offsetMax = new Vector2(-20f, -64f);
    }

    void PopulateCards(List<RoguelikeBuffId> choices)
    {
        for (int i = cardButtons.Count - 1; i >= 0; i--)
        {
            if (cardButtons[i] != null)
                Destroy(cardButtons[i].gameObject);
        }
        cardButtons.Clear();

        var rowHeight = 80f;
        for (int i = 0; i < choices.Count; i++)
        {
            var buff = choices[i];
            var text = $"{RoguelikeModifierService.GetTitle(buff)}\n<size=70%>{RoguelikeModifierService.GetDescription(buff)}</size>";
            var button = CreateButton(cardsRoot, text, new Vector2(0f, rowHeight - 8f), 18f, () => OnChoicePicked(buff));
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -i * rowHeight);
            rect.sizeDelta = new Vector2(-8f, rowHeight - 8f);
            cardButtons.Add(button);
        }
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static TextMeshProUGUI CreateLabel(Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.alignment = alignment;
        label.color = Color.white;
        label.richText = true;
        label.raycastTarget = false;
        UiDisplaySettings.ApplySharpText(label, fontSize);
        return label;
    }

    static Button CreateButton(Transform parent, string label, Vector2 size, float fontSize, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("BuffCard", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        var image = go.AddComponent<Image>();
        UiDisplaySettings.ApplyBuildButton(image, selected: false);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var text = CreateLabel(go.transform, label, fontSize, TextAlignmentOptions.Center);
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 4f);
        textRect.offsetMax = new Vector2(-10f, -4f);

        return button;
    }
}
