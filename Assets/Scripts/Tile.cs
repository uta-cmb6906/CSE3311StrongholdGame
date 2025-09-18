using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;
    [SerializeField] private GameObject forest;
    [SerializeField] private GameObject river;
    [SerializeField] private GameObject mountain;
    [SerializeField] private GameObject city;
    [SerializeField] private GameObject rallyPoint;

    private float _terrainModifier = 1;
    private bool _isDeveloped = false;
    private BaseUnit _unitStationed;

    public void Specialize(string terrain)
    {
        switch (terrain)
        {
            case "forest":
                _terrainModifier = 1.25f;
                forest.SetActive(true);
                break;
            case "river":
                _terrainModifier = 1.5f;
                river.SetActive(true);
                break;
            case "mountain":
                _terrainModifier = 2f;
                mountain.SetActive(true);
                break;
            case "city":
                _terrainModifier = 1.5f;
                city.SetActive(true);
                _isDeveloped = true;
                break;
            case "rallyPoint":
                _terrainModifier = 1f;
                rallyPoint.SetActive(true);
                _isDeveloped = true;
                break;
        }
    }

    public void Highlight(bool active)
    {
        _highlight.SetActive(active);
    }

    public void ChangeStationed(BaseUnit newUnit)
    {
        _unitStationed = newUnit;
    }

    public bool IsOccupied() => _unitStationed != null;
    public bool IsDeveloped() => _isDeveloped;
    public float TerrainModifier() => _terrainModifier;
    public BaseUnit GetStationedUnit() => _unitStationed;

    void OnMouseOver()
    {
        Highlight(true);
        Debug.Log("enter");
    }

    void OnMouseExit()
    {
        Highlight(false);
    }
}