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

    public static bool TryPayForBuild(BuildSlot slot, int goldCost)
    {
        if (slot == null || !slot.CanAcceptBuild())
            return false;

        if (GameManager.Instance == null)
            return false;

        return GameManager.Instance.TrySpendGold(goldCost);
    }

    public static void RefundBuild(BuildSlot slot, int goldCost)
    {
        if (GameManager.Instance == null || goldCost <= 0)
            return;

        GameManager.Instance.AddGold(goldCost);
    }
}
