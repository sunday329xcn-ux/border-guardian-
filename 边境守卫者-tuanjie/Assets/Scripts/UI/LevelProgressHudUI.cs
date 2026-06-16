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
        rect.anchoredPosition = new Vector2(28f, -152f);
        rect.sizeDelta = new Vector2(320f, 32f);
        UiDisplaySettings.SnapRectToPixels(rect);

        progressText = labelObject.AddComponent<TextMeshProUGUI>();
        progressText.alignment = TextAlignmentOptions.TopLeft;
        progressText.color = new Color(0.95f, 0.9f, 0.65f);
        progressText.raycastTarget = false;
        UiDisplaySettings.ApplySharpText(progressText, 22f);
    }

    void Refresh()
    {
        if (progressText == null)
            return;

        var bestStars = LevelProgressService.GetBestStars(currentLevel);
        var starText = bestStars > 0
            ? LevelStarRating.BuildStarText(bestStars)
            : "☆☆☆";

        progressText.text = $"Keys: {LevelProgressService.TotalKeys}  ·  Best {starText}";
    }
}
