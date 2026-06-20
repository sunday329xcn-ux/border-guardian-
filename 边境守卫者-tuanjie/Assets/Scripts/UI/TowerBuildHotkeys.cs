using UnityEngine;

public class TowerBuildHotkeys : MonoBehaviour
{
    [SerializeField] TowerBuildSelector buildSelector;

    static readonly TowerType[] HotkeyOrder =
    {
        TowerType.Arrow,
        TowerType.Frost,
        TowerType.Cannon,
        TowerType.Arcane,
        TowerType.Barracks,
        TowerType.DiamondMine
    };

    void Start()
    {
        if (buildSelector == null)
            buildSelector = FindObjectOfType<TowerBuildSelector>();
    }

    void Update()
    {
        if (buildSelector == null)
            return;

        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return;

        for (var i = 0; i < HotkeyOrder.Length; i++)
        {
            if (!Input.GetKeyDown(KeyCode.Alpha1 + i))
                continue;

            var towerType = HotkeyOrder[i];
            buildSelector.Select(towerType);

            if (BuildSlotSelectionController.Selected != null)
                TowerBuildService.TryBuild(towerType, BuildSlotSelectionController.Selected);

            break;
        }
    }
}
