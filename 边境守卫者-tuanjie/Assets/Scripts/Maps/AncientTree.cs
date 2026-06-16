using UnityEngine;

public class AncientTree : MonoBehaviour
{
    [SerializeField] int goldCost = 50;
    [SerializeField] float cooldownSeconds = 60f;
    [SerializeField] float effectRadius = 2f;
    [SerializeField] float slowPercent = 0.8f;
    [SerializeField] float effectDuration = 4f;

    Vector3 effectCenter;
    float cooldownRemaining;
    SpriteRenderer spriteRenderer;
    Color readyColor = new Color(0.2f, 0.55f, 0.25f);
    Color cooldownColor = new Color(0.35f, 0.35f, 0.35f);

    public bool IsReady => cooldownRemaining <= 0f;

    public void Initialize(Vector3 treePosition, Vector3 entangleCenter)
    {
        transform.position = treePosition;
        effectCenter = entangleCenter;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = MapGridControllerShared.GetWhiteSprite();
            spriteRenderer.sortingOrder = 3;
        }

        transform.localScale = new Vector3(0.7f, 1.1f, 1f);
        spriteRenderer.color = readyColor;

        var collider = gameObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.55f;
        collider.isTrigger = true;
    }

    void Update()
    {
        if (cooldownRemaining > 0f)
            cooldownRemaining -= Time.deltaTime;

        if (spriteRenderer == null)
            return;

        spriteRenderer.color = IsReady ? readyColor : cooldownColor;
    }

    public bool TryActivate()
    {
        if (!IsReady)
            return false;

        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return false;

        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return false;

        if (!GameManager.Instance.TrySpendGold(goldCost))
            return false;

        RootEntangleZone.Spawn(effectCenter, effectRadius, slowPercent, effectDuration);
        cooldownRemaining = cooldownSeconds;
        return true;
    }
}
