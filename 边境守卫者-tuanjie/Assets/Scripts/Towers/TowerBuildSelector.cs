using System;
using UnityEngine;

public class TowerBuildSelector : MonoBehaviour
{
    [SerializeField] TowerType selectedType = TowerType.Arrow;
    bool buildBarActive = true;

    public TowerType SelectedType => selectedType;
    public bool IsBuildBarActive => buildBarActive;
    public event Action<TowerType> OnSelectionChanged;

    public void Select(TowerType type)
    {
        if (!TowerBuildCatalog.IsImplemented(type))
            return;

        selectedType = type;
        buildBarActive = true;
        OnSelectionChanged?.Invoke(selectedType);
    }

    public void ClearBuildSelection()
    {
        if (!buildBarActive)
            return;

        buildBarActive = false;
        OnSelectionChanged?.Invoke(selectedType);
    }
}
