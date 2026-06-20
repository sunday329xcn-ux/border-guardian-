using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyInfoPanelUI : MonoBehaviour
{
    GameObject panelRoot;
    TextMeshProUGUI titleText;
    TextMeshProUGUI detailText;

    void Start()
    {
        CreatePanel();
        HidePanel();
        EnemySelectionController.OnSelectionChanged += HandleSelectionChanged;
    }

    void OnDestroy()
    {
        EnemySelectionController.OnSelectionChanged -= HandleSelectionChanged;
    }

    void Update()
    {
        var enemy = EnemySelectionController.Selected;
        if (enemy == null || panelRoot == null || !panelRoot.activeSelf)
            return;

        if (enemy.IsDead)
        {
            EnemySelectionController.Deselect();
            return;
        }

        RefreshPanel(enemy);
    }

    void HandleSelectionChanged(EnemyBase enemy)
    {
        if (enemy == null)
        {
            HidePanel();
            return;
        }

        ShowPanel();
        RefreshPanel(enemy);
    }

    void RefreshPanel(EnemyBase enemy)
    {
        if (enemy == null || titleText == null || detailText == null)
            return;

        titleText.text = EnemyCatalog.GetDisplayName(enemy.EnemyType);
        detailText.text =
            $"HP: {Mathf.Clamp(enemy.CurrentHealth, 0, enemy.MaxHealth)} / {enemy.MaxHealth}\n" +
            $"Armor: {enemy.GetArmorRating()} ({enemy.Armor})\n" +
            $"Magic Resist: {enemy.GetMagicResistRating()} ({enemy.MagicResist})\n" +
            $"Threat: {enemy.GetAttackRating()}";
    }

    void ShowPanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    void HidePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    void CreatePanel()
    {
        const float buildBarClearance = 116f;

        panelRoot = CreateUiObject("EnemyInfoPanel", transform);
        var panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0f);
        panelRect.pivot = new Vector2(1f, 0f);
        panelRect.sizeDelta = new Vector2(320f, 168f);
        panelRect.anchoredPosition = new Vector2(-24f, buildBarClearance);
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var background = panelRoot.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(background, 0.94f);

        titleText = CreateLabel(panelRoot.transform, "Enemy", 22f, TextAlignmentOptions.TopLeft);
        LayoutTop(titleText.rectTransform, -14f, 30f);

        detailText = CreateLabel(panelRoot.transform, string.Empty, 17f, TextAlignmentOptions.TopLeft);
        detailText.color = new Color(0.88f, 0.92f, 0.88f);
        LayoutTop(detailText.rectTransform, -48f, 108f);
    }

    static void LayoutTop(RectTransform rect, float y, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(16f, y);
        rect.sizeDelta = new Vector2(-32f, height);
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
        label.enableWordWrapping = true;
        UiDisplaySettings.ApplySharpText(label, fontSize);
        return label;
    }
}
