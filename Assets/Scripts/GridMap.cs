using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System;
using System.Linq;

public class GameLogic : MonoBehaviour
{
    private int _width = 20, _height = 10;
    [SerializeField] private Tile _tileprefab;
    [SerializeField] private Transform _cam;
    private Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();

    private void Start()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tileprefab, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                tiles[new Vector2(x, y)] = spawnedTile;
            }
        }
        SpecializeTiles();
        CenterCamera();
    }

    public void SpecializeTiles()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("MapData");
        string[] lines = csvFile.text.Split('\n');

        //each line in MapData.csv holds x and y of special tile and its terrain type (x y type)
        foreach (var line in lines)
        {
            var tileData = line.Replace("\n", "").Replace("\r", "").Split(' ');
            Vector2 position = new Vector2(float.Parse(tileData[0]), float.Parse(tileData[1]));
            Tile tile = GetTileAtPosition(position);
            tile.Specialize(tileData[2]);
        }
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