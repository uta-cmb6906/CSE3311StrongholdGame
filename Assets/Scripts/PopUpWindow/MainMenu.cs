using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    //public GameObject optionsPanel;
    public GameObject difficultyPanel;
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void QuitGame()
    {

        Application.Quit();
        Debug.Log("Quitting...");
    }

    // public void OpenOptions()
    // {
    //     mainMenuPanel.SetActive(false);
    //     optionsPanel.SetActive(false);
    //     difficultyPanel.SetActive(false);
    // }

    // public void CloseOptions()
    // {
    //     optionsPanel.SetActive(false);
    //     mainMenuPanel.SetActive(true);
    // }

    public void OpenDifficultySettings()
    {
        mainMenuPanel.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    public void CloseDifficultySettings()
    {
        difficultyPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // all the difficulty settings logic will implement it later
    public void SetDifficulty(string difficultyLevel)
    {
        Debug.Log("Difficulty is set to " + difficultyLevel);
        CloseDifficultySettings();
    }
}