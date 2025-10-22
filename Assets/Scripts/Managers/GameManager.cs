using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static string MapToLoad = "MapData";
    public GameState GameState;

    // Make a list of player cities/units
    public List<CityTile> playerCities = new List<CityTile>();
    public List<BaseUnit> playerUnits = new List<BaseUnit>();
    // Make list of enemy cities/units
    public List<CityTile> enemyCities = new List<CityTile>();
    public List<BaseUnit> enemyUnits = new List<BaseUnit>();
    // Store Rally Points
    public RallyPointTile playerRallyPoint;
    public RallyPointTile enemyRallyPoint;

    // Store selected plains tile
    public PlainsTile selectedPlainsTile;

    //needed prefabs
    [Header("Necessary PreFabs")]
    [SerializeField] private BaseUnit playerCastle;

    //end turn message
    [Header("UI References")]
    [SerializeField] private GameObject endTurnMessagePanel;

    //end turn message
    [Header("Difficulty")]
    [SerializeField] public int difficulty;

    public static event Action<GameState> OnGameStateChanged;

    // ===== ECONOMY: BEGIN =====
    public static event Action<Team, int> OnGoldChanged;  // (who, newGold)

    [Header("Economy / Starting Gold")]
    [SerializeField] private int startingGold;

    [Header("Economy / Costs")]
    [SerializeField] public int CastleCost = 150;
    [SerializeField] public int HealCost = 25;
    [SerializeField] public int UpgradeCost = 75;

    [SerializeField] private int _playerGold;
    [SerializeField] private int _enemyGold;

    public int GetGold(Team who) => who == Team.Player ? _playerGold : _enemyGold;

    private void InitializeGold()
    {
        _playerGold = startingGold;
        _enemyGold = startingGold;
    }

    private void AddPlayerTurnGold()
    {
        foreach (CityTile city in playerCities) _playerGold += city.Production();
        OnGoldChanged?.Invoke(Team.Player, _playerGold);
    }

    private void AddEnemyTurnGold()
    {
        foreach (CityTile city in enemyCities) _enemyGold += city.Production();
    }

    public bool TrySpendGold(Team who, int cost)
    {
        if (cost < 0) return false;
        if (who == Team.Player)
        {
            if (_playerGold < cost) return false;
            _playerGold -= cost;
            OnGoldChanged?.Invoke(who, _playerGold);
            return true;
        }
        else
        {
            if (_enemyGold < cost) return false;
            _enemyGold -= cost;
            OnGoldChanged?.Invoke(who, _enemyGold);
            return true;
        }
    }

    //player tries to purchase a unit
    public bool TryPurchaseUnit(BaseUnit unit)
    {
        //rally point needs to exist
        if (!playerRallyPoint)
        {
            //TODO error pop-up
            return false;
        }

        //rally point needs to be unoccupied
        if (playerRallyPoint.IsOccupied())
        {
            //TODO error pop-up
            return false;
        }

        //check if unit affordable
        if (!TrySpendGold(Team.Player, unit.Cost()))
        {
            //TODO error pop-up
            return false;
        }

        CreatePurchasedUnit(Team.Player, unit);
        return true;
    }

    public bool TryUpgrade()
    {
        //if not player turn ignore
        if (GameState != GameState.PlayerTurn) return false;

        //check if unit selected
        BaseUnit currentlySelectedUnit = UnitManager.Instance.SelectedUnit;
        if (!currentlySelectedUnit) return false;

        // spend from the correct wallet based on isPlayer
        if (!TrySpendGold(Team.Player, UpgradeCost))
        {
            Debug.Log($"Not enough gold to upgrade (cost {UpgradeCost}).");
            return false;
        }

        //upgrade unit and rehighlight available units
        currentlySelectedUnit.UpgradeUnit();
        UnitManager.Instance.SelectUnit(null);
        HighlightAvailableUnits();
        return true;
    }

    public bool TryHeal()
    {
        //if not player turn ignore
        if (GameState != GameState.PlayerTurn) return false;

        //check if unit selected
        BaseUnit currentlySelectedUnit = UnitManager.Instance.SelectedUnit;
        if (!currentlySelectedUnit) return false;

        // spend from the correct wallet based on isPlayer
        if (!TrySpendGold(Team.Player, HealCost))
        {
            Debug.Log($"Not enough gold to heal (cost {HealCost}).");
            return false;
        }

        //upgrade unit and rehighlight available units
        currentlySelectedUnit.HealUnit();
        UnitManager.Instance.SelectUnit(null);
        HighlightAvailableUnits();
        return true;
    }

    private void CreatePurchasedUnit(Team team, BaseUnit unit)
    {
        if (team == Team.Player) UnitManager.Instance.CreateUnit(unit, playerRallyPoint, true);
        else UnitManager.Instance.CreateUnit(unit, playerRallyPoint, false);
    }

    // Try to buy a castle on a given tile (prefab type kept generic)
    public bool TryBuyCastle()
    {
        //if not player turn ignore
        if (GameState != GameState.PlayerTurn) return false;

        //check if plains tile selected and unoccupied
        if (!selectedPlainsTile || selectedPlainsTile.IsOccupied()) return false;

        // spend from the correct wallet based on isPlayer
        if (!TrySpendGold(Team.Player, CastleCost))
        {
            Debug.Log($"Not enough gold to build castle (cost {CastleCost}).");
            return false;
        }

        //build castle
        UnitManager.Instance.CreateUnit(playerCastle, selectedPlainsTile, true);
        HighlightAvailableUnits();
        return true;
    }
    // ===== ECONOMY: END =====
    public void HighlightAvailableUnits()
    {
        foreach (BaseUnit unit in playerUnits)
        {
            if (unit.ActionpointRemaining()) unit.OccupiedTile.HighlightTile(Color.yellow, true);
        }
    }

    private void UnhighlightAvailableUnits()
    {
        foreach (BaseUnit unit in playerUnits) unit.OccupiedTile.UnhighlightTile();
    }

    private void ResetPlayerUnitActionPoints()
    {
        foreach (BaseUnit unit in playerUnits) unit.ResetAction();
    }

    public void EndPlayerTurn()
    {
        // if we are currently in the PlayerTurn state
        if (GameState == GameState.PlayerTurn)
        {
            //show that my turn has ended gracefully :)
            if (endTurnMessagePanel != null)
            {
                endTurnMessagePanel.SetActive(true);
            }

            UnhighlightAvailableUnits();
            ChangeState(GameState.EnemyTurn);
        }
    }

    //ensure all scenes are using the same instance of the manager
    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        // ===== ECONOMY: initialize wallets =====
        InitializeGold();

        //hidden endturn message
        if (endTurnMessagePanel != null)
        {
            endTurnMessagePanel.SetActive(false);
        }

        // Notify UI of starting gold
        //Debug log
        Debug.Log($"[GM] Start gold: P={_playerGold}, E={_enemyGold}");
        OnGoldChanged?.Invoke(Team.Player, _playerGold);
        OnGoldChanged?.Invoke(Team.Enemy, _enemyGold);

    }

    //start on state generate Grid
    void Start()
    {
        //TODO eventually start at start menu
        ChangeState(GameState.GenerateGrid);
    }

    //change game state (initiating state logic), and broadcast state change event
    public void ChangeState(GameState newState)
    {
        GameState = newState;
        switch (newState)
        {
            case GameState.GenerateGrid:
                GridManager.Instance.GenerateGrid(MapToLoad);
                break;
            case GameState.SpawnFactions:
                UnitManager.Instance.SpawnFactions("examplePlayer", "exampleEnemy");
                break;
            case GameState.PlayerTurn:
                Debug.Log("[GM] -> PlayerTurn");
                AddPlayerTurnGold();
                ResetPlayerUnitActionPoints();
                HighlightAvailableUnits();


                break;
            case GameState.EnemyTurn:
                Debug.Log("[GM] -> EnemyTurn");
                AddEnemyTurnGold();
                StartCoroutine(WaitThenEndEnemyTurn(2.0f));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnGameStateChanged?.Invoke(newState);
    }

    //enemy turn
    private IEnumerator WaitThenEndEnemyTurn(float delay)
    {
        // Placeholder for AI work
        yield return new WaitForSeconds(delay);

        //hide message again after 2s
        if (endTurnMessagePanel != null)
        {
            endTurnMessagePanel.SetActive(false);
        }

        // After delay, switch back to player turn, triggering PayoutTurnIncome() again
        ChangeState(GameState.PlayerTurn);
    }
}

public enum GameState
{
    GenerateGrid,
    SpawnFactions,
    PlayerTurn,
    EnemyTurn

}



