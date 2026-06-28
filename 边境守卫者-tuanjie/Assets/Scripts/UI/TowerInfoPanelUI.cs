using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TowerInfoPanelUI : MonoBehaviour
{
    GameObject panelRoot;
    TextMeshProUGUI titleText;
    TextMeshProUGUI hpText;
    TextMeshProUGUI detailText;
    Button upgradeButton;
    Button sellButton;
    Button rallyButton;
    Button branchAButton;
    Button branchBButton;
    TextMeshProUGUI upgradeButtonText;
    TextMeshProUGUI sellButtonText;
    TextMeshProUGUI rallyButtonText;

    bool resourcesBound;

    void Start()
    {
        CreatePanel();
        HidePanel();

        TowerSelectionController.OnSelectionChanged += HandleSelectionChanged;
        TowerBase.OnTowerDamaged += HandleTowerDamaged;

        TryBindGameManager();
    }

    void Update()
    {
        if (!resourcesBound)
            TryBindGameManager();
    }

    void TryBindGameManager()
    {
        if (resourcesBound || GameManager.Instance == null)
            return;

        GameManager.Instance.OnResourcesChanged += RefreshPanel;
        resourcesBound = true;
    }

    void OnDestroy()
    {
        TowerSelectionController.OnSelectionChanged -= HandleSelectionChanged;
        TowerBase.OnTowerDamaged -= HandleTowerDamaged;

        if (GameManager.Instance != null)
            GameManager.Instance.OnResourcesChanged -= RefreshPanel;
    }

    void HandleTowerDamaged(TowerBase tower, int damage)
    {
        if (tower == TowerSelectionController.Selected)
            RefreshPanel();
    }

    void HandleSelectionChanged(TowerBase tower)
    {
        if (tower == null)
        {
            HidePanel();
            return;
        }

        if (tower is BarracksTower barracks)
            barracks.CancelRallyPlacement();

        ShowPanel();
        RefreshPanel();
    }

    void RefreshPanel()
    {
        var tower = TowerSelectionController.Selected;
        if (tower == null || panelRoot == null)
            return;

        titleText.text = $"{tower.GetDisplayName()}  ·  Lv.{tower.Level}";
        hpText.text = $"HP  {tower.CurrentTowerHealth} / {tower.MaxTowerHealth}";

        branchAButton.gameObject.SetActive(false);
        branchBButton.gameObject.SetActive(false);
        upgradeButton.gameObject.SetActive(true);
        rallyButton.gameObject.SetActive(tower.SupportsRally);

        if (tower is BarracksTower selectedBarracks)
        {
            rallyButtonText.text = selectedBarracks.IsPlacingRally ? "Pick Rally Point" : $"Rally (r={selectedBarracks.RallyRange:0.#})";
            rallyButton.interactable = true;
        }

        var nextKind = tower.GetNextUpgradeKind();

        if (tower is DiamondMineTower)
        {
            detailText.text = AppendSynergy(tower, "Produces 0.1 diamonds/sec. Cannot upgrade.");
            upgradeButtonText.text = "No Upgrade";
            upgradeButton.interactable = false;
        }
        else if (tower is SpotterTower)
        {
            detailText.text = "Reveals Shades within 3 range. Build before wave 11. Cannot upgrade.";
            upgradeButtonText.text = "No Upgrade";
            upgradeButton.interactable = false;
        }
        else if (tower is BeaconTower)
        {
            detailText.text = "Nearby combat towers +10% attack speed (2.5 range). Cannot upgrade.";
            upgradeButtonText.text = "No Upgrade";
            upgradeButton.interactable = false;
        }
        else if (tower is BountyShrineTower)
        {
            detailText.text = "Kills within 2.8 range grant +15% gold. Cannot upgrade.";
            upgradeButtonText.text = "No Upgrade";
            upgradeButton.interactable = false;
        }
        else if (nextKind == TowerUpgradeKind.Gold)
        {
            var cost = tower.GetUpgradeGoldCostForNextLevel();
            var canAfford = GameManager.Instance != null && GameManager.Instance.Gold >= cost;
            detailText.text = AppendSynergy(tower, canAfford
                ? $"Upgrade to Lv.{tower.Level + 1} with gold."
                : $"Upgrade to Lv.{tower.Level + 1} needs {cost}g (you have {GameManager.Instance?.Gold ?? 0}g).");
            upgradeButtonText.text = $"Upgrade ({cost}g)";
            upgradeButton.interactable = canAfford;
        }
        else if (nextKind == TowerUpgradeKind.DiamondLevel4)
        {
            var cost = tower.GetUpgradeDiamondCostForNextLevel();
            detailText.text = AppendSynergy(tower, "Breakthrough to Lv.4 costs diamonds.");
            upgradeButtonText.text = $"Upgrade ({cost} dia)";
            upgradeButton.interactable = GameManager.Instance != null && GameManager.Instance.Diamonds >= cost;
        }
        else if (nextKind == TowerUpgradeKind.DiamondLevel5Branch)
        {
            upgradeButton.gameObject.SetActive(false);
            detailText.text = AppendSynergy(tower, "Choose a Lv.5 branch:");
            branchAButton.gameObject.SetActive(true);
            branchBButton.gameObject.SetActive(true);

            var costA = tower.GetBranchDiamondCost(TowerBranch.BranchA);
            var costB = tower.GetBranchDiamondCost(TowerBranch.BranchB);
            branchAButton.GetComponentInChildren<TextMeshProUGUI>().text =
                $"{TowerBuildCatalog.GetBranchName(tower.TowerType, TowerBranch.BranchA)} ({costA}d)";
            branchBButton.GetComponentInChildren<TextMeshProUGUI>().text =
                $"{TowerBuildCatalog.GetBranchName(tower.TowerType, TowerBranch.BranchB)} ({costB}d)";
            branchAButton.interactable = GameManager.Instance != null && GameManager.Instance.Diamonds >= costA;
            branchBButton.interactable = GameManager.Instance != null && GameManager.Instance.Diamonds >= costB;
        }
        else
        {
            var branchName = tower.GetBranchDisplayName();
            var baseDetail = string.IsNullOrEmpty(branchName)
                ? "Max level reached."
                : $"Max level. Specialization: {branchName}";
            detailText.text = AppendSynergy(tower, baseDetail);
            upgradeButtonText.text = "Max Level";
            upgradeButton.interactable = false;
        }

        sellButtonText.text = $"Sell (+{tower.GetSellRefund()}g)";
        sellButton.interactable = true;
    }

    static string AppendSynergy(TowerBase tower, string baseText)
    {
        return baseText + TowerSynergyService.BuildPanelSummary(tower);
    }

    void OnUpgradeClicked()
    {
        var tower = TowerSelectionController.Selected;
        if (tower == null)
            return;

        if (tower.TryUpgrade())
            RefreshPanel();
    }

    void OnSellClicked()
    {
        TowerSelectionController.Selected?.TrySell();
    }

    void OnRallyClicked()
    {
        if (TowerSelectionController.Selected is BarracksTower barracks)
        {
            barracks.BeginRallyPlacement();
            RefreshPanel();
        }
    }

    void OnBranchAClicked()
    {
        TryBranchUpgrade(TowerBranch.BranchA);
    }

    void OnBranchBClicked()
    {
        TryBranchUpgrade(TowerBranch.BranchB);
    }

    void TryBranchUpgrade(TowerBranch branch)
    {
        var tower = TowerSelectionController.Selected;
        if (tower == null)
            return;

        if (tower.TryUpgradeBranch(branch))
            RefreshPanel();
    }

    void ShowPanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    void HidePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    void CreatePanel()
    {
        panelRoot = CreateUiObject("TowerInfoPanel", transform);
        var panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(0f, 0f);
        panelRect.pivot = new Vector2(0f, 0f);
        panelRect.sizeDelta = new Vector2(420f, 328f);
        panelRect.anchoredPosition = new Vector2(UiDisplaySettings.LeftInteractiveClearance, UiDisplaySettings.ScreenPadding);
        UiDisplaySettings.SnapRectToPixels(panelRect);

        var background = panelRoot.AddComponent<Image>();
        UiDisplaySettings.ApplyPanelBackground(background);

        titleText = CreateLabel(panelRoot.transform, "Select a tower", 20f, TextAlignmentOptions.TopLeft);
        titleText.enableWordWrapping = false;
        titleText.overflowMode = TextOverflowModes.Ellipsis;
        LayoutTop(titleText.rectTransform, -14f, 28f);

        hpText = CreateLabel(panelRoot.transform, "HP  — / —", 18f, TextAlignmentOptions.TopLeft);
        hpText.color = new Color(0.75f, 0.95f, 0.78f);
        hpText.enableWordWrapping = false;
        LayoutTop(hpText.rectTransform, -44f, 24f);

        detailText = CreateLabel(panelRoot.transform, string.Empty, 15f, TextAlignmentOptions.TopLeft);
        detailText.color = new Color(0.88f, 0.92f, 0.88f);
        detailText.enableWordWrapping = true;
        detailText.lineSpacing = -2f;
        LayoutTop(detailText.rectTransform, -72f, 148f);

        var upgradeObject = CreateButton(panelRoot.transform, "Upgrade", new Vector2(112f, 40f), 16f, OnUpgradeClicked);
        upgradeButton = upgradeObject.GetComponent<Button>();
        upgradeButtonText = upgradeObject.GetComponentInChildren<TextMeshProUGUI>();
        PlaceBottomLeft(upgradeObject.GetComponent<RectTransform>(), 16f, 80f);

        rallyObject = CreateButton(panelRoot.transform, "Rally", new Vector2(112f, 40f), 16f, OnRallyClicked);
        rallyButton = rallyObject.GetComponent<Button>();
        rallyButtonText = rallyObject.GetComponentInChildren<TextMeshProUGUI>();
        PlaceBottomLeft(rallyObject.GetComponent<RectTransform>(), 136f, 80f);

        var sellObject = CreateButton(panelRoot.transform, "Sell", new Vector2(112f, 40f), 16f, OnSellClicked);
        sellButton = sellObject.GetComponent<Button>();
        sellButtonText = sellObject.GetComponentInChildren<TextMeshProUGUI>();
        PlaceBottomLeft(sellObject.GetComponent<RectTransform>(), 276f, 80f);

        branchAButton = CreateButton(panelRoot.transform, "Branch A", new Vector2(188f, 40f), 15f, OnBranchAClicked).GetComponent<Button>();
        PlaceBottomLeft(branchAButton.GetComponent<RectTransform>(), 16f, 28f);

        branchBButton = CreateButton(panelRoot.transform, "Branch B", new Vector2(188f, 40f), 15f, OnBranchBClicked).GetComponent<Button>();
        PlaceBottomLeft(branchBButton.GetComponent<RectTransform>(), 216f, 28f);
    }

    GameObject rallyObject;

    static void LayoutTop(RectTransform rect, float y, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(16f, y);
        rect.sizeDelta = new Vector2(-32f, height);
    }

    static void PlaceBottomLeft(RectTransform rect, float x, float y)
    {
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(x, y);
    }

    static GameObject CreateUiObject(string objectName, Transform parent)
    {
        var go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = parent.gameObject.layer;
        return go;
    }

    static TextMeshProUGUI CreateLabel(Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
    {
        var go = CreateUiObject("Label", parent);
        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.alignment = alignment;
        label.color = Color.white;
        label.raycastTarget = false;
        UiDisplaySettings.ApplySharpText(label, fontSize);
        return label;
    }

    static GameObject CreateButton(Transform parent, string label, Vector2 size, float fontSize, UnityAction onClick)
    {
        var go = CreateUiObject("Button", parent);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        var image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.85f);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var text = CreateLabel(go.transform, label, fontSize, TextAlignmentOptions.Center);
        UiDisplaySettings.ApplyButtonText(text, fontSize);
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(6f, 4f);
        textRect.offsetMax = new Vector2(-6f, -4f);
        return go;
    }
}
