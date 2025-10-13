using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;

    [SerializeField] protected float _terrainModifier;
    [SerializeField] protected bool _isDeveloped;
    [SerializeField] public bool isPlayer;
    [SerializeField] protected int x;
    [SerializeField] protected int y;

    [Header("Highlight Settings")]
    [SerializeField] private GameObject _highlight;
    private SpriteRenderer highlightRenderer;
    private bool forceHighlight = false;

    public BaseUnit _unitStationed;

    public int X() => x;
    public int Y() => y;
    public bool IsOccupied() => _unitStationed != null;
    public bool IsDeveloped() => _isDeveloped;
    public float TerrainModifier() => _terrainModifier;
    public BaseUnit GetStationedUnit() => _unitStationed;

    // ---------------- Initialization ----------------
    private void Awake()
    {
        TryInitHighlight();
    }

    private void OnValidate()
    {
        // Helps auto-link the SpriteRenderer when editing prefabs
        if (_highlight != null && highlightRenderer == null)
            highlightRenderer = _highlight.GetComponent<SpriteRenderer>();
    }

    private void TryInitHighlight()
    {
        if (_highlight == null)
        {
            Debug.LogWarning($"[{name}] Tile._highlight is not assigned. Highlighting will be disabled.");
            return;
        }

        highlightRenderer = _highlight.GetComponent<SpriteRenderer>();
        if (highlightRenderer == null)
        {
            Debug.LogWarning($"[{name}] _highlight object has no SpriteRenderer. Color tinting won't work.");
        }

        _highlight.SetActive(false);
    }

    // ---------------- Core Logic ----------------
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
        if (GameManager.Instance.GameState != GameState.PlayerTurn)
            return;

        BaseUnit currentlySelectedUnit = UnitManager.Instance.SelectedUnit;

        if (IsOccupied())
        {
            if (_unitStationed.isPlayer)
            {
                UnitManager.Instance.SelectUnit(_unitStationed);
            }
            else if (currentlySelectedUnit != null)
            {
                var enemy = _unitStationed;
                if (!currentlySelectedUnit.AttemptAttack(enemy))
                    DisplayTileInfo();
                UnitManager.Instance.SelectUnit(null);
            }
            else
            {
                DisplayTileInfo();
            }
        }
        else if (currentlySelectedUnit != null)
        {
            if (!currentlySelectedUnit.AttemptMovement(this))
                DisplayTileInfo();
            UnitManager.Instance.SelectUnit(null);
        }
        else
        {
            DisplayTileInfo();
        }
    }

    // ---------------- Tile Info Display ----------------
    public virtual string TileInfo()
    {
        return $"{GetType().Name}\n+ {_terrainModifier}% Defense";
    }

    public void DisplayTileInfo()
    {
        if (TileInfoDisplayManager.Instance.CurrentTile == this)
        {
            TileInfoDisplayManager.Instance.HideInfo();
        }
        else
        {
            TileInfoDisplayManager.Instance.DisplayInfo(TileInfo(), this);
        }
    }

    // ---------------- Highlight Logic ----------------
    public void HighlightTile(Color color, bool force)
    {
        if (_highlight == null)
            return;

        if (highlightRenderer != null)
        {
            color.a = 0.25f;
            highlightRenderer.color = color;
        }

        if (force)
            forceHighlight = true;

        _highlight.SetActive(true);
    }

    public void UnhighlightTile()
    {
        forceHighlight = false;
        if (_highlight != null)
            _highlight.SetActive(false);
    }

    // ---------------- Mouse Input ----------------
    private void OnMouseDown()
    {
        EvaluateSelectedTile();
    }

    private void OnMouseEnter()
    {
        if (!forceHighlight)
            HighlightTile(Color.white, false);
    }

    private void OnMouseExit()
    {
        if (!forceHighlight)
            UnhighlightTile();
    }
}
