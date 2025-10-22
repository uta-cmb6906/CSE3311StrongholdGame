using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BuyUnitButton : MonoBehaviour
{
    [Header("Unit To Purchase")]
    [Tooltip("Assign the ScriptableUnit (like Knight, Archer, etc.) that this button should spawn.")]
    public ScriptableUnit unitToBuy;

    [Header("Buyer Settings")]
    [Tooltip("Which team this purchase belongs to.")]
    public Team buyer = Team.Player;

    [Header("Optional: Auto-disable if unaffordable")]
    public Button button;

    private void Reset()
    {
        button = GetComponent<Button>();
    }

    private void Update()
    {
        // Optional: you could grey out the button if the player doesn't have enough gold.
        // For now, just leave it enabled. GameManager/UnitManager will actually block the purchase if needed.
        if (button != null && unitToBuy != null && GameManager.Instance != null)
        {
            button.interactable = true;
        }
    }

    // Called when the player clicks the "Buy" button in the UI
    public void OnClick_Buy()
    {
        if (unitToBuy == null)
        {
            Debug.LogWarning("[BuyUnit] Missing ScriptableUnit reference on button.");
            return;
        }

        // Find a valid tile to spawn this unit (usually a rally point or city)
        Tile spawnTile = FindSpawnTile(buyer);
        if (spawnTile == null)
        {
            Debug.LogWarning("[BuyUnit] No open rally point or city found for this team.");
            return;
        }

        // Try to create the unit — UnitManager handles gold cost + validation
        bool created = UnitManager.Instance.CreateUnit(
            unitToBuy.UnitPrefab,
            spawnTile,
            ignoreCost: false,
            buyer: buyer
        );

        if (!created)
        {
            Debug.Log("[BuyUnit] Purchase failed (likely not enough gold or tile occupied).");
            return;
        }

        Debug.Log($"[BuyUnit] Spawned {unitToBuy.name} for {buyer} at ({spawnTile.X()}, {spawnTile.Y()}).");
    }

    // ----------------- Helper Functions -----------------

    // Finds a valid tile to place the unit for the given team
    private Tile FindSpawnTile(Team team)
    {
        // 1. Prefer a rally point belonging to this team that isn't occupied
        var rally = FindObjectsOfType<RallyPointTile>()
                    .FirstOrDefault(t => (t.isPlayer ? Team.Player : Team.Enemy) == team && !t.IsOccupied());
        if (rally != null) return rally;

        // 2. Otherwise, check for an unoccupied friendly city
        var city = FindObjectsOfType<CityTile>()
                   .FirstOrDefault(t => (t.isPlayer ? Team.Player : Team.Enemy) == team && !t.IsOccupied());
        if (city != null) return city;

        // 3. Last resort — find an empty tile next to a friendly city
        var anyCity = FindObjectsOfType<CityTile>()
                      .FirstOrDefault(t => (t.isPlayer ? Team.Player : Team.Enemy) == team);
        if (anyCity != null)
        {
            var adj = GetFirstEmptyNeighbor(anyCity);
            if (adj != null) return adj;
        }

        return null;
    }

    // Simple neighbor check to find an empty tile next to a city
    private Tile GetFirstEmptyNeighbor(Tile origin)
    {
        var candidates = new[]
        {
            GridManager.Instance.GetTileAtPosition(new Vector2(origin.X() + 1, origin.Y())),
            GridManager.Instance.GetTileAtPosition(new Vector2(origin.X() - 1, origin.Y())),
            GridManager.Instance.GetTileAtPosition(new Vector2(origin.X(), origin.Y() + 1)),
            GridManager.Instance.GetTileAtPosition(new Vector2(origin.X(), origin.Y() - 1))
        };

        // Return the first open one
        return candidates.FirstOrDefault(t => t != null && !t.IsOccupied());
    }
}
