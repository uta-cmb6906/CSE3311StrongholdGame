using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameManager;

public class MainMenu : MonoBehaviour
{

    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject mapPanel;
    public GameObject difficultyPanel;
    public GameObject tutorialPanel;
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void QuitGame()
    {

        Application.Quit();
        Debug.Log("Quitting...");
    }

    //methods to handle the Difficulty Panel
    public void OpenDifficulty()
    {
        mainMenuPanel.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    public void CloseDifficulty()
    {
        difficultyPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    //method to set the difficulty and close the panel
    public void SetDifficultyAndClose(int value)
    {
        GameManager.GlobalDifficulty = value;
        Debug.Log($"[MainMenu] Setting global difficulty to: {value}");

        CloseDifficulty();
    }

    public void OpenMap()
    {
        mainMenuPanel.SetActive(false);
        mapPanel.SetActive(true);
    }

    public void CloseMap()
    {
        mapPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OpenTutorial()
    {
        mainMenuPanel.SetActive(false);
        tutorialPanel.SetActive(true);
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // all the Map settings logic will implement it later

    // Implemented :)
    public void SetMap(string mapType)
    {
        Debug.Log("Map is set to " + mapType);

        switch (mapType)
        {
            case "Map 1":
                MapToLoad = "MapData1";
                break;
            case "Map 2":
                MapToLoad = "MapData2";
                break;
            case "Map 3":
                MapToLoad = "MapData3";
                break;
            default:
                MapToLoad = "MapData";
                break;
        }
        Debug.Log($"Map set to load: {MapToLoad}.csv");
        CloseMap();
    }
}