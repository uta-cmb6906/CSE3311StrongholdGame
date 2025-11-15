using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static string MapToLoad = "RandomMap";
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
    public static int GlobalDifficulty = 0;

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

    [Header("Enemy Economy")]
    [SerializeField] private List<BaseUnit> enemyRecruitables = new List<BaseUnit>(); // prefabs the enemy can buy
    [SerializeField] private int enemyMaxPurchasesPerTurn = 2;
    [SerializeField] private int enemyMaxUpgradesPerTurn = 1;
    [SerializeField] private float enemyEconomyDelay = 0.15f; // pacing between actions


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

    // private void CreatePurchasedUnit(Team team, BaseUnit unit)
    // {
    //     // if (team == Team.Player) UnitManager.Instance.CreateUnit(unit, playerRallyPoint, true);
    //     // else UnitManager.Instance.CreateUnit(unit, playerRallyPoint, false);
    //     bool isPlayer = (team == Team.Player);

    //     // keep old behavior as fallback
    //     Tile spawnTile = playerRallyPoint;
    //     if (!isPlayer && enemyRallyPoint != null)
    //         spawnTile = enemyRallyPoint;

    //     if (spawnTile == null || spawnTile.IsOccupied())
    //     {
    //         Debug.Log($"[GM] Spawn aborted: {(isPlayer ? "player" : "enemy")} rally unavailable/occupied.");
    //         return;
    //     }

    //     UnitManager.Instance.CreateUnit(unit, spawnTile, isPlayer);
    // }
    private void CreatePurchasedUnit(Team team, BaseUnit unit)
    {
        bool isPlayer = (team == Team.Player);
        Tile rally = isPlayer ? (Tile)playerRallyPoint : (Tile)enemyRallyPoint;

        if (rally == null)
        {
            Debug.Log("[GM] Spawn aborted: rally point missing.");
            return;
        }

        // Find a free tile: prefer around rally, fall back to rally itself if empty
        Tile spawnTile = FindFreeSpawnTileNear(rally);
        if (spawnTile == null)
        {
            Debug.Log("[GM] Spawn aborted: No free tile near rally.");
            return;
        }

        UnitManager.Instance.CreateUnit(unit, spawnTile, isPlayer);

        // Optional: ensure it can act now (esp. if CreateUnit marks it as used)
        if (isPlayer && playerUnits.Count > 0)
        {
            var spawned = playerUnits[playerUnits.Count - 1];
            if (spawned != null)
            {
                spawned.ResetAction();
                UnitManager.Instance.SelectUnit(spawned);
                HighlightAvailableUnits();
            }
        }
    }

    // Looks in a 1-tile ring around center; if none, uses center if empty
    private Tile FindFreeSpawnTileNear(Tile center)
    {
        // 8-neighborhood around the rally (you can expand radius if you want)
        int cx = center.X();
        int cy = center.Y();

        // First, try adjacent tiles
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var t = GridManager.Instance.GetTileAtPosition(new Vector3(cx + dx, cy + dy));
                if (t != null && !t.IsOccupied())
                    return t;
            }
        }

        // Fallback: allow spawning on rally itself if it’s free
        if (!center.IsOccupied()) return center;

        return null;
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

    private IEnumerator EnemyEconomyPhase()
    {
        Debug.Log("[GM] EnemyEconomyPhase starting...");


        // ---------- BUY PHASE ----------
        int purchases = 0;
        while (purchases < enemyMaxPurchasesPerTurn)
        {
            // need a rally point and it must be free
            if (enemyRallyPoint == null || enemyRallyPoint.IsOccupied())
            {
                Debug.Log("[GM] EnemyEconomy: rally point null or occupied — skipping buy phase.");
                break;
            }

            int gold = GetGold(Team.Enemy);
            BaseUnit pick = null;
            int pickCost = -1;

            // // Random: choose a random affordable unit instead of always the most expensive
            // var affordable = new List<BaseUnit>();
            // foreach (var prefab in enemyRecruitables)
            // {
            //     if (prefab == null) continue;
            //     int cost = Mathf.Max(0, prefab.Cost());
            //     if (cost <= gold)
            //         affordable.Add(prefab);
            // }

            // if (affordable.Count == 0)
            // {
            //     Debug.Log("[GM] EnemyEconomy: no affordable units found.");
            //     break;
            // }

            // Pick a random one
            // BaseUnit pick = affordable[UnityEngine.Random.Range(0, affordable.Count)];
            // int pickCost = Mathf.Max(0, pick.Cost());

            // Greedy: most expensive affordable unit
            foreach (var prefab in enemyRecruitables)
            {
                if (prefab == null) continue;
                int cost = Mathf.Max(0, prefab.Cost());
                if (cost <= gold && cost >= pickCost)
                {
                    pick = prefab;
                    pickCost = cost;
                }
            }

            if (pick == null)
            {
                Debug.Log("[GM] EnemyEconomy: no affordable units found.");
                break;
            }

            if (TrySpendGold(Team.Enemy, pickCost))
            {
                Debug.Log($"[GM] EnemyEconomy: buying {pick.name} for {pickCost} gold (remaining: {GetGold(Team.Enemy)})");
                CreatePurchasedUnit(Team.Enemy, pick);
                purchases++;
                yield return new WaitForSeconds(enemyEconomyDelay);
            }
            else
            {
                Debug.Log("[GM] EnemyEconomy: insufficient gold after check — aborting purchases.");
                break; // wallet changed since choosing
            }
        }

        // ---------- UPGRADE PHASE ----------
        int upgrades = 0;
        var snapshot = new List<BaseUnit>(enemyUnits); // copy in case list mutates
        foreach (var u in snapshot)
        {
            if (u == null) continue;
            if (upgrades >= enemyMaxUpgradesPerTurn) break;

            if (!TrySpendGold(Team.Enemy, UpgradeCost))
            {
                Debug.Log("[GM] EnemyEconomy: insufficient gold to upgrade — skipping.");
                break;
            }

            u.UpgradeUnit();
            upgrades++;
            Debug.Log($"[GM] EnemyEconomy: upgraded {u.name} (remaining gold: {GetGold(Team.Enemy)})");
            yield return new WaitForSeconds(enemyEconomyDelay);
        }
        Debug.Log("[GM] EnemyEconomyPhase complete.");
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

    private float enemyUnitActions(BaseUnit unit)
    {
        if (!unit.ActionpointRemaining()) return 1.0f;

        // Check if any player unit is in attack range
        BaseUnit targetInRange = null;
        foreach (BaseUnit p in playerUnits)
        {
            if (p == null) continue;
            int dx = Mathf.Abs(unit.OccupiedTile.X() - p.OccupiedTile.X());
            int dy = Mathf.Abs(unit.OccupiedTile.Y() - p.OccupiedTile.Y());
            if (dx <= unit.AttackRange() && dy <= unit.AttackRange())
            {
                targetInRange = p;
                break;
            }
        }

        if (targetInRange != null)
        {
            unit.AttemptAttack(targetInRange);
            Debug.Log("Attack Test");
            return 1.0f;
        }

        // No attack possible → choose movement behavior
        int decision = UnityEngine.Random.Range(1, 4);

        Tile targetTile = null;

        // helper function to find nearest tile to a list of player units
        Tile FindClosestTileTo()
        {
            Tile best = null;
            float bestDistSq = float.MaxValue;
            foreach (BaseUnit p in playerUnits)
            {
                if (p == null) continue;
                int dx = unit.OccupiedTile.X() - p.OccupiedTile.X();
                int dy = unit.OccupiedTile.Y() - p.OccupiedTile.Y();
                float distSq = dx * dx + dy * dy;
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    best = p.OccupiedTile;
                }
            }
            return best;
        }

        // helper for finding nearest player city
        Tile FindClosestCityTile()
        {
            Tile best = null;
            float bestDistSq = float.MaxValue;
            foreach (CityTile c in playerCities)
            {
                if (c == null) continue;
                int dx = unit.OccupiedTile.X() - c.X();
                int dy = unit.OccupiedTile.Y() - c.Y();
                float distSq = dx * dx + dy * dy;
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    best = c;
                }
            }
            return best;
        }

        // Determine target destination
        switch (decision)
        {
            case 1: // Move toward nearest player unit
                targetTile = FindClosestTileTo();
                break;

            case 2: // Move toward nearest player city
                targetTile = FindClosestCityTile();
                break;

            case 3: // Move toward player rally point
                if (playerRallyPoint != null) targetTile = playerRallyPoint;
                break;
        }

        if (targetTile == null) return 1.0f; // nothing to move toward

        // Pick a tile closer to target within movement range
        Tile bestTile = unit.OccupiedTile;
        float bestDistSqToTarget = (bestTile.X() - targetTile.X()) * (bestTile.X() - targetTile.X()) + (bestTile.Y() - targetTile.Y()) * (bestTile.Y() - targetTile.Y());
        int r = unit.MovementRange();
        // search in a square area within movement range
        for (int x = unit.OccupiedTile.X() - r; x <= unit.OccupiedTile.X() + r; x++)
        {
            for (int y = unit.OccupiedTile.Y() - r; y <= unit.OccupiedTile.Y() + r; y++)
            {
                Tile candidate = GridManager.Instance.GetTileAtPosition(new Vector3(x, y));
                if (candidate == null) continue;
                if (candidate.IsOccupied()) continue;

                // ensure tile is actually in movement square (matches BaseUnit.HighlightValidTiles logic)
                int dx = Mathf.Abs(unit.OccupiedTile.X() - x);
                int dy = Mathf.Abs(unit.OccupiedTile.Y() - y);
                if (dx > r || dy > r) continue;

                float distSq = (candidate.X() - targetTile.X()) * (candidate.X() - targetTile.X()) +
                               (candidate.Y() - targetTile.Y()) * (candidate.Y() - targetTile.Y());

                if (distSq < bestDistSqToTarget)
                {
                    bestDistSqToTarget = distSq;
                    bestTile = candidate;
                }
            }
        }

        if (bestTile != unit.OccupiedTile) unit.AttemptMovement(bestTile);
        return 1.0f;
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

        //difficulty field
        difficulty = GlobalDifficulty;

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
                StartCoroutine(EnemyTurn());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnGameStateChanged?.Invoke(newState);
    }

    //enemy turn
    private IEnumerator EnemyTurn()
    {
        foreach (BaseUnit player in playerUnits)
        {
            player.DeleteAction();
        }
        if (endTurnMessagePanel.activeSelf)
        {
            yield return new WaitForSeconds(2.0f);
            endTurnMessagePanel.SetActive(false);
        }

        // Spend the gold that was added in ChangeState -> EnemyTurn
        yield return StartCoroutine(EnemyEconomyPhase());

        var currentEnemies = new List<BaseUnit>(enemyUnits);
        foreach (BaseUnit enemy in currentEnemies)
        {
            if (enemy == null) continue;

            enemy.ResetAction();   // ✅ resets before acting
            yield return new WaitForSeconds(enemyUnitActions(enemy));
        }


        // After delay, switch back to player turn
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









