using System.Collections.Generic;
using UnityEngine;

public class TowerGroundZone : MonoBehaviour
{
    static readonly List<TowerGroundZone> ActiveZones = new();

    float radius;
    float duration;
    float slowPercent;
    float damagePerSecond;
    float expireTime;
    bool appliesSlow;
    bool appliesDamage;

    public static void Spawn(Vector3 position, float radius, float duration, float slowPercent)
    {
        SpawnEffect(position, radius, duration, slowPercent, 0f, true, false);
    }

    public static void SpawnFire(Vector3 position, float radius, float duration, float damagePerSecond, float slowPercent = 0f)
    {
        SpawnEffect(position, radius, duration, slowPercent, damagePerSecond, slowPercent > 0f, true);
    }

    static void SpawnEffect(Vector3 position, float radius, float duration, float slowPercent, float dps, bool slow, bool damage)
    {
        var go = new GameObject("GroundZone");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * radius * 2f;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = MapGridControllerShared.GetWhiteSprite();
        renderer.color = damage
            ? new Color(1f, 0.35f, 0.1f, 0.25f)
            : new Color(0.45f, 0.85f, 1f, 0.25f);
        renderer.sortingOrder = 1;

        var zone = go.AddComponent<TowerGroundZone>();
        zone.radius = radius;
        zone.duration = duration;
        zone.slowPercent = slowPercent;
        zone.damagePerSecond = dps;
        zone.expireTime = Time.time + duration;
        zone.appliesSlow = slow;
        zone.appliesDamage = damage;
        ActiveZones.Add(zone);
    }

    void Update()
    {
        if (Time.time >= expireTime)
        {
            ActiveZones.Remove(this);
            Destroy(gameObject);
            return;
        }

        foreach (var enemy in EnemyRegistry.ActiveEnemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            if (Vector2.Distance(enemy.transform.position, transform.position) > radius)
                continue;

            if (appliesSlow)
            {
                var slow = enemy.GetComponent<EnemySlowEffect>();
                if (slow != null)
                    slow.ApplySlow(slowPercent, 0.35f);
            }

            if (appliesDamage && damagePerSecond > 0f)
                enemy.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(damagePerSecond * Time.deltaTime)), DamageType.True);
        }
    }
}
