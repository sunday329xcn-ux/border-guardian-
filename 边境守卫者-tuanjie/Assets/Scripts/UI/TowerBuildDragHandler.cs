using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerBuildDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    const float DragThreshold = 10f;

    [SerializeField] TowerType towerType;
    [SerializeField] TowerBuildSelector buildSelector;

    MapGridController mapGridController;
    Canvas rootCanvas;
    RectTransform dragGhostRect;
    TextMeshProUGUI dragGhostLabel;
    Vector2 dragStartScreenPos;
    bool dragActive;
    bool suppressClick;

    public static bool IsDragging { get; private set; }
    public static TowerType DraggingType { get; private set; }

    public void Initialize(TowerType type, TowerBuildSelector selector)
    {
        towerType = type;
        buildSelector = selector;
    }

    void Awake()
    {
        if (buildSelector == null)
            buildSelector = FindObjectOfType<TowerBuildSelector>();

        mapGridController = FindObjectOfType<MapGridController>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        if (!dragActive || !Input.GetKeyDown(KeyCode.Escape))
            return;

        CancelDrag();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanInteract())
            return;

        dragActive = false;
        suppressClick = false;
        dragStartScreenPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanInteract())
            return;

        if (!dragActive)
        {
            if (Vector2.Distance(eventData.position, dragStartScreenPos) < DragThreshold)
                return;

            dragActive = true;
            suppressClick = true;
            IsDragging = true;
            DraggingType = towerType;
            buildSelector?.Select(towerType);
            EnsureDragGhost(eventData);
        }

        UpdateDragGhostPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragActive)
            return;

        var slot = BuildSlotPlacementUtility.FindBuildSlotAtScreen(
            mapGridController,
            Camera.main,
            eventData.position);

        if (slot != null)
            TowerBuildService.TryBuild(towerType, slot);

        FinishDrag();
    }

    public bool ConsumeClickSuppression()
    {
        if (!suppressClick)
            return false;

        suppressClick = false;
        return true;
    }

    void CancelDrag()
    {
        if (!dragActive)
            return;

        FinishDrag();
    }

    void FinishDrag()
    {
        dragActive = false;
        IsDragging = false;
        DraggingType = default;

        if (dragGhostRect != null)
            dragGhostRect.gameObject.SetActive(false);
    }

    bool CanInteract()
    {
        if (!MainMenuUI.IsSessionStarted)
            return false;

        if (!TowerBuildCatalog.IsImplemented(towerType))
            return false;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return false;

        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return false;

        return true;
    }

    void EnsureDragGhost(PointerEventData eventData)
    {
        if (rootCanvas == null)
            return;

        if (dragGhostRect == null)
        {
            var ghostObject = new GameObject("TowerDragGhost", typeof(RectTransform), typeof(CanvasGroup));
            ghostObject.transform.SetParent(rootCanvas.transform, false);
            ghostObject.transform.SetAsLastSibling();

            dragGhostRect = ghostObject.GetComponent<RectTransform>();
            dragGhostRect.sizeDelta = new Vector2(104f, 72f);

            var background = ghostObject.AddComponent<Image>();
            UiDisplaySettings.ApplyBuildButton(background, selected: true);
            background.raycastTarget = false;

            var labelObject = new GameObject("Label", typeof(RectTransform));
            labelObject.transform.SetParent(ghostObject.transform, false);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(6f, 4f);
            labelRect.offsetMax = new Vector2(-6f, -4f);

            dragGhostLabel = labelObject.AddComponent<TextMeshProUGUI>();
            dragGhostLabel.alignment = TextAlignmentOptions.Center;
            dragGhostLabel.raycastTarget = false;

            var group = ghostObject.GetComponent<CanvasGroup>();
            group.alpha = 0.82f;
            group.blocksRaycasts = false;
        }

        var cost = TowerBuildCatalog.GetBuildCost(towerType);
        var name = TowerBuildCatalog.GetDisplayName(towerType);
        dragGhostLabel.text = $"{name}\n{cost}g";
        UiDisplaySettings.ApplyButtonText(dragGhostLabel, UiDisplaySettings.FontSizeBody);

        dragGhostRect.gameObject.SetActive(true);
        UpdateDragGhostPosition(eventData);
    }

    void UpdateDragGhostPosition(PointerEventData eventData)
    {
        if (dragGhostRect == null || rootCanvas == null)
            return;

        var canvasRect = rootCanvas.transform as RectTransform;
        if (canvasRect == null)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                eventData.position,
                rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
                out var localPoint))
        {
            dragGhostRect.anchoredPosition = localPoint;
        }
    }
}
