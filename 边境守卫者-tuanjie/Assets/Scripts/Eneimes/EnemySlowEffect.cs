using UnityEngine;

public class EnemySlowEffect : MonoBehaviour
{
    float slowMultiplier = 1f;
    float slowEndTime;

    public float SpeedMultiplier
    {
        get
        {
            if (Time.time >= slowEndTime)
                slowMultiplier = 1f;

            return slowMultiplier;
        }
    }

    public void ApplySlow(float slowPercent, float duration)
    {
        slowPercent = Mathf.Clamp01(slowPercent);
        duration = Mathf.Max(0f, duration);

        slowMultiplier = Mathf.Min(slowMultiplier, 1f - slowPercent);
        slowEndTime = Time.time + duration;
    }
}
