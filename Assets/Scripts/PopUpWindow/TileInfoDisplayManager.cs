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
}
