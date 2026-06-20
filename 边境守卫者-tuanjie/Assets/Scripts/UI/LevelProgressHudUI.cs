using TMPro;
using UnityEngine;

/// <summary>
/// Shows persistent keys and current level best stars in the HUD.
/// </summary>
public class LevelProgressHudUI : MonoBehaviour
{
    [SerializeField] LevelId currentLevel = LevelId.GrimmForest;

    TextMeshProUGUI progressText;

    void Start()
    {
        var waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
            currentLevel = waveManager.CurrentLevelId;

        CreateProgressLine();
        LevelProgressService.OnProgressChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        LevelProgressService.OnProgressChanged -= Refresh;
    }

    void CreateProgressLine()
    {
        var labelObject = new GameObject("LevelProgressLine", typeof(RectTransform));
        labelObject.transform.SetParent(transform, false);

        var rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(
            UiDisplaySettings.HudLeftInset,
            -(UiDisplaySettings.HudTopPadding + UiDisplaySettings.HudLineHeight * 3f + 10f));
        rect.sizeDelta = new Vector2(320f, 32f);
        UiDisplaySettings.SnapRectToPixels(rect);

        progressText = labelObject.AddComponent<TextMeshProUGUI>();
        progressText.alignment = TextAlignmentOptions.TopLeft;
        progressText.margin = new Vector4(4f, 0f, 8f, 0f);
        progressText.color = UiDisplaySettings.AccentText;
        progressText.raycastTarget = false;
        UiDisplaySettings.ApplySharpText(progressText, UiDisplaySettings.FontSizeBody);
    }

    void Refresh()
    {
        if (progressText == null)
            return;

        var bestStars = LevelProgressService.GetBestStars(currentLevel);
        var starText = bestStars > 0
            ? LevelStarRating.BuildStarText(bestStars)
            : "---";

        progressText.text = $"Keys: {LevelProgressService.TotalKeys}  ·  Best {starText}";
    }
}
