using UnityEngine;
using UnityEngine.EventSystems;

public class TowerBuildController : MonoBehaviour
{
    [SerializeField] MapGridController mapGridController;
    [SerializeField] TowerBuildSelector buildSelector;
    [SerializeField] float clickRadius = 0.55f;
    [SerializeField] bool ignoreUiClicks = true;

    Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (mapGridController == null)
            mapGridController = FindObjectOfType<MapGridController>();

        if (buildSelector == null)
            buildSelector = FindObjectOfType<TowerBuildSelector>();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return;

        if (ignoreUiClicks && IsPointerOverUi())
            return;

        if (mainCamera == null || mapGridController == null || buildSelector == null)
            return;

        var worldPoint = GetMouseWorldPosition();

        if (TryHandleRallyPlacement(worldPoint))
            return;

        if (TryHandleEnvironmentClick(worldPoint))
            return;

        HandleWorldClick(worldPoint);
    }

    bool TryHandleEnvironmentClick(Vector3 worldPoint)
    {
        if (mapGridController?.Environment == null)
            return false;

        return mapGridController.Environment.TryHandleClick(worldPoint);
    }

    bool TryHandleRallyPlacement(Vector3 worldPoint)
    {
        if (TowerSelectionController.Selected is not BarracksTower barracks || !barracks.IsPlacingRally)
            return false;

        barracks.TrySetRallyPoint(worldPoint);
        return true;
    }

    void HandleWorldClick(Vector3 worldPoint)
    {
        var hits = Physics2D.OverlapPointAll(worldPoint);

        foreach (var hit in hits)
        {
            if (hit == null)
                continue;

            var tower = hit.GetComponent<TowerBase>();
            if (tower != null)
            {
                TowerSelectionController.Select(tower);
                return;
            }
        }

        foreach (var hit in hits)
        {
            if (hit == null)
                continue;

            var slot = hit.GetComponent<BuildSlot>();
            if (slot == null || slot.IsOccupied)
                continue;

            if (TowerFactory.Build(buildSelector.SelectedType, slot) != null)
            {
                TowerSelectionController.Deselect();
                return;
            }
        }

        var fallbackSlot = FindBuildSlotAt(worldPoint);
        if (fallbackSlot != null)
        {
            if (TowerFactory.Build(buildSelector.SelectedType, fallbackSlot) != null)
                TowerSelectionController.Deselect();

            return;
        }

        TowerSelectionController.Deselect();
    }

    static bool IsPointerOverUi()
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject();
    }

    Vector3 GetMouseWorldPosition()
    {
        var screenPoint = Input.mousePosition;
        screenPoint.z = Mathf.Abs(mainCamera.transform.position.z);
        var worldPoint = mainCamera.ScreenToWorldPoint(screenPoint);
        worldPoint.z = 0f;
        return worldPoint;
    }

    BuildSlot FindBuildSlotAt(Vector3 worldPoint)
    {
        BuildSlot closestSlot = null;
        var closestDistanceSqr = clickRadius * clickRadius;

        foreach (var slot in mapGridController.BuildSlots)
        {
            if (slot == null || slot.IsOccupied)
                continue;

            var offset = slot.transform.position - worldPoint;
            offset.z = 0f;
            var distanceSqr = offset.sqrMagnitude;
            if (distanceSqr > closestDistanceSqr)
                continue;

            closestDistanceSqr = distanceSqr;
            closestSlot = slot;
        }

        return closestSlot;
    }
}
