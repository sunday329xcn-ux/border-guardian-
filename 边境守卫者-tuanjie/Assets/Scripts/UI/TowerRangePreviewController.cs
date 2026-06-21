using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draws build/selection range rings and synergy partner links.
/// </summary>
public class TowerRangePreviewController : MonoBehaviour
{
    const int CircleSegments = 56;
    const float BuildHoverRadius = 0.55f;

    [SerializeField] MapGridController mapGridController;
    [SerializeField] TowerBuildSelector buildSelector;
    [SerializeField] float lineWidth = 0.045f;

    Camera mainCamera;
    LineRenderer buildRangeRing;
    LineRenderer selectedRangeRing;
    LineRenderer synergyRangeRing;
    readonly List<LineRenderer> synergyLines = new();

    Material lineMaterial;
    TowerBase selectedTower;

    void Start()
    {
        mainCamera = Camera.main;

        if (mapGridController == null)
            mapGridController = FindObjectOfType<MapGridController>();

        if (buildSelector == null)
            buildSelector = FindObjectOfType<TowerBuildSelector>();

        lineMaterial = new Material(Shader.Find("Sprites/Default"));
        buildRangeRing = CreateRing("BuildRangeRing", new Color(0.45f, 0.85f, 0.55f, 0.55f));
        selectedRangeRing = CreateRing("SelectedRangeRing", new Color(1f, 0.92f, 0.35f, 0.75f));
        synergyRangeRing = CreateRing("SynergyRangeRing", new Color(0.55f, 0.75f, 1f, 0.45f));

        SetRingVisible(buildRangeRing, false);
        SetRingVisible(selectedRangeRing, false);
        SetRingVisible(synergyRangeRing, false);

        TowerSelectionController.OnSelectionChanged += HandleTowerSelectionChanged;
        BuildSlotSelectionController.OnSelectionChanged += HandleBuildPreviewChanged;
        if (buildSelector != null)
            buildSelector.OnSelectionChanged += HandleBuildPreviewChanged;
    }

    void OnDestroy()
    {
        TowerSelectionController.OnSelectionChanged -= HandleTowerSelectionChanged;
        BuildSlotSelectionController.OnSelectionChanged -= HandleBuildPreviewChanged;
        if (buildSelector != null)
            buildSelector.OnSelectionChanged -= HandleBuildPreviewChanged;
    }

    void HandleBuildPreviewChanged(BuildSlot _)
    {
        RefreshBuildPreview();
    }

    void HandleBuildPreviewChanged(TowerType _)
    {
        RefreshBuildPreview();
    }

    void Update()
    {
        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        RefreshBuildPreview();
        RefreshSelectedPreview();
    }

    void HandleTowerSelectionChanged(TowerBase tower)
    {
        selectedTower = tower;
        RefreshSelectedPreview();
    }

    void RefreshBuildPreview()
    {
        if (buildSelector == null || mapGridController == null || buildRangeRing == null)
            return;

        if (TowerSelectionController.Selected != null && !TowerBuildDragHandler.IsDragging)
        {
            SetRingVisible(buildRangeRing, false);
            return;
        }

        TowerType previewType;
        BuildSlot slot;

        if (TowerBuildDragHandler.IsDragging)
        {
            previewType = TowerBuildDragHandler.DraggingType;
            slot = FindHoveredBuildSlot() ?? BuildSlotSelectionController.Selected;
        }
        else
        {
            var selectedSlot = BuildSlotSelectionController.Selected;
            if (selectedSlot == null || !TowerBuildBarHoverController.TryGetHovered(out previewType))
            {
                SetRingVisible(buildRangeRing, false);
                return;
            }

            slot = selectedSlot;
        }

        if (slot == null || (!TowerBuildCatalog.ShowsCombatRangeRing(previewType) && !TowerBuildCatalog.ShowsSupportRangeRing(previewType)))
        {
            SetRingVisible(buildRangeRing, false);
            return;
        }

        var range = TowerBuildCatalog.GetPreviewRange(previewType);
        var ringColor = TowerBuildCatalog.ShowsSupportRangeRing(previewType)
            ? new Color(0.95f, 0.75f, 0.35f, 0.55f)
            : new Color(0.45f, 0.85f, 0.55f, 0.55f);
        buildRangeRing.startColor = ringColor;
        buildRangeRing.endColor = ringColor;
        SetCircle(buildRangeRing, slot.transform.position, range);
        SetRingVisible(buildRangeRing, true);
    }

    void RefreshSelectedPreview()
    {
        ClearSynergyLines();

        if (selectedTower == null)
        {
            SetRingVisible(selectedRangeRing, false);
            SetRingVisible(synergyRangeRing, false);
            return;
        }

        if (selectedTower is DiamondMineTower)
        {
            SetRingVisible(selectedRangeRing, false);
            SetRingVisible(synergyRangeRing, false);
            return;
        }

        if (selectedTower is SpotterTower or BeaconTower or BountyShrineTower)
        {
            var supportRange = selectedTower switch
            {
                SpotterTower => SpotterTower.RevealRadius,
                BeaconTower => SupportTowerService.BeaconRadius,
                BountyShrineTower => SupportTowerService.BountyRadius,
                _ => 0f
            };

            SetCircle(selectedRangeRing, selectedTower.transform.position, supportRange);
            SetRingVisible(selectedRangeRing, true);
            SetRingVisible(synergyRangeRing, false);
            return;
        }

        var center = selectedTower.transform.position;
        var range = selectedTower.Range;

        if (selectedTower is BarracksTower barracks)
            range = barracks.RallyRange;

        SetCircle(selectedRangeRing, center, range);
        SetRingVisible(selectedRangeRing, true);

        if (!selectedTower.HasSynergyUnlocked ||
            selectedTower.SynergyRange <= 0.01f ||
            (!TowerSynergyCatalog.IsCombatTower(selectedTower.TowerType) && selectedTower.TowerType != TowerType.Barracks))
        {
            SetRingVisible(synergyRangeRing, false);
            return;
        }

        SetCircle(synergyRangeRing, center, selectedTower.SynergyRange);
        SetRingVisible(synergyRangeRing, true);

        DrawSynergyLinks(selectedTower);
    }

    void DrawSynergyLinks(TowerBase tower)
    {
        foreach (var rule in TowerSynergyCatalog.RulesList)
        {
            if (!TowerSynergyService.IsRuleActive(tower, rule))
                continue;

            var partnerType = tower.TowerType == rule.PartnerA ? rule.PartnerB : rule.PartnerA;
            var partner = FindNearestPartner(tower, partnerType, tower.SynergyRange);
            if (partner == null)
                continue;

            var line = RentSynergyLine();
            line.startColor = line.endColor = new Color(0.65f, 0.9f, 1f, 0.85f);
            line.SetPosition(0, tower.transform.position);
            line.SetPosition(1, partner.transform.position);
            line.gameObject.SetActive(true);
        }
    }

    static TowerBase FindNearestPartner(TowerBase tower, TowerType partnerType, float range)
    {
        TowerBase best = null;
        var bestDist = float.MaxValue;
        var rangeSqr = range * range;

        foreach (var candidate in TowerRegistry.ActiveTowersSnapshot)
        {
            if (candidate == null || candidate == tower || candidate.TowerType != partnerType)
                continue;

            if (!candidate.HasSynergyUnlocked)
                continue;

            var distSqr = (candidate.transform.position - tower.transform.position).sqrMagnitude;
            if (distSqr > rangeSqr || distSqr >= bestDist)
                continue;

            bestDist = distSqr;
            best = candidate;
        }

        return best;
    }

    BuildSlot FindHoveredBuildSlot()
    {
        if (mainCamera == null || mapGridController == null)
            return null;

        return BuildSlotPlacementUtility.FindBuildSlotAt(
            mapGridController,
            GetMouseWorldPosition(),
            BuildHoverRadius);
    }

    Vector3 GetMouseWorldPosition()
    {
        var screenPoint = Input.mousePosition;
        screenPoint.z = Mathf.Abs(mainCamera.transform.position.z);
        var worldPoint = mainCamera.ScreenToWorldPoint(screenPoint);
        worldPoint.z = 0f;
        return worldPoint;
    }

    LineRenderer CreateRing(string objectName, Color color)
    {
        var go = new GameObject(objectName);
        go.transform.SetParent(transform, false);

        var line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = true;
        line.widthMultiplier = lineWidth;
        line.numCapVertices = 4;
        line.numCornerVertices = 4;
        line.material = lineMaterial;
        line.sortingOrder = 2;
        line.startColor = line.endColor = color;
        line.positionCount = CircleSegments + 1;
        return line;
    }

    LineRenderer RentSynergyLine()
    {
        foreach (var line in synergyLines)
        {
            if (!line.gameObject.activeSelf)
                return line;
        }

        var go = new GameObject("SynergyLine");
        go.transform.SetParent(transform, false);
        var created = go.AddComponent<LineRenderer>();
        created.useWorldSpace = true;
        created.loop = false;
        created.widthMultiplier = lineWidth * 0.85f;
        created.material = lineMaterial;
        created.sortingOrder = 3;
        created.positionCount = 2;
        synergyLines.Add(created);
        return created;
    }

    void ClearSynergyLines()
    {
        foreach (var line in synergyLines)
        {
            if (line != null)
                line.gameObject.SetActive(false);
        }
    }

    static void SetCircle(LineRenderer line, Vector3 center, float radius)
    {
        if (line == null || radius <= 0.01f)
            return;

        line.positionCount = CircleSegments + 1;
        for (var i = 0; i <= CircleSegments; i++)
        {
            var angle = i / (float)CircleSegments * Mathf.PI * 2f;
            line.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }

    static void SetRingVisible(LineRenderer line, bool visible)
    {
        if (line != null)
            line.gameObject.SetActive(visible);
    }
}
