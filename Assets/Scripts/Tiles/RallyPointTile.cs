using UnityEditor.Rendering;
using UnityEngine;

public class RallyPointTile : Tile
{

    // Can be called from anywhere to give the location and owner of ralley point
    public string RallyPointOwnerLoc()
    {
        return this.x + " " + this.y + " " + isPlayer;
    }
    public void DestroyTile()
    {
        // Check if GridManager is initialized
        if (GridManager.Instance == null)
        {
            Debug.LogWarning("GridManager instance not found. Cannot destroy tile.");
            return;
        }

        // Check if coordinates are valid within the grid
        if (!GridManager.Instance.IsValidCoordinate(x, y))
        {
            Debug.LogWarning($"Invalid coordinates ({x}, {y}) when trying to destroy tile.");
            return;
        }

        // Replace with plains tile 
        GridManager.Instance.CreateTile(x, y, GridManager.Instance.plains, 0);

        Destroy(gameObject);
        }

    //clicking tile opens unit purchase interface (not implemented yet)
    public override string TileInfo()
    {
        return "This tile is where new units will be spawned";
    }
}