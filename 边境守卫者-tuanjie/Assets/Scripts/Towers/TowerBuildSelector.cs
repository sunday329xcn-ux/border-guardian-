using System;
using UnityEngine;

public class TowerBuildSelector : MonoBehaviour
{
    [SerializeField] TowerType selectedType = TowerType.Arrow;

    public TowerType SelectedType => selectedType;
    public event Action<TowerType> OnSelectionChanged;

    public void Select(TowerType type)
    {
        if (!TowerBuildCatalog.IsImplemented(type))
            return;

        selectedType = type;
        OnSelectionChanged?.Invoke(selectedType);
    }
}
