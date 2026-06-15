using UnityEngine;

/// <summary>
/// Debug shortcuts (remove before release).
/// G +10 gold | D +1 diamond | L -1 life | R reset
/// </summary>
public class GameDebugInput : MonoBehaviour
{
    void Update()
    {
        if (GameManager.Instance == null) return;

        if (Input.GetKeyDown(KeyCode.G))
            GameManager.Instance.AddGold(10);

        if (Input.GetKeyDown(KeyCode.D))
            GameManager.Instance.AddDiamonds(1);

        if (Input.GetKeyDown(KeyCode.L))
            GameManager.Instance.TakeDamage(1);

        if (Input.GetKeyDown(KeyCode.R))
            GameManager.Instance.ResetResources();
    }
}
