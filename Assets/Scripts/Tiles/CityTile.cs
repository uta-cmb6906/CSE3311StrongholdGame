using UnityEngine;

// CityTile produces gold for its current owner each turn.
// GameManager holds the actual gold totals; this tile just declares income.
public class CityTile : Tile, ICityIncome
{
    [Header("Economy")]
    [Tooltip("Gold this tile pays its owner at the start of a turn")]
    public int productionAmount = 10;

    // ---- ICityIncome (used by GameManager.PayoutTurnIncome) ----
    public Team Owner => isPlayer ? Team.Player : Team.Enemy;
    public int IncomePerTurn => productionAmount;

    //Add City to list held in game manager
    private void Start()
    {
        if (isPlayer)
            GameManager.Instance.playerCities.Add(this);
        else
            GameManager.Instance.enemyCities.Add(this);
    }

    //Remove City from list held in game manager
    private void OnDestroy()
    {
        if (isPlayer)
            GameManager.Instance.playerCities.Remove(this);
        else
            GameManager.Instance.enemyCities.Remove(this);
    }

    private void OnEnable()
    {
        GameManager.Instance?.RegisterIncomeTile(this);
    }

    private void OnDisable()
    {
        GameManager.Instance?.UnregisterIncomeTile(this);
    }

    // Toggle ownership (keeps your original behavior)
    public void ChangeUser()
    {
        isPlayer = !isPlayer;
        // TODO: update any ownership visuals here (color/flag/etc.)
    }

    // Optional convenience if you want to set by Team directly
    public void SetOwner(Team team)
    {
        isPlayer = (team == Team.Player);
    }

    // Backward compatibility: if old code still calls AddProduction,
    // forward it to the central wallet in GameManager.
    public void AddProduction(int amount)
    {
        GameManager.Instance?.AddGold(Owner, amount);
    }

    public override string TileInfo()
    {
        // Show income-per-turn and the owner's current wallet (from GameManager).
        int ownerGold = GameManager.Instance != null ? GameManager.Instance.GetGold(Owner) : 0;
        string ownerText = Owner == Team.Player ? "Player" : "Enemy";

        return $"{GetType().Name}\n+ {_terrainModifier}% Defense\n" +
               $"+{productionAmount} Gold/turn\n" +
               $"Owner: {ownerText}\n" +
               $"Owner Gold: {ownerGold}";
    }
}
