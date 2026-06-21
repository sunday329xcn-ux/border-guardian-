using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EasterEggHintBannerUI : MonoBehaviour
{
    public static EasterEggHintBannerUI Instance { get; private set; }

    const string HintMessage = "Hidden easter eggs await on this map!";

    GameObject bannerRoot;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        UiDisplaySettings.ConfigureCanvas(GetComponentInParent<Canvas>());
        EnsureBannerCreated();
        HideImmediate();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void ShowStartHint()
    {
        EnsureInstance()?.Show();
    }

    public static void HideHint()
    {
        EnsureInstance()?.HideImmediate();
    }

    static EasterEggHintBannerUI EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        var existing = FindObjectOfType<EasterEggHintBannerUI>(true);
        if (existing != null)
            return existing;

        var gameUi = FindObjectOfType<GameUiController>();
        if (gameUi != null)
            return gameUi.gameObject.AddComponent<EasterEggHintBannerUI>();

        var canvas = FindObjectOfType<Canvas>();
        return canvas != null ? canvas.gameObject.AddComponent<EasterEggHintBannerUI>() : null;
    }

    void Show()
    {
        EnsureBannerCreated();
        bannerRoot.SetActive(true);
    }

    void HideImmediate()
    {
        if (bannerRoot != null)
            bannerRoot.SetActive(false);
    }

    void EnsureBannerCreated()
    {
        if (bannerRoot != null)
            return;

        CreateBanner();
    }

    void CreateBanner()
    {
        bannerRoot = new GameObject("EasterEggHintBanner", typeof(RectTransform));
        bannerRoot.transform.SetParent(transform, false);

        var rootRect = bannerRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(640f, 52f);
        rootRect.anchoredPosition = new Vector2(0f, -UiDisplaySettings.HudTopPadding);
        UiDisplaySettings.SnapRectToPixels(rootRect);

        var background = bannerRoot.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(background, 0.94f);
        background.color = new Color(0.12f, 0.18f, 0.12f, 0.94f);

        var accent = new GameObject("Accent", typeof(RectTransform));
        accent.transform.SetParent(bannerRoot.transform, false);
        var accentRect = accent.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(1f, 0f);
        accentRect.pivot = new Vector2(0.5f, 0f);
        accentRect.sizeDelta = new Vector2(0f, 3f);
        accentRect.anchoredPosition = Vector2.zero;
        var accentImage = accent.AddComponent<Image>();
        accentImage.color = UiDisplaySettings.StarGold;
        accentImage.raycastTarget = false;

        var labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(bannerRoot.transform, false);
        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(16f, 6f);
        labelRect.offsetMax = new Vector2(-16f, -6f);

        var label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = HintMessage;
        label.alignment = TextAlignmentOptions.Center;
        label.color = UiDisplaySettings.AccentText;
        label.raycastTarget = false;
        UiDisplaySettings.ApplySharpText(label, UiDisplaySettings.FontSizeBody);
    }
}
