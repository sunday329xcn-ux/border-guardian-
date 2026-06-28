using System;
using UnityEngine;

/// <summary>
/// Owns the hero unit and drives its input + abilities (P4.1):
/// right-click to move, three active skills with cooldowns, XP from kills.
/// Created at runtime by <c>GameUiController</c>, so no scene authoring needed.
/// </summary>
public class HeroController : MonoBehaviour
{
    public static HeroController Instance { get; private set; }

    static readonly float[] SkillCooldown = { 12f, 20f, 18f };

    HeroUnit hero;
    Camera cam;
    bool spawned;

    HeroSkillId armedSkill;
    bool hasArmedSkill;

    readonly float[] cooldownReadyAt = new float[3];

    public HeroUnit Hero => hero;
    public bool IsMeteorArmed => hasArmedSkill && armedSkill == HeroSkillId.Meteor;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cam = Camera.main;
        EnemyBase.OnEnemyKilledByPlayer += HandleEnemyKilled;
    }

    void OnDestroy()
    {
        EnemyBase.OnEnemyKilledByPlayer -= HandleEnemyKilled;
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (!MainMenuUI.IsSessionStarted)
            return;

        if (cam == null)
            cam = Camera.main;

        EnsureHero();

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return;

        HandleMoveCommand();
    }

    void EnsureHero()
    {
        if (spawned)
            return;

        var spawn = MapGridSettings.GridToWorld(MapGridSettings.Width / 2, 3);
        hero = HeroUnit.Spawn(spawn);
        spawned = true;
    }

    void HandleMoveCommand()
    {
        if (hero == null || hero.IsDead || cam == null)
            return;

        if (!Input.GetMouseButtonDown(1))
            return;

        if (UiInputUtility.IsPointerOverUi())
            return;

        hero.SetDestination(ScreenToWorld());
    }

    Vector3 ScreenToWorld()
    {
        var sp = Input.mousePosition;
        sp.z = Mathf.Abs(cam.transform.position.z);
        var wp = cam.ScreenToWorldPoint(sp);
        wp.z = 0f;
        return wp;
    }

    public bool IsSkillReady(HeroSkillId skill)
    {
        if (hero == null || hero.IsDead)
            return false;

        return Time.time >= cooldownReadyAt[(int)skill];
    }

    public float GetCooldownRemaining(HeroSkillId skill) =>
        Mathf.Max(0f, cooldownReadyAt[(int)skill] - Time.time);

    public float GetCooldownTotal(HeroSkillId skill) => SkillCooldown[(int)skill];

    public void ActivateSkill(HeroSkillId skill)
    {
        if (!IsSkillReady(skill))
            return;

        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return;

        switch (skill)
        {
            case HeroSkillId.Meteor:
                hasArmedSkill = true;
                armedSkill = HeroSkillId.Meteor;
                break;
            case HeroSkillId.Freeze:
                CastFreeze();
                StartCooldown(skill);
                break;
            case HeroSkillId.Reinforce:
                CastReinforce();
                StartCooldown(skill);
                break;
        }
    }

    public void CancelArmedSkill() => hasArmedSkill = false;

    /// <summary>Consumes a world click when a targeted skill is armed (mirrors Goblin Missile).</summary>
    public bool TryCastArmedSkillAt(Vector3 worldPoint)
    {
        if (!hasArmedSkill)
            return false;

        hasArmedSkill = false;

        if (armedSkill == HeroSkillId.Meteor && IsSkillReady(HeroSkillId.Meteor))
        {
            CastMeteor(worldPoint);
            StartCooldown(HeroSkillId.Meteor);
        }

        return true;
    }

    void StartCooldown(HeroSkillId skill)
    {
        cooldownReadyAt[(int)skill] = Time.time + SkillCooldown[(int)skill];
    }

    void CastMeteor(Vector3 center)
    {
        center.z = 0f;
        var level = hero != null ? hero.Level : 1;
        var damage = 80 + 20 * level;
        const float radius = 1.8f;
        var radiusSqr = radius * radius;

        foreach (var enemy in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            if ((enemy.transform.position - center).sqrMagnitude <= radiusSqr)
                enemy.TakeDamage(damage, DamageType.True);
        }
    }

    void CastFreeze()
    {
        foreach (var enemy in EnemyRegistry.ActiveEnemiesSnapshot)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            var slow = enemy.GetComponent<EnemySlowEffect>();
            if (slow != null)
                slow.ApplySlow(0.6f, 3f);
        }
    }

    void CastReinforce()
    {
        if (hero == null)
            return;

        var basePos = hero.transform.position;
        var reinforceColor = new Color(0.7f, 0.85f, 0.5f);
        for (int i = 0; i < 2; i++)
        {
            var offset = new Vector3(i == 0 ? -0.4f : 0.4f, -0.2f, 0f);
            var soldier = SoldierUnit.Spawn(null, basePos + offset, reinforceColor, 120, 5, 10, 16, 0.8f);
            soldier.SetTemporaryLifetime(12f);
        }
    }

    void HandleEnemyKilled(EnemyBase enemy)
    {
        if (hero == null || enemy == null)
            return;

        var xp = enemy.IsBoss ? 20 : enemy.IsElite ? 5 : 1;
        hero.AddExperience(xp);
    }
}
