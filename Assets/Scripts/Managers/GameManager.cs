using System;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState GameState;

    public static event Action<GameState> OnGameStateChanged;

    // ===== ECONOMY: BEGIN =====
    public static event Action<Team, int> OnGoldChanged;  // (who, newGold)

    [Header("Economy / Starting Gold")]
    [SerializeField] private int playerStartingGold = 200;
    [SerializeField] private int enemyStartingGold  = 200;

    [Header("Economy / Costs")]
    public int CastleCost = 150;
    public int HealCost   = 25;
    public int UpgradeCost = 75;

    private int _playerGold;
    private int _enemyGold;

    // city income tiles register here (optional but useful)
    private readonly List<ICityIncome> _incomeTiles = new();

    public int GetGold(Team who) => who == Team.Player ? _playerGold : _enemyGold;

    public void AddGold(Team who, int amount)
    {
        if (who == Team.Player) _playerGold += amount; else _enemyGold += amount;
        OnGoldChanged?.Invoke(who, GetGold(who));
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

    // Register/unregister per-turn income providers (city tiles)
    public void RegisterIncomeTile(ICityIncome t)
    {
        if (!_incomeTiles.Contains(t)) _incomeTiles.Add(t);
    }
    public void UnregisterIncomeTile(ICityIncome t) => _incomeTiles.Remove(t);

    // Pay income to whoever owns tiles; call this at start of each turn.
    public void PayoutTurnIncome()
    {
        Debug.Log("[GM] Paying out turn income…");    
        foreach (var t in _incomeTiles)
        {
            Debug.Log($"[GM] {t} pays {t.IncomePerTurn} to {t.Owner}");
            if (t.IncomePerTurn <= 0) continue;
            AddGold(t.Owner, t.IncomePerTurn);
        }
    }

    // Try to buy a castle on a given tile (prefab type kept generic)
    public bool TryBuyCastle(Team buyer, GameObject castlePrefab, Tile tile, Transform parent = null)
    {
        if (tile == null || castlePrefab == null) return false;
        // basic buildability check: don't place where a unit already sits
        if (tile._unitStationed != null) return false;

        if (!TrySpendGold(buyer, CastleCost)) return false;

        var go = Instantiate(castlePrefab, tile.transform.position, Quaternion.identity, parent);
        // If your castle is a BaseUnit, you *may* want to mark it on the tile:
        // var baseUnit = go.GetComponent<BaseUnit>();
        // if (baseUnit != null) { tile._unitStationed = baseUnit; baseUnit.OccupiedTile = tile; }

        return true;
    }
    // ===== ECONOMY: END =====

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
        _playerGold = playerStartingGold;
        _enemyGold  = enemyStartingGold;

        // Notify UI of starting gold
        //Debug log
        Debug.Log($"[GM] Start gold: P={_playerGold}, E={_enemyGold}");
        OnGoldChanged?.Invoke(Team.Player, _playerGold);
        OnGoldChanged?.Invoke(Team.Enemy,  _enemyGold);
        
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
                GridManager.Instance.GenerateGrid("MapData");
                break;
            case GameState.SpawnFactions:
                UnitManager.Instance.SpawnFactions("examplePlayer", "exampleEnemy");
                break;
            case GameState.PlayerTurn:
                Debug.Log("[GM] -> PlayerTurn");
                PayoutTurnIncome();
                break;
            case GameState.EnemyTurn:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnGameStateChanged?.Invoke(newState);
    }
}

public enum GameState
{
    GenerateGrid,
    SpawnFactions,
    PlayerTurn,
    EnemyTurn

}

// ===== ECONOMY: simple interface a city tile can implement =====
public interface ICityIncome
{
    Team Owner { get; }
    int IncomePerTurn { get; }
}



