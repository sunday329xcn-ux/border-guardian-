using UnityEngine;

public static class TowerVisualFactory
{
    public static GameObject CreateTowerObject(string objectName, BuildSlot slot, Color color)
    {
        var towerObject = new GameObject(objectName);
        towerObject.transform.position = slot.transform.position;

        var renderer = towerObject.AddComponent<SpriteRenderer>();
        renderer.sprite = MapGridControllerShared.GetWhiteSprite();
        renderer.color = color;
        renderer.sortingOrder = 4;
        return towerObject;
    }

    public static bool TryPayAndOccupy(BuildSlot slot, int goldCost)
    {
        if (slot == null || slot.IsOccupied)
            return false;

        if (GameManager.Instance == null)
            return false;

        if (!GameManager.Instance.TrySpendGold(goldCost))
            return false;

        if (slot.TryOccupy())
            return true;

        GameManager.Instance.AddGold(goldCost);
        return false;
    }
}
