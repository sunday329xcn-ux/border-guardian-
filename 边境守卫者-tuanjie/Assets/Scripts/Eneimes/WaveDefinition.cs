using System;

[Serializable]
public struct WaveSpawnGroup
{
    public EnemyType enemyType;
    public int count;
    public float spawnInterval;
    public float delayBeforeGroup;
}

[Serializable]
public class WaveDefinition
{
    public string note;
    public WaveSpawnGroup[] groups = Array.Empty<WaveSpawnGroup>();
}
