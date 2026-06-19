using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Preparation-phase spawn lane intel buttons at upper/lower entry points.
/// </summary>
public class WaveSpawnHintUI : MonoBehaviour
{
    const float ButtonSize = 44f;

    static readonly Vector2Int[] SpawnCells =
    {
        GrimmForestMapLayout.UpperSpawnCell,
        GrimmForestMapLayout.LowerSpawnCell
    };

    static readonly Vector3[] SpawnOffsets =
    {
        new Vector3(0.85f, 0.15f, 0f),
        new Vector3(0.85f, -0.15f, 0f)
    };

    WaveManager waveManager;
    Canvas canvas;
    RectTransform canvasRect;
    Camera worldCamera;

    readonly LaneWidget[] lanes = new LaneWidget[2];
    int openLaneIndex = -1;

    struct LaneWidget
    {
        public GameObject buttonRoot;
        public Button button;
        public GameObject popupRoot;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI bodyText;
    }

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas != null ? canvas.transform as RectTransform : null;
        worldCamera = Camera.main;
        waveManager = FindObjectOfType<WaveManager>();

        var root = CreateUiObject("WaveSpawnHints", transform);
        Stretch(root.GetComponent<RectTransform>());

        for (int i = 0; i < lanes.Length; i++)
            lanes[i] = CreateLaneWidget(root.transform, i);

        if (waveManager != null)
            waveManager.OnWaveStateChanged += Refresh;

        Refresh();
    }

    void OnDestroy()
    {
        if (waveManager != null)
            waveManager.OnWaveStateChanged -= Refresh;
    }

    void Update()
    {
        if (waveManager == null || waveManager.State != WaveState.Preparation)
            return;

        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i].buttonRoot == null || !lanes[i].buttonRoot.activeSelf)
                continue;

            PositionAtWorld(lanes[i].buttonRoot.transform as RectTransform, GetSpawnWorldPosition(i));
            if (openLaneIndex == i && lanes[i].popupRoot != null && lanes[i].popupRoot.activeSelf)
                PositionPopup(lanes[i]);
        }
    }

    void Refresh()
    {
        if (waveManager == null)
            return;

        var showHints = waveManager.State == WaveState.Preparation
                        && waveManager.UpcomingWaveDefinition != null
                        && (GamePauseController.Instance == null || !GamePauseController.Instance.IsPaused);

        if (!showHints)
        {
            ClosePopup();
            for (int i = 0; i < lanes.Length; i++)
            {
                if (lanes[i].buttonRoot != null)
                    lanes[i].buttonRoot.SetActive(false);
            }

            return;
        }

        for (int i = 0; i < lanes.Length; i++)
        {
            var laneActive = waveManager.WillUpcomingWaveUseSpawnLane(i);
            if (lanes[i].buttonRoot != null)
                lanes[i].buttonRoot.SetActive(laneActive);

            if (!laneActive && openLaneIndex == i)
                ClosePopup();
        }
    }

    void ToggleLanePopup(int laneIndex)
    {
        if (openLaneIndex == laneIndex)
        {
            ClosePopup();
            return;
        }

        ClosePopup();
        openLaneIndex = laneIndex;

        ref var lane = ref lanes[laneIndex];
        if (lane.popupRoot == null || waveManager == null)
            return;

        lane.titleText.text =
            $"{WaveManager.GetSpawnLaneDisplayName(laneIndex)} · Wave {waveManager.CurrentWaveNumber}";

        var enemySummary = waveManager.GetUpcomingEnemySummaryForSpawnLane(laneIndex);
        var count = waveManager.GetUpcomingEnemyCountForSpawnLane(laneIndex);
        var hint = waveManager.GetUpcomingHint();
        lane.bodyText.text = string.IsNullOrWhiteSpace(hint)
            ? $"Incoming ({count}):\n{enemySummary}"
            : $"Incoming ({count}):\n{enemySummary}\n\n{hint}";

        lane.popupRoot.SetActive(true);
        PositionPopup(lane);
    }

    void ClosePopup()
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i].popupRoot != null)
                lanes[i].popupRoot.SetActive(false);
        }

        openLaneIndex = -1;
    }

    void PositionPopup(LaneWidget lane)
    {
        var buttonRect = lane.buttonRoot.transform as RectTransform;
        var popupRect = lane.popupRoot.transform as RectTransform;
        if (buttonRect == null || popupRect == null)
            return;

        popupRect.anchoredPosition = buttonRect.anchoredPosition + new Vector2(ButtonSize * 0.5f + 8f, 72f);
    }

    Vector3 GetSpawnWorldPosition(int laneIndex)
    {
        var cell = SpawnCells[laneIndex];
        return MapGridSettings.GridToWorld(cell.x, cell.y) + SpawnOffsets[laneIndex];
    }

    void PositionAtWorld(RectTransform rect, Vector3 worldPosition)
    {
        if (rect == null || canvasRect == null || worldCamera == null)
            return;

        var screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPosition);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out var localPoint))
        {
            rect.anchoredPosition = localPoint;
        }
    }

    LaneWidget CreateLaneWidget(Transform parent, int laneIndex)
    {
        var lane = new LaneWidget();

        lane.buttonRoot = CreateUiObject($"SpawnHintButton_{laneIndex}", parent);
        var buttonRect = lane.buttonRoot.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(ButtonSize, ButtonSize);

        var buttonImage = lane.buttonRoot.AddComponent<Image>();
        buttonImage.color = new Color(0.92f, 0.35f, 0.28f, 0.94f);

        lane.button = lane.buttonRoot.AddComponent<Button>();
        lane.button.targetGraphic = buttonImage;
        var capturedLane = laneIndex;
        lane.button.onClick.AddListener(() => ToggleLanePopup(capturedLane));

        var label = CreateLabel(lane.buttonRoot.transform, "?", 24f, TextAlignmentOptions.Center);
        Stretch(label.rectTransform);
        label.color = Color.white;

        lane.popupRoot = CreateUiObject($"SpawnHintPopup_{laneIndex}", parent);
        var popupRect = lane.popupRoot.GetComponent<RectTransform>();
        popupRect.sizeDelta = new Vector2(260f, 168f);
        popupRect.pivot = new Vector2(0f, 0f);

        var popupBackground = lane.popupRoot.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(popupBackground, 0.96f);

        lane.titleText = CreateLabel(lane.popupRoot.transform, "Upper Route", 17f, TextAlignmentOptions.TopLeft);
        LayoutPopupLine(lane.titleText.rectTransform, -10f, 24f);

        lane.bodyText = CreateLabel(lane.popupRoot.transform, string.Empty, 15f, TextAlignmentOptions.TopLeft);
        lane.bodyText.color = new Color(0.88f, 0.92f, 0.88f);
        lane.bodyText.enableWordWrapping = true;
        LayoutPopupLine(lane.bodyText.rectTransform, -38f, 118f);

        lane.popupRoot.SetActive(false);
        lane.buttonRoot.SetActive(false);
        return lane;
    }

    static void LayoutPopupLine(RectTransform rect, float y, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(14f, y);
        rect.sizeDelta = new Vector2(-28f, height);
    }

    static void Stretch(RectTransform rect)
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
