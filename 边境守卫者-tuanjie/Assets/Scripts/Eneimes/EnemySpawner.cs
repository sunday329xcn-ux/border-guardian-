using UnityEngine;

/// <summary>
/// Legacy spawner kept for reference. Use WaveManager instead.
/// </summary>
[DisallowMultipleComponent]
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] bool disabledInFavorOfWaveManager = true;

    void Start()
    {
        if (disabledInFavorOfWaveManager)
        {
            Debug.Log("EnemySpawner is disabled. WaveManager handles waves now.");
            enabled = false;
        }
    }
}
