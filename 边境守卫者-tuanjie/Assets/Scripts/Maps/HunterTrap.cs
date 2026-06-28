using UnityEngine;

public class HunterTrap : MonoBehaviour
{
    const int TrapDamage = 100;
    const float RootDuration = 5f;
    const float TriggerRadius = 0.55f;

    bool consumed;
    SpriteRenderer spriteRenderer;
    System.Action onTriggered;

    public bool IsActive => !consumed;
    public Vector2Int GridCell { get; private set; }

    public static HunterTrap Create(Transform parent, Vector2Int gridCell, System.Action triggeredCallback)
    {
        var trapObject = new GameObject($"HunterTrap_{gridCell.x}_{gridCell.y}");
        trapObject.transform.SetParent(parent, false);

        var trap = trapObject.AddComponent<HunterTrap>();
        trap.onTriggered = triggeredCallback;
        trap.GridCell = gridCell;
        trap.Initialize(MapGridSettings.GridToWorld(gridCell.x, gridCell.y));
        return trap;
    }

    void Initialize(Vector3 position)
    {
        transform.position = position;
        transform.localScale = Vector3.one * 0.55f;

        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = MapGridControllerShared.GetWhiteSprite();
        spriteRenderer.color = new Color(0.85f, 0.55f, 0.15f, 0.85f);
        spriteRenderer.sortingOrder = 3;
        EnvironmentVisual.DecorateTrap(transform);
    }

    void Update()
    {
        if (consumed)
            return;

        foreach (var enemy in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            if (Vector2.Distance(enemy.transform.position, transform.position) > TriggerRadius)
                continue;

            TriggerOn(enemy);
            return;
        }
    }

    void TriggerOn(EnemyBase enemy)
    {
        if (consumed)
            return;

        consumed = true;
        enemy.TakeDamage(TrapDamage, DamageType.True);
        enemy.ApplyRoot(RootDuration);
        EnvironmentVisual.PlayTrapTrigger(transform.position);
        onTriggered?.Invoke();
        Destroy(gameObject);
    }
}
