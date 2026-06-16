using UnityEngine;

public class RootEntangleZone : MonoBehaviour
{
    float radius;
    float slowPercent;
    float expireTime;

    public static void Spawn(Vector3 center, float radius, float slowPercent, float duration)
    {
        var go = new GameObject("RootEntangleZone");
        go.transform.position = center;
        go.transform.localScale = Vector3.one * radius * 2f;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = MapGridControllerShared.GetWhiteSprite();
        renderer.color = new Color(0.25f, 0.55f, 0.2f, 0.35f);
        renderer.sortingOrder = 2;

        var zone = go.AddComponent<RootEntangleZone>();
        zone.radius = radius;
        zone.slowPercent = slowPercent;
        zone.expireTime = Time.time + duration;
    }

    void Update()
    {
        if (Time.time >= expireTime)
        {
            Destroy(gameObject);
            return;
        }

        foreach (var enemy in EnemyRegistry.ActiveEnemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            if (Vector2.Distance(enemy.transform.position, transform.position) > radius)
                continue;

            var slow = enemy.GetComponent<EnemySlowEffect>();
            if (slow != null)
                slow.ApplySlow(slowPercent, 0.35f);
        }
    }
}
