using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System;
using System.Linq;
using NUnit.Framework;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public int _width = 20, _height = 10;
    [SerializeField] public PlainsTile plains;
    [SerializeField] private ForestTile forest;
    [SerializeField] private RiverTile river;
    [SerializeField] private MountainTile mountain;
    [SerializeField] private CityTile city;
    [SerializeField] private RallyPointTile rallyPoint;
    [SerializeField] private Transform _cam;
    private Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();

    private Camera GetCam()
    {
        if (_cam != null) return _cam.GetComponent<Camera>();
        return Camera.main;
    }
    
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
            return;
        }
        if (_cam == null && Camera.main != null)
        _cam = Camera.main.transform;
    }

    public void GenerateGrid(string map)
    {
        if (map == "RandomMap")
        {
            MakeRandomMap();
        }
        else
        {
            CreateSpecialTiles(map);
            CreatePlainsTiles();
        }
        
        CenterCamera();
        GameManager.Instance.ChangeState(GameState.SpawnFactions);
    }

    public void MakeRandomMap()
    {
        int playerCities = 0;
        int enemyCities = 0;
        int randomInt;

        for(int x = 0; x < GridManager.Instance._width; x++)
        {
            for(int y = 0; y < GridManager.Instance._height; y++)
            {
                //player rally point
                if (x == 1 && y == 5)
                {
                    CreateTile(x, y, rallyPoint, true);
                }
                //enemy rally point
                else if (x == 18 && y == 5)
                {
                    CreateTile(x, y, rallyPoint, false);
                }
                //other tiles
                else
                {
                    randomInt = UnityEngine.Random.Range(0, 100);
                    //plains 60%
                    if (randomInt < 60)
                    {
                        CreateTile(x, y, plains, false);
                    }
                    //forest 8%
                    else if (randomInt < 68)
                    {
                        CreateTile(x, y, forest, false);
                    }
                    //river 8%
                    else if (randomInt < 76)
                    {
                        CreateTile(x, y, river, false);
                    }
                    //mountain 8%
                    else if (randomInt < 84)
                    {
                        CreateTile(x, y, mountain, false);
                    }
                    //player city 8%
                    else if (randomInt < 92)
                    {
                        if (x <= 7 && playerCities < 3)
                        {
                            CreateTile(x, y, city, true);
                            playerCities++;
                        }
                        else CreateTile(x, y, plains, false);
                    }
                    //enemy city 8%
                    else
                    {
                        if (x >= 12 && enemyCities < 3)
                        {
                            CreateTile(x, y, city, false);
                            enemyCities++;
                        }
                        else CreateTile(x, y, plains, false);
                    }
                }
            }
        }
    }

    //use povided map to populate all special tiles on map
    private void CreateSpecialTiles(string map)
    {
        //find specified map csv file
        TextAsset csvFile = Resources.Load<TextAsset>(map);
        string[] lines = csvFile.text.Split('\n');

        //each line in map.csv holds x and y coordinates of special tile and its terrain type (x y type)
        foreach (var line in lines)
        {
            var tileData = line.Replace("\n", "").Replace("\r", "").Split(' ');
            int x = int.Parse(tileData[0]);
            int y = int.Parse(tileData[1]);
            Tile tileType = null;
            bool isPlayer = false;

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
                case "userCity":
                    tileType = city;
                    isPlayer = true;
                    break;
                case "enemyCity":
                    tileType = city;
                    break;
                case "userRallyPoint":
                    tileType = rallyPoint;
                    isPlayer = true;
                    break;
                case "enemyRallyPoint":
                    tileType = rallyPoint;
                    break;
            }

            CreateTile(x, y, tileType, isPlayer);
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
                if (GetTileAtPosition(new Vector2(x, y)) == null) CreateTile(x, y, plains, false);
            }
        }
    }

    //create tile of specified terrain type add to dictionary
    public void CreateTile(int x, int y, Tile tileType, bool isPlayer)
    {
        var spawnedTile = Instantiate(tileType, new Vector3(x, y), Quaternion.identity);
        spawnedTile.SetPlayer(isPlayer);
        spawnedTile.SetCoords(x, y);
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

    public bool IsValidCoordinate(int x, int y)
    {
        if (x < _width && y < _height)
            return true;
        else
            return false;
    }
}
