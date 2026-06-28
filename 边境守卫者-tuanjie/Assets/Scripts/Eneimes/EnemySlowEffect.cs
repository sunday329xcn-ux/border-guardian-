using UnityEngine;

public class EnemySlowEffect : MonoBehaviour
{
    float slowMultiplier = 1f;
    float slowEndTime;
    float speedBuffMultiplier = 1f;
    float buffEndTime;

    public float SpeedMultiplier
    {
        get
        {
            if (Time.time >= slowEndTime)
                slowMultiplier = 1f;

            if (Time.time >= buffEndTime)
                speedBuffMultiplier = 1f;

            return slowMultiplier * speedBuffMultiplier;
        }
    }

    public void ApplySlow(float slowPercent, float duration)
    {
        slowPercent = Mathf.Clamp01(slowPercent * MapModifierService.SlowMultiplier);
        duration = Mathf.Max(0f, duration);

        slowMultiplier = Mathf.Min(slowMultiplier, 1f - slowPercent);
        slowEndTime = Time.time + duration;
    }

    public void ApplySpeedBuff(float buffPercent, float duration)
    {
        buffPercent = Mathf.Max(0f, buffPercent);
        duration = Mathf.Max(0f, duration);

        speedBuffMultiplier = Mathf.Max(speedBuffMultiplier, 1f + buffPercent);
        buffEndTime = Time.time + duration;
    }
}
