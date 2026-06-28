using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Combat juice: floating damage/reward text, hit flash, death burst, camera shake, procedural SFX.
/// </summary>
public class CombatFeedbackService : MonoBehaviour
{
    public static CombatFeedbackService Instance { get; private set; }

    const int FloatingTextPoolSize = 48;
    const int BurstPoolSize = 16;

    readonly List<FloatingCombatText> floatingTexts = new();
    readonly List<HitBurstEffect> hitBursts = new();

    AudioSource audioSource;
    CameraShakeController cameraShake;
    Canvas floatingCanvas;
    Camera worldCamera;
    TMP_FontAsset floatingFont;
    Transform burstRoot;

    AudioClip critClip;
    AudioClip deathClip;
    AudioClip goldClip;
    AudioClip diamondClip;
    AudioClip towerHitClip;

    float masterVolume = 0.85f;
    float lastCritSoundTime = -999f;
    float lastDeathSoundTime = -999f;
    float lastGoldSoundTime = -999f;
    float lastDiamondSoundTime = -999f;
    float lastTowerHitSoundTime = -999f;

    const float CritSoundCooldown = 0.14f;
    const float DeathSoundCooldown = 0.08f;
    const float GoldSoundCooldown = 0.18f;
    const float DiamondSoundCooldown = 0.12f;
    const float TowerHitSoundCooldown = 0.35f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        masterVolume = masterVolumeStatic;
        EnsureRoots();
        EnsureAudio();
        EnsureCameraShake();
        BuildPools();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        var deltaTime = Time.unscaledDeltaTime;

        for (var i = floatingTexts.Count - 1; i >= 0; i--)
        {
            var text = floatingTexts[i];
            if (text == null)
                continue;

            if (text.gameObject.activeSelf)
                text.Tick(deltaTime);
        }

        for (var i = hitBursts.Count - 1; i >= 0; i--)
        {
            var burst = hitBursts[i];
            if (burst == null)
                continue;

            if (burst.gameObject.activeSelf)
                burst.Tick(deltaTime);
        }
    }

    public static void ReportEnemyDamaged(EnemyBase enemy, int finalDamage, DamageType damageType, bool isCrit)
    {
        if (Instance == null || enemy == null || finalDamage <= 0)
            return;

        Instance.ShowDamageNumber(enemy.transform.position, finalDamage, damageType, isCrit);

        if (!ReduceMotion)
        {
            Instance.PlayHitFlash(enemy);
            Instance.PlayHitBurst(enemy.transform.position, GetDamageColor(damageType, isCrit), isCrit ? 0.55f : 0.35f);
        }

        if (isCrit)
            Instance.TryPlayCritSound();
    }

    public static void ReportEnemyDeath(EnemyBase enemy, int goldReward, int diamondReward, bool leaked)
    {
        if (Instance == null || enemy == null)
            return;

        var position = enemy.transform.position;

        if (!leaked)
        {
            if (!ReduceMotion)
                Instance.PlayDeathBurst(position, enemy.DisplayColor);

            Instance.TryPlayDeathSound();

            if (goldReward > 0)
            {
                Instance.ShowFloatingText($"+{goldReward}g", position + Vector3.up * 0.15f,
                    new Color(1f, 0.92f, 0.35f), isCrit: false, fontSize: 28f);
                Instance.TryPlayGoldSound();
            }

            if (diamondReward > 0)
            {
                Instance.ShowFloatingText($"+{diamondReward}D", position + Vector3.up * 0.55f,
                    new Color(0.55f, 0.85f, 1f), isCrit: false, fontSize: 26f);
                Instance.TryPlayDiamondSound();
            }
        }
        else
        {
            Instance.ShowFloatingText("LEAK!", position, new Color(1f, 0.45f, 0.45f), isCrit: false, fontSize: 30f);
        }
    }

    public static void ReportTowerDamaged(Vector3 worldPosition)
    {
        if (Instance == null)
            return;

        if (!ReduceMotion)
        {
            Instance.cameraShake?.Shake(0.12f, 0.22f);
            Instance.PlayHitBurst(worldPosition, new Color(1f, 0.35f, 0.25f), 0.5f);
        }

        Instance.TryPlayTowerHitSound();
    }

    public static void SetMasterVolume(float volume)
    {
        masterVolumeStatic = Mathf.Clamp01(volume);
        if (Instance != null)
            Instance.masterVolume = masterVolumeStatic;
    }

    public static void SetReduceMotion(bool enabled)
    {
        reduceMotionStatic = enabled;
    }

    public static bool ReduceMotion => reduceMotionStatic;

    public static float MasterVolume => Instance != null ? Instance.masterVolume : masterVolumeStatic;

    static float masterVolumeStatic = 0.85f;
    static bool reduceMotionStatic;

    void ShowDamageNumber(Vector3 worldPosition, int damage, DamageType damageType, bool isCrit)
    {
        var color = GetDamageColor(damageType, isCrit);
        var fontSize = isCrit ? 32f : 26f;
        var prefix = isCrit ? "!" : string.Empty;
        ShowFloatingText($"{damage}{prefix}", worldPosition, color, isCrit, fontSize);
    }

    void ShowFloatingText(string text, Vector3 worldPosition, Color color, bool isCrit, float fontSize)
    {
        var drift = new Vector3(Random.Range(-0.25f, 0.25f), RiseSpeedFor(isCrit), 0f);
        if (floatingFont == null)
            return;
        var floating = RentFloatingText();
        if (floating == null)
            return;

        floating.Initialize(text, worldPosition, color, fontSize, drift);
    }

    static float RiseSpeedFor(bool isCrit) => isCrit ? 0.9f : 0.75f;

    static Color GetDamageColor(DamageType damageType, bool isCrit)
    {
        if (isCrit)
            return new Color(1f, 0.82f, 0.2f);

        return damageType switch
        {
            DamageType.Physical => new Color(1f, 0.95f, 0.85f),
            DamageType.Magic => new Color(0.65f, 0.85f, 1f),
            DamageType.True => new Color(1f, 0.55f, 0.35f),
            _ => Color.white
        };
    }

    void PlayHitFlash(EnemyBase enemy)
    {
        if (enemy == null)
            return;

        enemy.PlayHitFlash();
    }

    void PlayHitBurst(Vector3 position, Color color, float size)
    {
        var burst = RentBurst();
        if (burst == null)
            return;

        burst.Play(position, color, size);
    }

    void PlayDeathBurst(Vector3 position, Color color)
    {
        PlayHitBurst(position, Color.Lerp(color, Color.white, 0.35f), 0.85f);
        PlayHitBurst(position, new Color(1f, 0.75f, 0.2f, 0.8f), 0.55f);
    }

    void PlayOneShot(AudioClip clip, float volume, float pitch = 1f)
    {
        if (clip == null || audioSource == null)
            return;

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clip, volume * masterVolume);
        audioSource.pitch = 1f;
    }

    void TryPlayCritSound()
    {
        var now = Time.unscaledTime;
        if (now - lastCritSoundTime < CritSoundCooldown)
            return;

        lastCritSoundTime = now;
        PlayOneShot(critClip, 0.28f, Random.Range(0.92f, 1.08f));
    }

    void TryPlayDeathSound()
    {
        var now = Time.unscaledTime;
        if (now - lastDeathSoundTime < DeathSoundCooldown)
            return;

        lastDeathSoundTime = now;
        PlayOneShot(deathClip, 0.26f, Random.Range(0.88f, 1f));
    }

    void TryPlayGoldSound()
    {
        var now = Time.unscaledTime;
        if (now - lastGoldSoundTime < GoldSoundCooldown)
            return;

        lastGoldSoundTime = now;
        PlayOneShot(goldClip, 0.18f, Random.Range(0.95f, 1.12f));
    }

    void TryPlayDiamondSound()
    {
        var now = Time.unscaledTime;
        if (now - lastDiamondSoundTime < DiamondSoundCooldown)
            return;

        lastDiamondSoundTime = now;
        PlayOneShot(diamondClip, 0.22f, Random.Range(0.96f, 1.06f));
    }

    void TryPlayTowerHitSound()
    {
        var now = Time.unscaledTime;
        if (now - lastTowerHitSoundTime < TowerHitSoundCooldown)
            return;

        lastTowerHitSoundTime = now;
        PlayOneShot(towerHitClip, 0.34f, Random.Range(0.9f, 1f));
    }

    FloatingCombatText RentFloatingText()
    {
        foreach (var text in floatingTexts)
        {
            if (text.IsAvailable)
                return text;
        }

        var created = CreateFloatingText();
        floatingTexts.Add(created);
        return created;
    }

    HitBurstEffect RentBurst()
    {
        foreach (var burst in hitBursts)
        {
            if (burst.IsAvailable)
                return burst;
        }

        var created = CreateBurst();
        hitBursts.Add(created);
        return created;
    }

    FloatingCombatText CreateFloatingText()
    {
        var go = new GameObject("FloatingText", typeof(RectTransform));
        go.transform.SetParent(floatingCanvas.transform, false);
        go.SetActive(true);

        var text = go.AddComponent<FloatingCombatText>();
        text.Setup(worldCamera, floatingFont);
        go.SetActive(false);
        return text;
    }

    HitBurstEffect CreateBurst()
    {
        var go = new GameObject("HitBurst");
        go.transform.SetParent(burstRoot, false);
        var burst = go.AddComponent<HitBurstEffect>();
        go.SetActive(false);
        return burst;
    }

    void EnsureRoots()
    {
        EnsureFloatingCanvas();

        burstRoot = new GameObject("HitBursts").transform;
        burstRoot.SetParent(transform, false);
    }

    void EnsureFloatingCanvas()
    {
        worldCamera = Camera.main;

        var canvasObject = new GameObject("FloatingTextCanvas");
        canvasObject.transform.SetParent(transform, false);

        floatingCanvas = canvasObject.AddComponent<Canvas>();
        floatingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        floatingCanvas.sortingOrder = 200;
        floatingCanvas.pixelPerfect = false;

        canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        UiDisplaySettings.ConfigureCanvas(floatingCanvas);

        floatingFont = ResolveFloatingFont();
        if (floatingFont == null)
            Debug.LogWarning("CombatFeedbackService: TMP font not found. Floating combat text disabled.");
    }

    static TMP_FontAsset ResolveFloatingFont()
    {
        if (TMP_Settings.defaultFontAsset != null)
            return TMP_Settings.defaultFontAsset;

        return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
    }

    void EnsureAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;

        critClip = CreateToneClip("crit", 520f, 0.08f, 0.28f);
        deathClip = CreateToneClip("death", 160f, 0.12f, 0.24f);
        goldClip = CreateToneClip("gold", 760f, 0.06f, 0.18f);
        diamondClip = CreateToneClip("diamond", 920f, 0.08f, 0.2f);
        towerHitClip = CreateToneClip("tower_hit", 95f, 0.14f, 0.3f);
    }

    void EnsureCameraShake()
    {
        var camera = Camera.main;
        if (camera == null)
            return;

        cameraShake = camera.GetComponent<CameraShakeController>();
        if (cameraShake == null)
            cameraShake = camera.gameObject.AddComponent<CameraShakeController>();
    }

    void BuildPools()
    {
        for (var i = 0; i < FloatingTextPoolSize; i++)
            floatingTexts.Add(CreateFloatingText());

        for (var i = 0; i < BurstPoolSize; i++)
            hitBursts.Add(CreateBurst());
    }

    static AudioClip CreateToneClip(string clipName, float frequency, float duration, float volume)
    {
        const int sampleRate = 44100;
        var sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
        var samples = new float[sampleCount];

        for (var i = 0; i < sampleCount; i++)
        {
            var t = i / (float)sampleRate;
            var envelope = 1f - t / duration;
            samples[i] = Mathf.Sin(Mathf.PI * 2f * frequency * t) * envelope * volume;
        }

        var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    sealed class HitBurstEffect : MonoBehaviour
    {
        const float Lifetime = 0.35f;

        SpriteRenderer spriteRenderer;
        float age;
        float startScale;
        Color baseColor;
        bool active;

        public bool IsAvailable => !active;

        void EnsureRenderer()
        {
            if (spriteRenderer != null)
                return;

            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = MapGridControllerShared.GetWhiteSprite();
            spriteRenderer.sortingOrder = 18;
        }

        public void Play(Vector3 position, Color color, float size)
        {
            EnsureRenderer();

            transform.position = position;
            startScale = size;
            transform.localScale = Vector3.one * startScale;
            baseColor = color;
            spriteRenderer.color = color;
            age = 0f;
            active = true;
            gameObject.SetActive(true);
        }

        public void Tick(float deltaTime)
        {
            if (!active)
                return;

            EnsureRenderer();

            age += deltaTime;
            var progress = Mathf.Clamp01(age / Lifetime);
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, startScale * 1.8f, progress);
            spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - progress);

            if (age >= Lifetime)
                Release();
        }

        public void Release()
        {
            active = false;
            gameObject.SetActive(false);
        }
    }
}
