using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuyUnitPanel : MonoBehaviour
{
    [Header("Units the player can buy")]
    public List<ScriptableUnit> availableUnits = new List<ScriptableUnit>();

    [Header("UI Prefab & Mount Point")]
    public Button unitButtonPrefab;            // prefab with children: "Icon"(Image), "Label"(TMP)
    public Transform contentParent;            // where buttons get spawned (Vertical/Grid Layout)

    [Header("Behavior")]
    public bool closeOnPurchase = true;

    // runtime
    private readonly List<Button> spawned = new();

    private void OnEnable()
    {
        BuildIfNeeded();
        RefreshButtons();
        GameManager.OnGoldChanged += OnGoldChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGoldChanged -= OnGoldChanged;
    }

    private void OnGoldChanged(Team who, int _)
    {
        if (who == Team.Player) RefreshButtons();
    }

    private void BuildIfNeeded()
    {
        if (spawned.Count > 0) return;

        if (unitButtonPrefab == null || contentParent == null)
        {
            Debug.LogWarning("[BuyUnitPanel] Missing unitButtonPrefab or contentParent.");
            return;
        }

        for (int i = 0; i < availableUnits.Count; i++)
        {
            var su = availableUnits[i];
            if (su == null || su.UnitPrefab == null) continue;

            var btn = Instantiate(unitButtonPrefab, contentParent);
            spawned.Add(btn);

            // Hook click
            btn.onClick.AddListener(() => OnChoose(su));

            // Fill visuals
            var icon = btn.transform.Find("Icon")?.GetComponent<Image>();
            var label = btn.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();

            if (icon && su.Icon != null) icon.sprite = su.Icon;        // optional; ok if null
            if (label)
            {
                int cost = Mathf.Max(0, su.GoldCost);                  // adjust if your field name differs
                label.text = $"{su.name}  ({cost})";
            }
        }
    }

    private void RefreshButtons()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        int gold = gm.GetGold(Team.Player);
        bool rallyBlocked = (gm.playerRallyPoint == null) || gm.playerRallyPoint.IsOccupied();

        for (int i = 0; i < spawned.Count; i++)
        {
            var su = i < availableUnits.Count ? availableUnits[i] : null;
            if (su == null) { spawned[i].interactable = false; continue; }

            int cost = Mathf.Max(0, su.GoldCost);
            spawned[i].interactable = !rallyBlocked && gold >= cost;
        }
    }

    private void OnChoose(ScriptableUnit su)
    {
        var gm = GameManager.Instance;
        if (gm == null || su == null || su.UnitPrefab == null) return;

        // Centralized purchase logic (spend + spawn at player rally)
        bool ok = gm.TryPurchaseUnit(su.UnitPrefab);
        if (ok && closeOnPurchase) gameObject.SetActive(false);
        else RefreshButtons(); // e.g., if gold changed but spawn failed
    }

    // public helpers so external buttons can open/close it
    public void Open()  => gameObject.SetActive(true);
    public void Close() => gameObject.SetActive(false);
}
