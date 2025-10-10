using UnityEditor.Rendering;
using UnityEngine;

public class RallyPointTile : Tile
{

    public string RallyPointOwnerLoc()
    {
        return this.x + " " + this.y + " " + isPlayer;
    }
    public void DestroyTile()
    {
        GridManager.Instance.CreateTile(this.x, this.y, GridManager.Instance.plains, 0);
        Destroy(gameObject);
    }

    //clicking tile opens unit purchase interface (not implemented yet)
    public override string TileInfo()
    {
        return "This tile is where new units will be spawned";
    }
}