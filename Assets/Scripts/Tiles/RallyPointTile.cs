using UnityEngine;

public class RallyPointTile : Tile
{
    [Header("Plains Tiles")]
    [SerializeField] private Sprite plainsTile;
    [SerializeField] private Material plainsMaterial;
    private SpriteRenderer spriteRenderer;

    [Header("Rally Point Sprites")]
    [SerializeField] private Sprite playerRallyPoint;

    [SerializeField] private Sprite enemyRallyPoint;

    protected override void Start()
    {
        base.Start();

        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        //use correct flag color
        spriteRenderer.sprite = (isPlayer) ? playerRallyPoint : enemyRallyPoint;

        //update stored rally points in GameManager
        if (isPlayer) GameManager.Instance.playerRallyPoint = this;
        else GameManager.Instance.enemyRallyPoint = this;
    }

    //if new occupation unit is from other team convert rally point to plains tile
    public override void ChangeStationed(BaseUnit newUnit)
    {
        _unitStationed = newUnit;
        //if new unit not owner's unit
        if (_unitStationed && _unitStationed.IsPlayer() != isPlayer) ConvertToPlains();
    }

    private void ConvertToPlains()
    {
        //remove from gamemanager
        if (isPlayer) GameManager.Instance.playerRallyPoint = null;
        else GameManager.Instance.enemyRallyPoint = null;

        //replicate plains attributes and visuals
        _isDeveloped = false;
        isPlayer = false;
        spriteRenderer.sprite = plainsTile;
        spriteRenderer.material = plainsMaterial;
    }

    //clicking tile opens unit purchase interface (not implemented yet)
    public override string TileInfo()
    {
        if (IsDeveloped())
        {
            return "This tile is where new units will be spawned";
        }
        else
        {
            return $"PlainsTile\n+ {_terrainModifier * 100 - 100}% Defense";
        }
    }
}