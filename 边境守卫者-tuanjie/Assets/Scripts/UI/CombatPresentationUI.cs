using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// P1 combat feedback: elite/boss spawn banner, priority HP bar, tower-under-attack warning.
/// </summary>
public class CombatPresentationUI : MonoBehaviour
{
    const float ScreenPadding = 24f;
    const float SpawnBannerDuration = 2.8f;
    const float TowerWarningDuration = 3.2f;

    GameObject hpBarRoot;
    RectTransform hpFillRect;
    Image hpFillImage;
    TextMeshProUGUI hpNameText;
    TextMeshProUGUI hpValueText;

    GameObject spawnBannerRoot;
    TextMeshProUGUI spawnBannerTitle;
    TextMeshProUGUI spawnBannerSubtitle;

    GameObject towerWarningRoot;
    TextMeshProUGUI towerWarningText;

    EnemyBase trackedEnemy;
    float spawnBannerTimer;
    float towerWarningTimer;

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());

        CreateHpBar();
        CreateSpawnBanner();
        CreateTowerWarning();

        EnemyBase.OnPriorityEnemySpawned += HandlePriorityEnemySpawned;
        EnemyBase.OnEnemyPresentation += HandleEnemyPresentation;
        TowerBase.OnTowerDamaged += HandleTowerDamaged;

        SetHpBarVisible(false);
        SetSpawnBannerVisible(false);
        SetTowerWarningVisible(false);
    }

    void OnDestroy()
    {
        EnemyBase.OnPriorityEnemySpawned -= HandlePriorityEnemySpawned;
        EnemyBase.OnEnemyPresentation -= HandleEnemyPresentation;
        TowerBase.OnTowerDamaged -= HandleTowerDamaged;
    }

    void Update()
    {
        UpdateSpawnBanner();
        UpdateTowerWarning();
        UpdateHpBar();
    }

    void HandlePriorityEnemySpawned(EnemyBase enemy)
    {
        if (enemy == null)
            return;

        var title = enemy.IsBoss
            ? $"BOSS · {EnemyCatalog.GetDisplayName(enemy.EnemyType)}"
            : $"ELITE · {EnemyCatalog.GetDisplayName(enemy.EnemyType)}";

        var subtitle = enemy.IsBoss
            ? "Focus fire before it reaches your core."
            : "High threat target on the field.";

        ShowSpawnBanner(title, subtitle, enemy.IsBoss);
    }

    void HandleEnemyPresentation(EnemyBase enemy, string message)
    {
        if (enemy == null || string.IsNullOrWhiteSpace(message))
            return;

        var prefix = enemy.IsBoss ? "BOSS" : enemy.IsElite ? "ELITE" : "ALERT";
        ShowSpawnBanner($"{prefix} · {message}", string.Empty, enemy.IsBoss);
    }

    void HandleTowerDamaged(TowerBase tower, int damage)
    {
        if (tower == null || damage <= 0)
            return;

        towerWarningTimer = TowerWarningDuration;

        if (towerWarningText != null)
        {
            towerWarningText.text = tower.CurrentTowerHealth > 0
                ? $"Tower under attack! · {tower.GetDisplayName()} ({tower.CurrentTowerHealth}/{tower.MaxTowerHealth} HP)"
                : $"Tower destroyed! · {tower.GetDisplayName()}";
        }

        SetTowerWarningVisible(true);
    }

    void UpdateSpawnBanner()
    {
        if (spawnBannerTimer <= 0f)
            return;

        spawnBannerTimer -= Time.deltaTime;
        if (spawnBannerTimer <= 0f)
            SetSpawnBannerVisible(false);
    }

    void UpdateTowerWarning()
    {
        if (towerWarningTimer <= 0f)
            return;

        towerWarningTimer -= Time.deltaTime;
        if (towerWarningTimer <= 0f)
            SetTowerWarningVisible(false);
    }

    void UpdateHpBar()
    {
        var nextTracked = FindTrackedEnemy();
        if (nextTracked != trackedEnemy)
            trackedEnemy = nextTracked;

        if (trackedEnemy == null || trackedEnemy.IsDead)
        {
            trackedEnemy = null;
            SetHpBarVisible(false);
            return;
        }

        SetHpBarVisible(true);
        spawnBannerTimer = 0f;
        SetSpawnBannerVisible(false);

        var maxHealth = Mathf.Max(1, trackedEnemy.MaxHealth);
        var currentHealth = Mathf.Clamp(trackedEnemy.CurrentHealth, 0, maxHealth);
        var ratio = currentHealth / (float)maxHealth;

        SetHpFillRatio(ratio);

        if (hpNameText != null)
        {
            hpNameText.text = trackedEnemy.IsBoss
                ? $"BOSS · {EnemyCatalog.GetDisplayName(trackedEnemy.EnemyType)}"
                : $"ELITE · {EnemyCatalog.GetDisplayName(trackedEnemy.EnemyType)}";
            hpNameText.color = trackedEnemy.IsBoss
                ? new Color(1f, 0.82f, 0.82f)
                : new Color(1f, 0.92f, 0.75f);
        }

        if (hpValueText != null)
            hpValueText.text = $"{currentHealth} / {maxHealth}";
    }

    void SetHpFillRatio(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);

        if (hpFillRect != null)
        {
            hpFillRect.anchorMin = Vector2.zero;
            hpFillRect.anchorMax = new Vector2(ratio, 1f);
            hpFillRect.offsetMin = Vector2.zero;
            hpFillRect.offsetMax = Vector2.zero;
        }

        if (hpFillImage != null)
        {
            hpFillImage.color = trackedEnemy != null && trackedEnemy.IsBoss
                ? new Color(0.95f, 0.72f, 0.15f, 0.98f)
                : new Color(1f, 0.86f, 0.2f, 0.98f);
        }
    }

    static EnemyBase FindTrackedEnemy()
    {
        EnemyBase best = null;
        var bestScore = int.MinValue;

        foreach (var enemy in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (enemy == null || enemy.IsDead || (!enemy.IsElite && !enemy.IsBoss))
                continue;

            var score = enemy.IsBoss
                ? 1_000_000 + enemy.MaxHealth
                : enemy.MaxHealth;

            if (score <= bestScore)
                continue;

            bestScore = score;
            best = enemy;
        }

        return best;
    }

    void ShowSpawnBanner(string title, string subtitle, bool isBoss)
    {
        spawnBannerTimer = SpawnBannerDuration;

        if (spawnBannerTitle != null)
        {
            spawnBannerTitle.text = title;
            spawnBannerTitle.color = isBoss
                ? new Color(1f, 0.72f, 0.72f)
                : new Color(1f, 0.88f, 0.55f);
        }

        if (spawnBannerSubtitle != null)
        {
            spawnBannerSubtitle.text = subtitle ?? string.Empty;
            spawnBannerSubtitle.gameObject.SetActive(!string.IsNullOrWhiteSpace(subtitle));
        }

        SetSpawnBannerVisible(true);
    }

    void CreateHpBar()
    {
        hpBarRoot = CreateUiObject("PriorityHpBar", transform);
        var rootRect = hpBarRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(520f, 56f);
        rootRect.anchoredPosition = new Vector2(0f, -ScreenPadding - 108f);
        UiDisplaySettings.SnapRectToPixels(rootRect);

        var background = hpBarRoot.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(background, 0.88f);

        hpNameText = CreateLabel(hpBarRoot.transform, "BOSS · Ancient Dragon", 18f, TextAlignmentOptions.TopLeft);
        LayoutTopLine(hpNameText.rectTransform, -10f, 22f);

        hpValueText = CreateLabel(hpBarRoot.transform, "0 / 0", 16f, TextAlignmentOptions.TopRight);
        hpValueText.color = new Color(0.85f, 0.9f, 0.85f);
        LayoutTopLine(hpValueText.rectTransform, -10f, 22f, alignRight: true);

        var barBackground = CreateUiObject("BarBackground", hpBarRoot.transform);
        var barBackgroundRect = barBackground.GetComponent<RectTransform>();
        barBackgroundRect.anchorMin = new Vector2(0f, 0f);
        barBackgroundRect.anchorMax = new Vector2(1f, 0f);
        barBackgroundRect.pivot = new Vector2(0.5f, 0f);
        barBackgroundRect.anchoredPosition = new Vector2(0f, 10f);
        barBackgroundRect.sizeDelta = new Vector2(-24f, 18f);
        UiDisplaySettings.SnapRectToPixels(barBackgroundRect);

        var barBackgroundImage = barBackground.AddComponent<Image>();
        UiDisplaySettings.ApplyWhiteSprite(barBackgroundImage);
        barBackgroundImage.color = new Color(0.96f, 0.96f, 0.94f, 0.98f);
        barBackgroundImage.raycastTarget = false;

        var fillObject = CreateUiObject("BarFill", barBackground.transform);
        hpFillRect = fillObject.GetComponent<RectTransform>();
        hpFillRect.anchorMin = Vector2.zero;
        hpFillRect.anchorMax = Vector2.one;
        hpFillRect.offsetMin = Vector2.zero;
        hpFillRect.offsetMax = Vector2.zero;

        hpFillImage = fillObject.AddComponent<Image>();
        UiDisplaySettings.ApplyWhiteSprite(hpFillImage);
        hpFillImage.color = new Color(1f, 0.86f, 0.2f, 0.98f);
        hpFillImage.raycastTarget = false;
    }

    void CreateSpawnBanner()
    {
        spawnBannerRoot = CreateUiObject("SpawnBanner", transform);
        var rootRect = spawnBannerRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(560f, 72f);
        rootRect.anchoredPosition = new Vector2(0f, -ScreenPadding - 184f);
        UiDisplaySettings.SnapRectToPixels(rootRect);

        var background = spawnBannerRoot.AddComponent<Image>();
        background.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);
        background.raycastTarget = false;

        spawnBannerTitle = CreateLabel(spawnBannerRoot.transform, "ELITE · Rock Golem", 24f, TextAlignmentOptions.Center);
        LayoutTopLine(spawnBannerTitle.rectTransform, -10f, 30f, stretchHorizontal: true);

        spawnBannerSubtitle = CreateLabel(spawnBannerRoot.transform, string.Empty, 15f, TextAlignmentOptions.Center);
        spawnBannerSubtitle.color = new Color(0.78f, 0.82f, 0.78f);
        LayoutTopLine(spawnBannerSubtitle.rectTransform, -40f, 22f, stretchHorizontal: true);
    }

    void CreateTowerWarning()
    {
        towerWarningRoot = CreateUiObject("TowerWarning", transform);
        var rootRect = towerWarningRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(620f, 42f);
        rootRect.anchoredPosition = new Vector2(0f, -ScreenPadding - 52f);
        UiDisplaySettings.SnapRectToPixels(rootRect);

        var background = towerWarningRoot.AddComponent<Image>();
        background.color = new Color(0.45f, 0.08f, 0.08f, 0.88f);
        background.raycastTarget = false;

        towerWarningText = CreateLabel(towerWarningRoot.transform, "Tower under attack!", 18f, TextAlignmentOptions.Center);
        Stretch(towerWarningText.rectTransform, 8f);
        towerWarningText.color = new Color(1f, 0.92f, 0.92f);
    }

    void SetHpBarVisible(bool visible)
    {
        if (hpBarRoot != null)
            hpBarRoot.SetActive(visible);
    }

    void SetSpawnBannerVisible(bool visible)
    {
        if (spawnBannerRoot != null)
            spawnBannerRoot.SetActive(visible);
    }

    void SetTowerWarningVisible(bool visible)
    {
        if (towerWarningRoot != null)
            towerWarningRoot.SetActive(visible);
    }

    static void LayoutTopLine(RectTransform rect, float y, float height, bool alignRight = false, bool stretchHorizontal = false)
    {
        if (stretchHorizontal)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(-24f, height);
            return;
        }

        rect.anchorMin = new Vector2(alignRight ? 1f : 0f, 1f);
        rect.anchorMax = new Vector2(alignRight ? 1f : 0f, 1f);
        rect.pivot = new Vector2(alignRight ? 1f : 0f, 1f);
        rect.anchoredPosition = new Vector2(alignRight ? -12f : 12f, y);
        rect.sizeDelta = new Vector2(220f, height);
    }

    static void Stretch(RectTransform rect, float padding)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(padding, padding);
        rect.offsetMax = new Vector2(-padding, -padding);
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
