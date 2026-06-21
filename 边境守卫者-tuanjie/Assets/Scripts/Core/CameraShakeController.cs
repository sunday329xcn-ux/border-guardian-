using UnityEngine;

public class CameraShakeController : MonoBehaviour
{
    Vector3 originalLocalPosition;
    float shakeTimer;
    float shakeIntensity;

    void Awake()
    {
        SyncOrigin();
    }

    void Start()
    {
        SyncOrigin();
    }

    public void SyncOrigin()
    {
        originalLocalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        if (shakeTimer <= 0f)
        {
            transform.localPosition = originalLocalPosition;
            return;
        }

        shakeTimer -= Time.unscaledDeltaTime;
        var falloff = Mathf.Clamp01(shakeTimer / 0.25f);
        var offset = Random.insideUnitCircle * shakeIntensity * falloff;
        transform.localPosition = originalLocalPosition + new Vector3(offset.x, offset.y, 0f);

        if (shakeTimer <= 0f)
            transform.localPosition = originalLocalPosition;
    }

    public void Shake(float intensity, float duration)
    {
        if (intensity <= 0f || duration <= 0f)
            return;

        shakeIntensity = Mathf.Max(shakeIntensity, intensity);
        shakeTimer = Mathf.Max(shakeTimer, duration);
    }
}
