using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bottom-centre hero HUD (P4.1): status line (HP / level / revive) plus three
/// ability buttons with live cooldown labels. Created at runtime by
/// <c>GameUiController</c>. Placed bottom-centre to respect the no-overlap UI
/// rule (left = Goblin Missile, right = build rail).
/// </summary>
public class HeroSkillBarUI : MonoBehaviour
{
    const float PanelWidth = 320f;
    const float PanelHeight = 96f;
    const float ButtonWidth = 96f;
    const float ButtonHeight = 44f;

    HeroController controller;
    TextMeshProUGUI statusLabel;
    Button meteorButton;
    Button freezeButton;
    Button reinforceButton;
    TextMeshProUGUI meteorLabel;
    TextMeshProUGUI freezeLabel;
    TextMeshProUGUI reinforceLabel;

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());
        controller = FindObjectOfType<HeroController>();
        CreatePanel();
    }

    void Update()
    {
        if (controller == null)
            controller = FindObjectOfType<HeroController>();

        Refresh();
    }

    void CreatePanel()
    {
        var panel = new GameObject("HeroSkillBar", typeof(RectTransform));
        panel.transform.SetParent(transform, false);

        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
        panelRect.anchoredPosition = new Vector2(0f, UiDisplaySettings.ScreenPadding);
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var background = panel.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(background, UiDisplaySettings.PanelAlpha);

        statusLabel = CreateLabel(panel.transform, "Hero", 14f, TextAlignmentOptions.Center);
        statusLabel.color = UiDisplaySettings.AccentText;
        var statusRect = statusLabel.rectTransform;
        statusRect.anchorMin = new Vector2(0f, 1f);
        statusRect.anchorMax = new Vector2(1f, 1f);
        statusRect.pivot = new Vector2(0.5f, 1f);
        statusRect.anchoredPosition = new Vector2(0f, -6f);
        statusRect.sizeDelta = new Vector2(-16f, 24f);

        meteorButton = CreateSkillButton(panel.transform, -1, out meteorLabel, () => OnSkillClicked(HeroSkillId.Meteor));
        freezeButton = CreateSkillButton(panel.transform, 0, out freezeLabel, () => OnSkillClicked(HeroSkillId.Freeze));
        reinforceButton = CreateSkillButton(panel.transform, 1, out reinforceLabel, () => OnSkillClicked(HeroSkillId.Reinforce));
    }

    Button CreateSkillButton(Transform parent, int column, out TextMeshProUGUI label, UnityEngine.Events.UnityAction onClick)
    {
        var button = CreateButton(parent, "", new Vector2(ButtonWidth, ButtonHeight), 13f, onClick);
        var rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(column * (ButtonWidth + 8f), 10f);
        label = button.GetComponentInChildren<TextMeshProUGUI>();
        return button;
    }

    void OnSkillClicked(HeroSkillId skill)
    {
        if (controller == null)
            return;

        if (skill == HeroSkillId.Meteor && controller.IsMeteorArmed)
        {
            controller.CancelArmedSkill();
            return;
        }

        controller.ActivateSkill(skill);
    }

    void Refresh()
    {
        if (statusLabel == null)
            return;

        var hero = controller != null ? controller.Hero : null;
        if (hero == null)
        {
            statusLabel.text = "Hero — deploying...";
            SetButtonsInteractable(false);
            return;
        }

        if (hero.IsDead)
            statusLabel.text = $"Hero Lv.{hero.Level} — reviving {Mathf.CeilToInt(hero.ReviveRemaining)}s";
        else
            statusLabel.text = $"Hero Lv.{hero.Level}   HP {hero.CurrentHealth}/{hero.MaxHealth}   XP {hero.Experience}/{hero.ExperienceToNext}";

        UpdateSkillButton(meteorButton, meteorLabel, HeroSkillId.Meteor, "Meteor",
            controller.IsMeteorArmed ? "Aim..." : null);
        UpdateSkillButton(freezeButton, freezeLabel, HeroSkillId.Freeze, "Freeze", null);
        UpdateSkillButton(reinforceButton, reinforceLabel, HeroSkillId.Reinforce, "Reinforce", null);
    }

    void UpdateSkillButton(Button button, TextMeshProUGUI label, HeroSkillId skill, string name, string overrideText)
    {
        if (button == null || label == null)
            return;

        var ready = controller.IsSkillReady(skill);
        button.interactable = ready;

        if (!string.IsNullOrEmpty(overrideText))
        {
            label.text = $"{name}\n{overrideText}";
            button.interactable = true;
            return;
        }

        if (ready)
        {
            label.text = $"{name}\nReady";
            return;
        }

        var cd = controller.GetCooldownRemaining(skill);
        label.text = $"{name}\nCD {Mathf.CeilToInt(cd)}s";
    }

    void SetButtonsInteractable(bool interactable)
    {
        if (meteorButton != null) meteorButton.interactable = interactable;
        if (freezeButton != null) freezeButton.interactable = interactable;
        if (reinforceButton != null) reinforceButton.interactable = interactable;
    }

    static TextMeshProUGUI CreateLabel(Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.alignment = alignment;
        label.color = Color.white;
        label.raycastTarget = false;
        UiDisplaySettings.ApplySharpText(label, fontSize);
        return label;
    }

    static Button CreateButton(Transform parent, string label, Vector2 size, float fontSize, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Button", typeof(RectTransform));
        go.transform.SetParent(parent, false);
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
        textRect.offsetMin = new Vector2(4f, 2f);
        textRect.offsetMax = new Vector2(-4f, -2f);

        return button;
    }
}
