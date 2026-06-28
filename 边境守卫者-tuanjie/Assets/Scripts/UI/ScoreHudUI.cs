using TMPro;
using UnityEngine;

/// <summary>
/// In-game score readout (P4.3), shown top-centre just below the weather toggle.
/// Top-centre is otherwise free during gameplay, so this respects the no-overlap
/// rule. Mainly meaningful in Endless, but the live score / best is shown always.
/// </summary>
public class ScoreHudUI : MonoBehaviour
{
    WaveManager waveManager;
    TextMeshProUGUI label;
    float nextRefresh;

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());
        CreatePanel();
    }

    void CreatePanel()
    {
        var panel = new GameObject("ScoreHud", typeof(RectTransform));
        panel.transform.SetParent(transform, false);

        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(220f, 24f);
        // Stack below the 34px weather toggle (which sits at -ScreenPadding).
        rect.anchoredPosition = new Vector2(0f, -(UiDisplaySettings.ScreenPadding + 40f));
        UiDisplaySettings.SnapRectToPixels(rect);

        label = panel.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.95f, 0.9f, 0.65f);
        label.raycastTarget = false;
        UiDisplaySettings.ApplySharpText(label, 14f);
    }

    void Update()
    {
        if (Time.unscaledTime < nextRefresh)
            return;
        nextRefresh = Time.unscaledTime + 0.25f;

        if (waveManager == null)
            waveManager = FindObjectOfType<WaveManager>();

        if (waveManager == null || label == null)
            return;

        var best = LeaderboardService.GetBest(GameModeService.Mode);
        label.text = $"Score {waveManager.CurrentRunScore} · Best {best}";
    }
}
