using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuyUnitPanel : MonoBehaviour
{
    [Header("Units the player can buy")]
    public List<ScriptableUnit> AvailableUnits = new List<ScriptableUnit>(); // drag ScriptableUnit assets here

    [Header("UI Prefab & Mount Point")]
    public Button UnitButtonPrefab;           // your UnitChoiceButton prefab (must have a TMP_Text child named Label or any TMP child)
    public RectTransform ContentParent;       // the "Content" RectTransform under the panel

    [Header("Behavior")]
    public bool CloseOnPurchase = true;

    private readonly List<GameObject> _spawned = new();

    private void OnEnable()
    {
        // build/refresh whenever panel opens
        BuildList();
        // update interactability when gold changes
        GameManager.OnGoldChanged += OnGoldChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGoldChanged -= OnGoldChanged;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        BuildList();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnGoldChanged(Team who, int _)
    {
        if (who == Team.Player) RefreshInteractable();
    }

    private void BuildList()
    {
        // clear old
        foreach (var go in _spawned) if (go) Destroy(go);
        _spawned.Clear();

        if (!UnitButtonPrefab || !ContentParent)
        {
            Debug.LogWarning("[BuyUnitPanel] Missing UnitButtonPrefab or ContentParent.");
            return;
        }

        foreach (var su in AvailableUnits)
        {
            if (su == null || su.UnitPrefab == null) continue;

            var btn = Instantiate(UnitButtonPrefab, ContentParent);
            _spawned.Add(btn.gameObject);

            // label text (name + cost)
            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                int cost = Mathf.Max(0, su.GoldCost);
                label.text = $"{su.name}  ({cost})";
            }

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                // central purchase path (spends + checks rally in your GameManager)
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TryPurchaseUnit(su.UnitPrefab);
                }
                if (CloseOnPurchase) Hide();
                else RefreshInteractable();
            });
        }

        RefreshInteractable();
    }

    private void RefreshInteractable()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        int gold = gm.GetGold(Team.Player);
        bool rallyBlocked = (gm.playerRallyPoint == null) || gm.playerRallyPoint.IsOccupied();

        int i = 0;
        foreach (var su in AvailableUnits)
        {
            if (i >= _spawned.Count) break;
            var go = _spawned[i++];
            if (!go) continue;

            var btn = go.GetComponent<Button>();
            if (!btn || su == null) { if (btn) btn.interactable = false; continue; }

            int cost = Mathf.Max(0, su.GoldCost);
            btn.interactable = !rallyBlocked && gold >= cost;
        }
    }
}
