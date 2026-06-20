using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hover tooltip for build platform terrain (no click popup).
/// </summary>
public class PlatformTerrainInfoUI : MonoBehaviour
{
    const float PanelWidth = 300f;
    const float PanelHeight = 132f;
    const float ScreenPadding = 12f;
    const float BuildBarClearance = 118f;
    const float PickRadius = 0.55f;

    MapGridController mapGridController;
    Camera mainCamera;
    Canvas rootCanvas;
    GameObject panelRoot;
    RectTransform panelRect;
    TextMeshProUGUI titleText;
    TextMeshProUGUI detailText;
    BuildSlot hoveredSlot;

    void Start()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        UiDisplaySettings.ConfigureCanvas(rootCanvas);
        mapGridController = FindObjectOfType<MapGridController>();
        mainCamera = Camera.main;
        CreatePanel();
        HidePanel();
    }

    void Update()
    {
        if (!CanShowTooltip())
        {
            HidePanel();
            return;
        }

        if (UiInputUtility.IsPointerOverUi())
        {
            HidePanel();
            return;
        }

        var slot = FindHoveredBuildSlot();
        if (slot == null)
        {
            HidePanel();
            return;
        }

        ShowForSlot(slot);
        FollowPointer();
    }

    bool CanShowTooltip()
    {
        if (!MainMenuUI.IsSessionStarted)
            return false;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return false;

        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return false;

        if (TowerBuildDragHandler.IsDragging)
            return false;

        return mapGridController != null && mainCamera != null && panelRoot != null;
    }

    BuildSlot FindHoveredBuildSlot()
    {
        return BuildSlotPlacementUtility.FindBuildSlotAt(
            mapGridController,
            GetMouseWorldPosition(),
            PickRadius);
    }

    Vector3 GetMouseWorldPosition()
    {
        var screenPoint = Input.mousePosition;
        screenPoint.z = Mathf.Abs(mainCamera.transform.position.z);
        var worldPoint = mainCamera.ScreenToWorldPoint(screenPoint);
        worldPoint.z = 0f;
        return worldPoint;
    }

    void ShowForSlot(BuildSlot slot)
    {
        if (slot == null)
            return;

        hoveredSlot = slot;
        panelRoot.SetActive(true);

        var terrain = slot.TerrainType;
        titleText.text = PlatformTerrainCatalog.GetDisplayName(terrain);
        detailText.text =
            $"{PlatformTerrainCatalog.GetSubtitle(terrain)}\n" +
            PlatformTerrainCatalog.GetHoverSummary(terrain);

        if (!slot.CanAcceptBuild())
            detailText.text += "\nOccupied.";
    }

    void HidePanel()
    {
        hoveredSlot = null;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    void FollowPointer()
    {
        if (panelRect == null || rootCanvas == null)
            return;

        var canvasRect = rootCanvas.transform as RectTransform;
        if (canvasRect == null)
            return;

        var screenPoint = (Vector2)Input.mousePosition + new Vector2(18f, 18f);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
                out var localPoint))
        {
            panelRect.anchoredPosition = ClampToCanvas(localPoint, canvasRect);
        }
    }

    static Vector2 ClampToCanvas(Vector2 localPoint, RectTransform canvasRect)
    {
        var halfWidth = PanelWidth * 0.5f;
        var halfHeight = PanelHeight * 0.5f;
        var maxX = canvasRect.rect.width * 0.5f - halfWidth - ScreenPadding;
        var minX = -canvasRect.rect.width * 0.5f + halfWidth + ScreenPadding;
        var maxY = canvasRect.rect.height * 0.5f - halfHeight - ScreenPadding;
        var minY = -canvasRect.rect.height * 0.5f + halfHeight + ScreenPadding + BuildBarClearance;

        return new Vector2(
            Mathf.Clamp(localPoint.x, minX, maxX),
            Mathf.Clamp(localPoint.y, minY, maxY));
    }

    void CreatePanel()
    {
        panelRoot = new GameObject("PlatformTerrainTooltip", typeof(RectTransform), typeof(Image));
        panelRoot.transform.SetParent(transform, false);

        panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var background = panelRoot.GetComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(background, 0.94f);
        background.raycastTarget = false;

        titleText = CreateLabel(panelRoot.transform, "Platform", 20f, TextAlignmentOptions.TopLeft);
        LayoutTop(titleText.rectTransform, -12f, 28f);

        detailText = CreateLabel(panelRoot.transform, string.Empty, 15f, TextAlignmentOptions.TopLeft);
        detailText.color = new Color(0.88f, 0.92f, 0.88f);
        detailText.lineSpacing = -2f;
        LayoutTop(detailText.rectTransform, -42f, 82f);
    }

    static void LayoutTop(RectTransform rect, float y, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(14f, y);
        rect.sizeDelta = new Vector2(-28f, height);
    }

    static TextMeshProUGUI CreateLabel(Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
    {
        var labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(parent, false);

        var label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.alignment = alignment;
        label.color = Color.white;
        label.raycastTarget = false;
        label.enableWordWrapping = true;
        UiDisplaySettings.ApplySharpText(label, fontSize);
        return label;
    }
}
