using UnityEngine;

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
        if (TowerBuildDragHandler.IsDragging)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return;

        if (ignoreUiClicks && UiInputUtility.IsPointerOverUi())
            return;

        if (mainCamera == null || mapGridController == null)
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
                BuildSlotSelectionController.Deselect();
                TowerSelectionController.Select(tower);
                return;
            }

            var enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null && !enemy.IsDead)
            {
                EnemySelectionController.Select(enemy);
                return;
            }
        }

        foreach (var hit in hits)
        {
            if (hit == null)
                continue;

            var slot = hit.GetComponent<BuildSlot>();
            if (slot == null || !slot.CanAcceptBuild())
                continue;

            BuildSlotSelectionController.Select(slot);
            EnemySelectionController.Deselect();
            return;
        }

        var fallbackSlot = BuildSlotPlacementUtility.FindBuildSlotAt(mapGridController, worldPoint, clickRadius);
        if (fallbackSlot != null && fallbackSlot.CanAcceptBuild())
        {
            BuildSlotSelectionController.Select(fallbackSlot);
            EnemySelectionController.Deselect();
            return;
        }

        ClearWorldSelection();
    }

    void ClearWorldSelection()
    {
        BuildSlotSelectionController.Deselect();
        TowerSelectionController.Deselect();
        EnemySelectionController.Deselect();

        if (buildSelector != null)
            buildSelector.ClearBuildSelection();
    }

    Vector3 GetMouseWorldPosition()
    {
        var screenPoint = Input.mousePosition;
        screenPoint.z = Mathf.Abs(mainCamera.transform.position.z);
        var worldPoint = mainCamera.ScreenToWorldPoint(screenPoint);
        worldPoint.z = 0f;
        return worldPoint;
    }
}
