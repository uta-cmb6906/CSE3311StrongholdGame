using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Button handler for buying/spawning a unit at the currently selected tile.
/// NOTE: We avoid referencing Tile.isPlayer (varies by branch) and we don't hard-code
/// UnitManager.CreateUnit's signature (we call it via reflection if present).
/// If no usable CreateUnit overload is found, we fall back to a local Instantiate so
/// the UI remains testable without changing other scripts.
/// </summary>
public class BuyUnitButton : MonoBehaviour
{
    [Header("Unit to buy")]
    [Tooltip("Scriptable unit (holds the prefab) to create when this button is pressed.")]
    public ScriptableUnit unitToBuy;             // must expose UnitPrefab (BaseUnit)

    [Header("Economy")]
    [Tooltip("Optional cost to charge. If 0, no spending check is performed here.")]
    public int cost = 0;

    [Tooltip("Who pays / who owns the unit you spawn.")]
    public Team buyerTeam = Team.Player;

    [Header("UI")]
    [Tooltip("Optional: disable the button when purchase is not possible.")]
    public Button button;

    // Hook this to the button's OnClick in the Inspector
    public void OnClickBuy()
    {
        if (unitToBuy == null || unitToBuy.UnitPrefab == null)
        {
            Debug.LogWarning("[BuyUnit] No ScriptableUnit or UnitPrefab set.");
            return;
        }

        // We rely on the tile the player most recently clicked (already tracked by your UI)
        var currentTile = TileInfoDisplayManager.Instance != null
            ? TileInfoDisplayManager.Instance.CurrentTile
            : null;

        if (currentTile == null)
        {
            Debug.Log("[BuyUnit] No tile selected. Click a tile first (ideally your rally point).");
            return;
        }

        // Require a RallyPointTile and that it's empty. (No need to touch Tile.isPlayer here.)
        var rally = currentTile as RallyPointTile;
        if (rally == null)
        {
            Debug.Log("[BuyUnit] Selected tile is not a RallyPoint. Select your rally point and try again.");
            return;
        }
        if (rally.IsOccupied())
        {
            Debug.Log("[BuyUnit] Rally point is occupied. Clear it first.");
            return;
        }

        // If a cost is set, try to pay it through GameManager (if your branch supports it).
        if (cost > 0)
        {
            // If your GameManager has TrySpendGold, this will work.
            // If it doesn't, set cost to 0 in the Inspector and no spending check is performed.
            if (!GameManager.Instance.TrySpendGold(buyerTeam, cost))
            {
                Debug.Log("[BuyUnit] Not enough gold to buy this unit.");
                return;
            }
        }

        // Try to call UnitManager.CreateUnit using whatever signature exists in your branch.
        var created = TryCreateViaUnitManager(unitToBuy.UnitPrefab, rally);
        if (!created)
        {
            // As a last resort (and to avoid compile errors), spawn directly.
            SpawnLocally(unitToBuy.UnitPrefab, rally);
        }

        // Optional: refresh the right-hand tile panel if it's open
        if (TileInfoDisplayManager.Instance != null)
        {
            TileInfoDisplayManager.Instance.DisplayInfo(rally.TileInfo(), rally);
        }
    }

    /// <summary>
    /// Attempts to call UnitManager.CreateUnit(...) using reflection so we don't depend
    /// on a specific signature (branches differ).
    /// Returns true if some overload accepted the call.
    /// </summary>
    private bool TryCreateViaUnitManager(BaseUnit prefab, Tile tile)
    {
        var um = UnitManager.Instance;
        if (um == null) return false;

        var type = um.GetType();
        var methods = type.GetMethods(System.Reflection.BindingFlags.Instance |
                                      System.Reflection.BindingFlags.Public |
                                      System.Reflection.BindingFlags.NonPublic);

        // We try a few common shapes, without using named parameters.
        object[][] candidateArgLists = new object[][]
        {
            // (BaseUnit, Tile, Team, int)  -> some branches pass cost explicitly
            new object[] { prefab, tile, buyerTeam, cost },
            // (BaseUnit, Tile, Team)
            new object[] { prefab, tile, buyerTeam },
            // (BaseUnit, Tile, bool, Team, int) -> older branches with ignoreCost
            new object[] { prefab, tile, false, buyerTeam, cost },
            // (BaseUnit, Tile) -> simplest branch
            new object[] { prefab, tile }
        };

        foreach (var m in methods)
        {
            if (m.Name != "CreateUnit") continue;
            var parms = m.GetParameters();

            foreach (var args in candidateArgLists)
            {
                if (parms.Length != args.Length) continue;

                // quick shape check: each arg must be assignable to the parameter type
                bool shapeOK = true;
                for (int i = 0; i < parms.Length; i++)
                {
                    if (args[i] == null) { shapeOK = !parms[i].ParameterType.IsValueType; }
                    else shapeOK = parms[i].ParameterType.IsInstanceOfType(args[i]) ||
                                   // allow enum -> object boxing / int for cost, bool, etc.
                                   (args[i] is System.IConvertible &&
                                    System.Type.GetTypeCode(parms[i].ParameterType) != TypeCode.Object);
                    if (!shapeOK) break;
                }
                if (!shapeOK) continue;

                try
                {
                    var result = m.Invoke(um, args);
                    // If CreateUnit returns bool, respect it; otherwise assume success.
                    if (m.ReturnType == typeof(bool))
                        return (bool)result == true;

                    return true;
                }
                catch
                {
                    // Try next shape
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Fallback if no UnitManager.CreateUnit overload matched:
    /// spawn the unit directly and wire up basic tile ownership/links so the scene remains playable.
    /// </summary>
    private void SpawnLocally(BaseUnit prefab, Tile tile)
    {
        var spawned = Instantiate(prefab, tile.transform.position, Quaternion.identity);
        spawned.isPlayer = (buyerTeam == Team.Player);   // simple ownership flag
        spawned.OccupiedTile = tile;
        tile.ChangeStationed(spawned);

        Debug.Log("[BuyUnit] Spawned unit via local fallback (no matching CreateUnit overload).");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (button == null) button = GetComponent<Button>();
    }
#endif
}
