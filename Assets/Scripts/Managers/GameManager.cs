using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState GameState;

    public static event Action<GameState> OnGameStateChanged;

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

    //start on state generate Grid
    void Start()
    {
        //Instantiate(GridManager);
    //TODO eventually start at start menu
        ChangeState(GameState.GenerateGrid);
    }

    //change game state (initiating state logic), and broadcast state change event
    public void ChangeState(GameState newState)
    {
        GameState = newState;
        switch (newState)
        {
            case GameState.GenerateGrid:
                GridManager.Instance.GenerateGrid("MapData");
                break;
            case GameState.SpawnPlayer:
                UnitManager.Instance.SpawnPlayer("examplePlayer");
                break;
            case GameState.SpawnEnemy:
                UnitManager.Instance.SpawnEnemy("exampleEnemy");
                break;
            case GameState.PlayerTurn:
                break;
            case GameState.EnemyTurn:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnGameStateChanged?.Invoke(newState);
    }
}

public enum GameState
{
    GenerateGrid,
    SpawnPlayer,
    SpawnEnemy,
    PlayerTurn,
    EnemyTurn
}