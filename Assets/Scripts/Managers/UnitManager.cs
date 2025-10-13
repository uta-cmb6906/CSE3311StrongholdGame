using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;
    private List<ScriptableUnit> _units;
    public BaseUnit SelectedUnit;
    [SerializeField] private UnitStat unitStatUI;

    //ensure all scenes are using the same instance of the manager
    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        _units = Resources.LoadAll<ScriptableUnit>("Units").ToList();
    }

    //Spawn all initial player and enemy units
    public void SpawnFactions(string playerFaction, string enemyFaction)
    {
        SpawnFactionUnits(playerFaction, Team.Player);
        SpawnFactionUnits(enemyFaction, Team.Enemy);
        GameManager.Instance.ChangeState(GameState.PlayerTurn);
    }

    //use provided faction to spawn its units
    public void SpawnFactionUnits(string faction, Team team)
    {
        // find specified faction csv file
        TextAsset csvFile = Resources.Load<TextAsset>("Factions/" + faction);
        string[] lines = csvFile.text.Split('\n');

        // each line in faction.csv holds x and y coordinates of a unit and its type (x y type)
        foreach (var raw in lines)
        {
            var line = raw.Replace("\n", "").Replace("\r", "");
            if (string.IsNullOrWhiteSpace(line)) continue;

            var unitData = line.Split(' ');

            Vector2 position = new Vector2(float.Parse(unitData[0]), float.Parse(unitData[1]));
            Tile spawnTile = GridManager.Instance.GetTileAtPosition(position);

            // find the ScriptableUnit for this team
            var su = _units.Find(u => u.name == unitData[2] && u.Team == team);
            if (su == null)
            {
                Debug.LogWarning($"ScriptableUnit not found for '{unitData[2]}' ({team})");
                continue;
            }

            CreateUnit(su.UnitPrefab, spawnTile, team == Team.Player);
        }
    }

    public void CreateUnit(BaseUnit unit, Tile spawnTile, bool isPlayer)
    {
        var spawnedUnit = Instantiate(unit);
        spawnTile.ChangeStationed(spawnedUnit);
        spawnedUnit.OccupiedTile = spawnTile;
        spawnedUnit.transform.position = spawnTile.transform.position;
        
        //update stored units in GameManager
        if (isPlayer) GameManager.Instance.playerUnits.Add(spawnedUnit);
        else GameManager.Instance.enemyUnits.Add(spawnedUnit);
    }

    public void SelectUnit(BaseUnit unit)
    {
        //unhighlight valid tiles for previously selected unit
        if (SelectedUnit) SelectedUnit.UnhighlightValidTiles();

        //if newly is selected unit is the currently selected unit deselect it
        if (SelectedUnit == unit) unit = null;

        SelectedUnit = unit;

        //highlight selected units valid tiles
        if (SelectedUnit) SelectedUnit.HighlightValidTiles();

        //unit stat ui
        if (unitStatUI != null)
        {
            unitStatUI.UpdateUnitStatUI(SelectedUnit);
        }
    }
}

