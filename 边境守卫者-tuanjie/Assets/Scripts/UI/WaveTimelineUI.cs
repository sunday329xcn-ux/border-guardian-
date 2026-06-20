using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveTimelineUI : MonoBehaviour
{
    const float BarWidth = 420f;
    const float BarHeight = 10f;

    [SerializeField] WaveManager waveManager;

    RectTransform fillRect;
    Image fillImage;
    TextMeshProUGUI progressLabel;
    GameObject barRoot;

    void Start()
    {
        if (waveManager == null)
            waveManager = FindObjectOfType<WaveManager>();

        CreateBar();
        if (waveManager != null)
        {
            waveManager.OnWaveStateChanged += Refresh;
            waveManager.OnWaveSpawnProgressChanged += Refresh;
        }

        Refresh();
    }

    void OnDestroy()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveStateChanged -= Refresh;
            waveManager.OnWaveSpawnProgressChanged -= Refresh;
        }
    }

    void OnEnable()
    {
        if (barRoot != null)
            barRoot.SetActive(true);
    }

    void OnDisable()
    {
        if (barRoot != null)
            barRoot.SetActive(false);
    }

    void Update()
    {
        if (waveManager == null)
            return;

        if (waveManager.State == WaveState.Spawning)
            Refresh();
    }

    void CreateBar()
    {
        var root = new GameObject("WaveTimeline", typeof(RectTransform));
        barRoot = root;
        root.transform.SetParent(transform, false);

        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = new Vector2(0f, -18f);
        rootRect.sizeDelta = new Vector2(BarWidth, BarHeight + 28f);
        UiDisplaySettings.SnapRectToPixels(rootRect);

        progressLabel = CreateLabel(root.transform, "Wave progress", 14f, TextAlignmentOptions.Center);
        var labelRect = progressLabel.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.pivot = new Vector2(0.5f, 1f);
        labelRect.anchoredPosition = new Vector2(0f, -2f);
        labelRect.sizeDelta = new Vector2(0f, 18f);
        progressLabel.color = UiDisplaySettings.BodyText;

        var barObject = new GameObject("Bar", typeof(RectTransform));
        barObject.transform.SetParent(root.transform, false);
        var barRect = barObject.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 0f);
        barRect.anchorMax = new Vector2(1f, 0f);
        barRect.pivot = new Vector2(0.5f, 0f);
        barRect.anchoredPosition = new Vector2(0f, 0f);
        barRect.sizeDelta = new Vector2(0f, BarHeight + 4f);

        var background = barObject.AddComponent<Image>();
        background.color = new Color(0.08f, 0.1f, 0.08f, 0.55f);
        background.raycastTarget = false;

        var fillObject = new GameObject("Fill", typeof(RectTransform));
        fillObject.transform.SetParent(barObject.transform, false);
        fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);

        fillImage = fillObject.AddComponent<Image>();
        UiDisplaySettings.ApplyWhiteSprite(fillImage);
        fillImage.color = new Color(0.55f, 0.75f, 1f, 0.95f);
        fillImage.raycastTarget = false;
    }

    void Refresh()
    {
        if (waveManager == null || fillRect == null || fillImage == null)
            return;

        var ratio = waveManager.State switch
        {
            WaveState.Preparation => 0f,
            WaveState.Spawning => waveManager.WaveSpawnProgress,
            WaveState.Combat => 1f,
            WaveState.Victory => 1f,
            _ => 0f
        };

        var percent = Mathf.RoundToInt(Mathf.Clamp01(ratio) * 100f);
        fillRect.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);
        fillImage.color = waveManager.State switch
        {
            WaveState.Preparation => new Color(0.95f, 0.82f, 0.25f, 0.95f),
            WaveState.Spawning => new Color(0.55f, 0.75f, 1f, 0.95f),
            WaveState.Combat => new Color(0.45f, 0.78f, 0.42f, 0.95f),
            WaveState.Victory => new Color(0.95f, 0.82f, 0.25f, 0.95f),
            _ => fillImage.color
        };

        if (progressLabel != null)
        {
            progressLabel.text = waveManager.State switch
            {
                WaveState.Preparation => "Next wave",
                WaveState.Spawning when waveManager.CurrentWaveTotalEnemies > 0 =>
                    $"Spawning {percent}% ({waveManager.CurrentWaveSpawnedEnemies}/{waveManager.CurrentWaveTotalEnemies})",
                WaveState.Spawning => "Spawning...",
                WaveState.Combat when waveManager.CurrentWaveTotalEnemies > 0 =>
                    $"Wave {waveManager.CurrentWaveNumber}: 100% spawned",
                WaveState.Combat => "In combat",
                WaveState.Victory => "Victory",
                _ => string.Empty
            };
        }
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
}
