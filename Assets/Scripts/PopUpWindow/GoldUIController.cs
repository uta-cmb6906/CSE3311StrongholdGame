using UnityEngine;
using TMPro;
using System;

public class GoldUIController : MonoBehaviour
{
    // Assign the TextMeshProUGUI 
    [SerializeField]
    private TextMeshProUGUI goldText;

    // Player's gold for the top-left display
    private readonly Team targetTeam = Team.Player;

    void Start()
    {
        // Check if the Text component is assigned
        if (goldText == null)
        {
            Debug.LogError("GoldUIController: goldText (TextMeshProUGUI) is not assigned in the Inspector.");
            return;
        }

        //  Gold change 
        GameManager.OnGoldChanged += UpdateGoldDisplay;

        // If the GameManager is already initialized, get the starting gold 
        if (GameManager.Instance != null)
        {

            UpdateGoldDisplay(targetTeam, GameManager.Instance.GetGold(targetTeam));
        }
    }

    private void OnDestroy()
    {

        GameManager.OnGoldChanged -= UpdateGoldDisplay;
    }


    /// <param name="who">The team whose gold changed.</param>
    /// <param name="newGold">The new gold amount.</param>
    private void UpdateGoldDisplay(Team who, int newGold)
    {

        if (who == targetTeam)
        {
            // Update the text to display the gold count
            goldText.text = newGold.ToString() + " Gold";
        }
    }
}
