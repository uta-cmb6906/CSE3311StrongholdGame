using UnityEngine;
using TMPro; // TextMeshPro

public class TileInfoDisplayManager : MonoBehaviour
{
    public static TileInfoDisplayManager Instance { get; private set; }

    // track the tile currently displaying its information
    public Tile CurrentTile { get; private set; }

    [Header("UI Elements")]
    [Tooltip("The parent GameObject of the tile info banner.")]
    public GameObject PanelTile;

    [Tooltip("The TextMeshProUGUI component that displays the tile information.")]
    public TextMeshProUGUI infoText;

    private void Awake()
    {
        //  initialization
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Hide the panel at the beginning
        if (PanelTile != null)
        {
            PanelTile.SetActive(false);
        }
    }

    // Accepts the object currently being displayed
    public void DisplayInfo(string info, Tile tile)
    {
        //  Set the CurrentTile reference
        CurrentTile = tile;

        //  Update the TextMeshPro component
        if (infoText != null)
        {
            infoText.text = info;
        }

        //  Show the Panel
        if (PanelTile != null)
        {
            PanelTile.SetActive(true);
        }
    }

    public void HideInfo()
    {
        // Clear the CurrentTile reference
        CurrentTile = null;

        //  Hide the Panel
        if (PanelTile != null)
        {
            PanelTile.SetActive(false);
        }
    }

    public void OnUpgradeButton()
    {
        var unit = UnitManager.Instance.SelectedUnit;
        if (unit == null)
        {
            Debug.Log("[UI] No unit selected to upgrade.");
            return;
        }

        if (!unit.isPlayer)
        {
            Debug.Log("[UI] Cannot upgrade enemy units.");
            return;
        }

        if (GameManager.Instance.TryUpgrade())
        {
            Debug.Log("[UI] Upgrade successful.");
            // refresh UI text so the panel shows new stats
            DisplayInfo(unit.UnitInfo(), unit.OccupiedTile);
        }
        else
        {
            Debug.Log("[UI] Not enough gold to upgrade.");
        }
    }

    // Called by the Heal Button (hook this in OnClick)
    public void OnHealButton()
    {
        var unit = UnitManager.Instance.SelectedUnit;
        if (unit == null)
        {
            Debug.Log("[UI] No unit selected to heal.");
            return;
        }

        if (!unit.isPlayer)
        {
            Debug.Log("[UI] Cannot heal enemy units.");
            return;
        }

        if (GameManager.Instance.TryHeal())
        {
            Debug.Log("[UI] Heal successful.");
            DisplayInfo(unit.UnitInfo(), unit.OccupiedTile);
        }
        else
        {
            Debug.Log("[UI] Not enough gold to heal.");
        }
    }
}
