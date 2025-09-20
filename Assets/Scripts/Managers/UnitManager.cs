using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;
    private List<ScriptableUnit> _units;
    public BaseUnit SelectedUnit;

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

    //Spwn all player and enemy units
    public void SpawnFactions(string playerFaction, string enemyFaction)
    {
        SpawnFactionUnits(playerFaction, Team.Player);
        SpawnFactionUnits(enemyFaction, Team.Enemy);
        GameManager.Instance.ChangeState(GameState.PlayerTurn);
    }

    //use provided faction to spawn its units
    public void SpawnFactionUnits(string faction, Team team)
    {
        //find specified faction csv file
        TextAsset csvFile = Resources.Load<TextAsset>("Factions/" + faction);
        string[] lines = csvFile.text.Split('\n');

        //each line in faction.csv holds x and y coordinates of a unit and its type (x y type)
        foreach (var line in lines)
        {
            var unitData = line.Replace("\n", "").Replace("\r", "").Split(' ');

            Vector2 position = new Vector2(float.Parse(unitData[0]), float.Parse(unitData[1]));
            Tile spawnTile = GridManager.Instance.GetTileAtPosition(position);

            //find specified prefab of unit getting player/enemy version dependent on provided team
            var unitType = _units.Find(u => u.name == unitData[2] && u.Team == team).UnitPrefab;

            CreateUnit(unitType, spawnTile);
        }
    }

    public void CreateUnit(BaseUnit unit, Tile spawnTile)
    {
        var spawnedUnit = Instantiate(unit);
        spawnTile._unitStationed = spawnedUnit;
        spawnedUnit.OccupiedTile = spawnTile;
        spawnedUnit.transform.position = spawnTile.transform.position;
    }

    public void SelectUnit(BaseUnit unit)
    {
        SelectedUnit = unit;
    }
}
