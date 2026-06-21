using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Active skill: 3×3 expanding blast with true damage + stun; 50% tower damage; 120s CD, no gold cost.
/// </summary>
public class GoblinMissileController : MonoBehaviour
{
    public static GoblinMissileController Instance { get; private set; }

    const float CooldownSeconds = 120f;
    const int EnemyTrueDamage = 280;
    const float StunDuration = 2.5f;
    const float TowerDamageFraction = 0.5f;
    const float ExpandDuration = 0.42f;
    const float FinalBlastSize = 3f * MapGridSettings.CellSize;
    const float StartBlastSize = 0.18f;

    static readonly Color BlastColor = new(1f, 1f, 1f, 0.88f);

    float cooldownRemaining;
    bool isTargeting;
    CameraShakeController cameraShake;
    Transform effectRoot;
    Coroutine blastRoutine;

    public bool IsTargeting => isTargeting;
    public float CooldownRemaining => Mathf.Max(0f, cooldownRemaining);

    public bool IsReady =>
        cooldownRemaining <= 0f
        && !isTargeting
        && CanUseSkill;

    bool CanUseSkill =>
        GameManager.Instance != null
        && !GameManager.Instance.IsGameOver
        && (GamePauseController.Instance == null || !GamePauseController.Instance.IsPaused)
        && (EasterEggCelebrationUI.Instance == null || !EasterEggCelebrationUI.Instance.IsShowing);

    bool CanLaunchWhileTargeting => isTargeting && CanUseSkill && cooldownRemaining <= 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        effectRoot = new GameObject("GoblinMissileEffects").transform;
        effectRoot.SetParent(transform, false);
        EnsureCameraShake();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (cooldownRemaining > 0f)
            cooldownRemaining -= Time.deltaTime;
    }

    public void BeginTargeting()
    {
        if (!IsReady)
            return;

        isTargeting = true;
    }

    public void CancelTargeting()
    {
        isTargeting = false;
    }

    public bool TryLaunchAt(Vector3 worldPoint)
    {
        if (!CanLaunchWhileTargeting)
            return false;

        LaunchAt(FindTargetCell(worldPoint));
        return true;
    }

    static Vector2Int FindTargetCell(Vector3 worldPoint)
    {
        var gridPoint = MapGridSettings.WorldToGrid(worldPoint);
        var x = Mathf.Clamp(gridPoint.x, 0, MapGridSettings.Width - 1);
        var y = Mathf.Clamp(gridPoint.y, 0, MapGridSettings.Height - 1);
        return new Vector2Int(x, y);
    }

    void LaunchAt(Vector2Int centerCell)
    {
        isTargeting = false;
        cooldownRemaining = CooldownSeconds;

        if (blastRoutine != null)
            StopCoroutine(blastRoutine);

        blastRoutine = StartCoroutine(PlayExpandingBlast(centerCell));
        cameraShake?.Shake(0.4f, 0.5f);
    }

    IEnumerator PlayExpandingBlast(Vector2Int centerCell)
    {
        var center = MapGridSettings.GridToWorld(centerCell.x, centerCell.y);
        var blastObject = new GameObject("GoblinMissileBlast");
        blastObject.transform.SetParent(effectRoot, false);
        blastObject.transform.position = center;
        blastObject.transform.localScale = Vector3.one * StartBlastSize;

        var renderer = blastObject.AddComponent<SpriteRenderer>();
        renderer.sprite = MapGridControllerShared.GetWhiteSprite();
        renderer.color = BlastColor;
        renderer.sortingOrder = 9;

        var damagedEnemies = new HashSet<EnemyBase>();
        var damagedTowers = new HashSet<TowerBase>();

        var elapsed = 0f;
        while (elapsed < ExpandDuration)
        {
            elapsed += Time.deltaTime;
            var progress = Mathf.Clamp01(elapsed / ExpandDuration);
            var eased = 1f - (1f - progress) * (1f - progress);
            var size = Mathf.Lerp(StartBlastSize, FinalBlastSize, eased);
            blastObject.transform.localScale = new Vector3(size, size, 1f);

            ApplyDamageInSquare(center, size * 0.5f, damagedEnemies, damagedTowers);

            var alpha = progress < 0.72f
                ? BlastColor.a
                : Mathf.Lerp(BlastColor.a, 0f, (progress - 0.72f) / 0.28f);
            renderer.color = new Color(BlastColor.r, BlastColor.g, BlastColor.b, alpha);

            yield return null;
        }

        Destroy(blastObject);
        blastRoutine = null;
    }

    void ApplyDamageInSquare(
        Vector3 center,
        float halfSize,
        HashSet<EnemyBase> damagedEnemies,
        HashSet<TowerBase> damagedTowers)
    {
        foreach (var enemy in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (enemy == null || enemy.IsDead || damagedEnemies.Contains(enemy))
                continue;

            if (!IsInsideSquare(enemy.transform.position, center, halfSize))
                continue;

            enemy.TakeDamage(EnemyTrueDamage, DamageType.True);
            enemy.ApplyStun(StunDuration);
            damagedEnemies.Add(enemy);
        }

        foreach (var tower in TowerRegistry.ActiveTowersSnapshot)
        {
            if (tower == null || damagedTowers.Contains(tower))
                continue;

            if (!IsInsideSquare(tower.transform.position, center, halfSize))
                continue;

            var damage = Mathf.Max(1, Mathf.RoundToInt(tower.MaxTowerHealth * TowerDamageFraction));
            tower.TakeTowerDamage(damage);
            damagedTowers.Add(tower);
        }
    }

    static bool IsInsideSquare(Vector3 point, Vector3 center, float halfSize)
    {
        return Mathf.Abs(point.x - center.x) <= halfSize
               && Mathf.Abs(point.y - center.y) <= halfSize;
    }

    void EnsureCameraShake()
    {
        var camera = Camera.main;
        if (camera == null)
            return;

        cameraShake = camera.GetComponent<CameraShakeController>();
        if (cameraShake == null)
            cameraShake = camera.gameObject.AddComponent<CameraShakeController>();

        cameraShake.SyncOrigin();
    }
}
