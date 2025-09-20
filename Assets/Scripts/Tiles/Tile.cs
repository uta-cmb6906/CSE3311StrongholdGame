using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;

    [SerializeField] protected float _terrainModifier;
    [SerializeField] protected bool _isDeveloped;
    public BaseUnit _unitStationed;

    public bool IsOccupied() => _unitStationed != null;
    public bool IsDeveloped() => _isDeveloped;
    public float TerrainModifier() => _terrainModifier;
    public BaseUnit GetStationedUnit() => _unitStationed;

    public void ChangeStationed(BaseUnit newUnit)
    {
        _unitStationed = newUnit;
    }

    void OnMouseDown()
    {
        EvaluateSelectedTile();
    }

    private void EvaluateSelectedTile()
    {
        //ignore if not player turn
        if (GameManager.Instance.GameState != GameState.PlayerTurn) return;

        //get currently selected unit if any
        BaseUnit currentlySelectedUnit = UnitManager.Instance.SelectedUnit;

        //if unit on tile
        if (IsOccupied())
        {
            //if player unit on tile select it
            if (_unitStationed.isPlayer) UnitManager.Instance.SelectUnit(_unitStationed);

            //if tile has an enemy unit and player has already selcted a unit then initiate attack
            else if (currentlySelectedUnit != null)
            {
                var enemy = _unitStationed;
                
                //if enemy unit out of range just display tile info
                if (!currentlySelectedUnit.AttemptAttack(enemy)) DisplayTileInfo();

                UnitManager.Instance.SelectUnit(null);
            }

            //if player has no selected unit just display tile info
            else DisplayTileInfo();
        }

        //if empty tile and player has a selected unit then evaluate movement to tile
        else if (currentlySelectedUnit != null)
        {
            //if tile outside of movement range just display tile info
            if (!currentlySelectedUnit.AttemptMovement(this)) DisplayTileInfo();

            UnitManager.Instance.SelectUnit(null);
        }

        else DisplayTileInfo();
    }

    public void DisplayTileInfo()
    {
        ;
    }
}