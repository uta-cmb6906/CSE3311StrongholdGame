using UnityEngine;

// CityTile has a controller (player or enemy) and a production value added to the controllers economy each turn
public class CityTile : Tile
{
    private int production = 10;

    [Header("Cities Sprites")]
    [SerializeField] private Sprite playerCity;
    [SerializeField] private Sprite enemyCity;

    private SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        base.Start();

        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        //use correct graphic
        spriteRenderer.sprite = (isPlayer) ? playerCity : enemyCity;

        //update list in GameManager
        if (isPlayer) GameManager.Instance.playerCities.Add(this);
        else GameManager.Instance.enemyCities.Add(this);
    }

    public int Production() => production;

    //if new occupation unit is from other team switch owner
    public override void ChangeStationed(BaseUnit newUnit)
    {
        _unitStationed = newUnit;

        //if new unit not owner's unit
        if (_unitStationed && _unitStationed.IsPlayer() != isPlayer) SwapOwner();
    }

    // Change ownership of city and show on map
    private void SwapOwner()
    {
        isPlayer = !isPlayer;

        // update GameManager's city lists
        if (isPlayer)
        {
            GameManager.Instance.playerCities.Add(this);
            GameManager.Instance.enemyCities.Remove(this);
        }
        else
        {
            GameManager.Instance.playerCities.Remove(this);
            GameManager.Instance.enemyCities.Add(this);
        }

        spriteRenderer.sprite = (isPlayer) ? playerCity : enemyCity;
    }

    public override string TileInfo()
    {
        string ownerText = (isPlayer) ? "Player" : "Enemy";
        return base.TileInfo() +
               $"+{production} Gold/turn\n" +
               $"Owner: {ownerText}\n";
    }
}
