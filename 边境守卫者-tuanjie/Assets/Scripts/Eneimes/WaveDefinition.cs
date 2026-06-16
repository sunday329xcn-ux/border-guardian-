using System;
using System.Collections.Generic;
using System.Text;

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
    public string hintText;
    public WaveSpawnGroup[] groups = Array.Empty<WaveSpawnGroup>();
}

public readonly struct WavePreviewEntry
{
    public WavePreviewEntry(EnemyType enemyType, int count)
    {
        EnemyType = enemyType;
        Count = count;
    }

    public EnemyType EnemyType { get; }
    public int Count { get; }
}

public static class WavePreviewHelper
{
    public static IReadOnlyList<WavePreviewEntry> BuildEntries(WaveDefinition wave)
    {
        if (wave?.groups == null || wave.groups.Length == 0)
            return Array.Empty<WavePreviewEntry>();

        var counts = new Dictionary<EnemyType, int>();
        foreach (var group in wave.groups)
        {
            if (group.count <= 0)
                continue;

            counts.TryGetValue(group.enemyType, out var existing);
            counts[group.enemyType] = existing + group.count;
        }

        var entries = new List<WavePreviewEntry>(counts.Count);
        foreach (var pair in counts)
            entries.Add(new WavePreviewEntry(pair.Key, pair.Value));

        entries.Sort((a, b) => string.Compare(
            EnemyCatalog.GetDisplayName(a.EnemyType),
            EnemyCatalog.GetDisplayName(b.EnemyType),
            StringComparison.Ordinal));

        return entries;
    }

    public static string BuildEnemySummary(WaveDefinition wave)
    {
        var entries = BuildEntries(wave);
        if (entries.Count == 0)
            return "No enemies";

        var builder = new StringBuilder();
        for (int i = 0; i < entries.Count; i++)
        {
            if (i > 0)
                builder.Append("  ·  ");

            var entry = entries[i];
            builder.Append(EnemyCatalog.GetDisplayName(entry.EnemyType));
            builder.Append(" x");
            builder.Append(entry.Count);
        }

        return builder.ToString();
    }

    public static string GetHint(WaveDefinition wave)
    {
        if (wave == null || string.IsNullOrWhiteSpace(wave.hintText))
            return string.Empty;

        return wave.hintText;
    }
}
