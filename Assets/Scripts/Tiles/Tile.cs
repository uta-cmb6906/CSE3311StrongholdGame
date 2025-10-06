using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;

    [SerializeField] protected float _terrainModifier;
    [SerializeField] protected bool _isDeveloped;
    [SerializeField] protected int x;
    [SerializeField] protected int y;

    [SerializeField] private GameObject _highlight;
    SpriteRenderer highlightRenderer;
    private bool forceHighlight = false;
    public BaseUnit _unitStationed;

    public int X() => x;
    public int Y() => y;
    public bool IsOccupied() => _unitStationed != null;
    public bool IsDeveloped() => _isDeveloped;
    public float TerrainModifier() => _terrainModifier;
    public BaseUnit GetStationedUnit() => _unitStationed;

    void Start()
    {
        highlightRenderer = _highlight.GetComponent<SpriteRenderer>();
    }

    public void SetCoords(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void ChangeStationed(BaseUnit newUnit)
    {
        _unitStationed = newUnit;
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

    public string TileInfo()
    {
        return this.GetType().Name + "\n+ " + _terrainModifier + "% Defense";
    }

    public void DisplayTileInfo()
    {
        ;
    }

    //highlight the tile the specified color, and if force is true prevent OnMouseExit from removing highlight
    public void HighlightTile(Color color, bool force)
    {
        color.a = .25f;
        highlightRenderer.color = color;
        if (force) forceHighlight = true;
        _highlight.SetActive(true);
    }

    public void UnhighlightTile()
    {
        forceHighlight = false;
        _highlight.SetActive(false);
    }

    void OnMouseDown()
    {
        EvaluateSelectedTile();
    }

    void OnMouseEnter()
    {
        if(!forceHighlight) HighlightTile(Color.white, false);
    }

    void OnMouseExit()
    {
        if(!forceHighlight) UnhighlightTile();
    }
}