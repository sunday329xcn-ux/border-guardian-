using UnityEngine;
using UnityEngine.EventSystems;

public class TowerBuildButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    TowerType towerType;

    public void Initialize(TowerType type)
    {
        towerType = type;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TowerBuildBarHoverController.SetHovered(towerType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TowerBuildBarHoverController.ClearIf(towerType);
    }
}
