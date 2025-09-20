using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System;
using System.Linq;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    private int _width = 20, _height = 10;
    [SerializeField] private PlainsTile plains;
    [SerializeField] private ForestTile forest;
    [SerializeField] private RiverTile river;
    [SerializeField] private MountainTile mountain;
    [SerializeField] private CityTile city;
    [SerializeField] private RallyPointTile rallyPoint;
    [SerializeField] private Transform _cam;
    private Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();

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
    }

    public void GenerateGrid(string map)
    {
        CreateSpecialTiles(map);
        CreatePlainsTiles();
        CenterCamera();
        GameManager.Instance.ChangeState(GameState.SpawnPlayer);
    }

    //use povided map to populate all special tiles on map
    private void CreateSpecialTiles(string map)
    {
        //find specified faction csv file
        TextAsset csvFile = Resources.Load<TextAsset>(map);
        string[] lines = csvFile.text.Split('\n');

        //each line in MapData.csv holds x and y of special tile and its terrain type (x y type)
        foreach (var line in lines)
        {
            var tileData = line.Replace("\n", "").Replace("\r", "").Split(' ');
            int x = int.Parse(tileData[0]);
            int y = int.Parse(tileData[1]);
            Tile tileType = null;

            //get prefab of specified tile type
            switch (tileData[2])
            {
                case "forest":
                    tileType = forest;
                    break;
                case "river":
                    tileType = river;
                    break;
                case "mountain":
                    tileType = mountain;
                    break;
                case "city":
                    tileType = city;
                    break;
                case "rallyPoint":
                    tileType = rallyPoint;
                    break;
            }

            CreateTile(x, y, tileType);
        }
    }

    //fill in empty tile spots with plains
    private void CreatePlainsTiles()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                //if no tile at location make plains tile
                if (GetTileAtPosition(new Vector2(x, y)) == null) CreateTile(x, y, plains);
            }
        }
    }

    //create tile of specified terrain type add to dictionary
    private void CreateTile(int x, int y, Tile tileType)
    {   
        var spawnedTile = Instantiate(tileType, new Vector3(x, y), Quaternion.identity);
        spawnedTile.name = $"Tile {x} {y}";
        tiles[new Vector2(x, y)] = spawnedTile;
    }

    public void CenterCamera()
    {
        Camera.main.orthographicSize = Mathf.Max(_width / (2f * Camera.main.aspect), _height / 2f) * 1.1f;
        Camera.main.transform.position = new Vector3(_width / 2f - 0.5f, _height / 2f - 0.5f, -10);
        _cam.transform.position = new Vector3(_width / 2f - 0.5f, _height / 2f - 0.5f, -10);
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        return tiles.TryGetValue(pos, out var tile) ? tile : null;
    }

}