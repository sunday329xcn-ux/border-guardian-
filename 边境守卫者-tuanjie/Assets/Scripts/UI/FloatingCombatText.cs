using TMPro;
using UnityEngine;

public class FloatingCombatText : MonoBehaviour
{
    const float Lifetime = 0.85f;

    TextMeshProUGUI label;
    RectTransform rectTransform;
    Camera worldCamera;
    Vector3 worldPosition;
    Color baseColor;
    float age;
    Vector3 worldDriftVelocity;
    bool active;
    bool configured;

    public bool IsAvailable => !active;

    public void Setup(Camera camera, TMP_FontAsset font)
    {
        worldCamera = camera != null ? camera : Camera.main;
        rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(140f, 56f);

        label = gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null)
        {
            label.font = font;
            label.fontSharedMaterial = font.material;
        }

        label.alignment = TextAlignmentOptions.Center;
        label.fontStyle = FontStyles.Bold;
        label.raycastTarget = false;
        label.enableWordWrapping = false;
        configured = label.font != null;
    }

    public void Initialize(string text, Vector3 anchorWorldPosition, Color color, float fontSize, Vector3 worldDrift)
    {
        if (!configured || label == null)
        {
            active = false;
            gameObject.SetActive(false);
            return;
        }

        worldPosition = anchorWorldPosition;
        label.text = text;
        baseColor = color;
        label.color = color;
        label.fontSize = fontSize;
        worldDriftVelocity = worldDrift;
        age = 0f;
        active = true;
        UpdateScreenPosition();
        gameObject.SetActive(true);
    }

    public void Tick(float deltaTime)
    {
        if (!active || label == null)
            return;

        age += deltaTime;
        worldPosition += worldDriftVelocity * deltaTime;
        UpdateScreenPosition();

        var alpha = 1f - Mathf.Clamp01(age / Lifetime);
        label.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

        if (age >= Lifetime)
            Release();
    }

    public void Release()
    {
        active = false;
        gameObject.SetActive(false);
    }

    void UpdateScreenPosition()
    {
        if (rectTransform == null)
            return;

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (worldCamera == null)
            return;

        var screenPoint = worldCamera.WorldToScreenPoint(worldPosition);
        if (screenPoint.z < 0f)
        {
            Release();
            return;
        }

        rectTransform.position = screenPoint;
    }
}
