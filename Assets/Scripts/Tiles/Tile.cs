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

    public void ChangeStationed(BaseUnit newUnit)
    {
        _unitStationed = newUnit;
    }

    public bool IsOccupied() => _unitStationed != null;
    public bool IsDeveloped() => _isDeveloped;
    public float TerrainModifier() => _terrainModifier;
    public BaseUnit GetStationedUnit() => _unitStationed;

    void OnMouseDown()
    {
        Debug.Log("...");
        if (GameManager.Instance.GameState != GameState.PlayerTurn) return;

        if (IsOccupied())
        {
            if (_unitStationed.isPlayer) UnitManager.Instance.SelectUnit(_unitStationed);
            else if (UnitManager.Instance.SelectedUnit != null)
            {
                var enemy = _unitStationed;
                UnitManager.Instance.SelectedUnit.Attack(enemy);
                UnitManager.Instance.SelectUnit(null);
            }
            //else display tile info
        }
        else if (UnitManager.Instance.SelectedUnit != null)
        {
            UnitManager.Instance.SelectedUnit.Move(this);
        }
        //else display tile info
    }
}