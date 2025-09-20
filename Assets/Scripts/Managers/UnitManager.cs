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

    //use provided faction to create all player units
    public void SpawnPlayer(string faction)
    {
        //find specified faction csv file
        TextAsset csvFile = Resources.Load<TextAsset>("Factions/" + faction);
        string[] lines = csvFile.text.Split('\n');

        foreach (var line in lines)
        {
            var unitData = line.Replace("\n", "").Replace("\r", "").Split(' ');

            Vector2 position = new Vector2(float.Parse(unitData[0]), float.Parse(unitData[1]));
            Tile spawnTile = GridManager.Instance.GetTileAtPosition(position);
            var unitType = _units.Find(u => u.name == unitData[2] && u.Team == Team.Player).UnitPrefab;

            CreateUnit(unitType, spawnTile);
        }

        GameManager.Instance.ChangeState(GameState.SpawnEnemy);
    }

    //use provided faction to create all enemy units
    public void SpawnEnemy(string faction)
    {
        //find specified faction csv file
        TextAsset csvFile = Resources.Load<TextAsset>("Factions/" + faction);
        string[] lines = csvFile.text.Split('\n');

        foreach (var line in lines)
        {
            var unitData = line.Replace("\n", "").Replace("\r", "").Split(' ');

            Vector2 position = new Vector2(float.Parse(unitData[0]), float.Parse(unitData[1]));
            Tile spawnTile = GridManager.Instance.GetTileAtPosition(position);
            var unitType = _units.Find(u => u.name == unitData[2] && u.Team == Team.Enemy).UnitPrefab;

            CreateUnit(unitType, spawnTile);
        }

        GameManager.Instance.ChangeState(GameState.PlayerTurn);
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
