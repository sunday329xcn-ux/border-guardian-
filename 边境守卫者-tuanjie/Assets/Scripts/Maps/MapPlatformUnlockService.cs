using System.Collections.Generic;
using UnityEngine;

public static class MapPlatformUnlockService
{
    static readonly HashSet<Vector2Int> UnlockedLatentCells = new();

    public static void ResetSession()
    {
        UnlockedLatentCells.Clear();
    }

    public static void TryUnlockForWave(int waveNumber, MapGridController mapGridController)
    {
        if (mapGridController == null || waveNumber <= 0)
            return;

        var unlockedAny = false;
        foreach (var latent in GrimmForestMapLayout.LatentPlatforms)
        {
            if (latent.UnlockAtWave != waveNumber || UnlockedLatentCells.Contains(latent.Cell))
                continue;

            UnlockedLatentCells.Add(latent.Cell);
            mapGridController.UnlockBuildPlatform(latent.Cell, latent.Terrain);
            unlockedAny = true;
        }

        if (!unlockedAny)
            return;

        var hud = Object.FindObjectOfType<GameHUD>();
        hud?.ShowTransientMessage($"Wave {waveNumber}: +2 special build platforms unlocked!", 3.5f);
    }
}
