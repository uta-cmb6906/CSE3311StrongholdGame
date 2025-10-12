using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use this if you are using TextMeshPro, otherwise use UnityEngine.UI for standard Text

public class UnitStat : MonoBehaviour
{
    // Assign your Text or TextMeshProUGUI component in the Inspector
    [SerializeField] private TextMeshProUGUI unitInfoText;
    // OR [SerializeField] private Text unitInfoText;

    void Start()
    {
        // Optional: clear the text on start
        if (unitInfoText == null)
        {
            Debug.LogError("Unit Info Text component is not assigned in UnitStat.cs!");
        }
        else
        {
            unitInfoText.text = "";
        }

        // Hide the panel initially
        gameObject.SetActive(false);
    }

    // Call this method whenever a unit is selected or deselected
    public void UpdateUnitStatUI(BaseUnit unit)
    {
        if (unit != null)
        {
            // Unit is selected: show the panel and update text
            gameObject.SetActive(true);

            // Assuming your BaseUnit.UnitInfo() method returns a nicely formatted string
            unitInfoText.text = unit.UnitInfo();
        }
        else
        {
            // Unit is deselected: hide the panel and clear text
            gameObject.SetActive(false);
            unitInfoText.text = "";
        }
    }
}