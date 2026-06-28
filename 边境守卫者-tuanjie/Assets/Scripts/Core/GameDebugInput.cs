using UnityEngine;

/// <summary>
/// Debug shortcuts (remove before release).
/// G +1000 gold | D +500 diamonds | L restore lives | R reset run | K reset stars/keys
/// </summary>
public class GameDebugInput : MonoBehaviour
{
    void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (GameManager.Instance == null) return;

        if (Input.GetKeyDown(KeyCode.G))
            GameManager.Instance.AddGold(1000);

        if (Input.GetKeyDown(KeyCode.D))
            GameManager.Instance.AddDiamonds(500);

        if (Input.GetKeyDown(KeyCode.L))
            GameManager.Instance.RestoreLives();

        if (Input.GetKeyDown(KeyCode.R))
            GameManager.Instance.ResetResources();

        if (Input.GetKeyDown(KeyCode.K))
            LevelProgressService.ResetAllProgress();
#endif
    }
}
