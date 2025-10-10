using UnityEngine;

public class CityTile : Tile
{

    public int productionAmount, totalGoldPlayer, totalGoldEnemy;

    public void AddProduction(int productionAmount)
    {
        if (isPlayer)
            totalGoldPlayer += productionAmount;
        else
            totalGoldEnemy += productionAmount;
    }

    public void ChangeUser()
    {
        if (isPlayer)
            isPlayer = false;
        else
            isPlayer = true;
    }

    public override string TileInfo()
    {
        if (isPlayer)
            return this.GetType().Name + "\n+ " + _terrainModifier + "% Defense\n" + totalGoldPlayer + " Gold\n" + "Owned by: "; // need to define faction
        else
            return this.GetType().Name + "\n+ " + _terrainModifier + "% Defense\n" + totalGoldEnemy + " Gold\n" + "Owned by: "; // need to define faction
    }


}