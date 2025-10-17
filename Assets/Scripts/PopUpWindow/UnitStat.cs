using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use this if you are using TextMeshPro, otherwise use UnityEngine.UI for standard Text

public class UnitStat : MonoBehaviour
{
    // Assign your Text or TextMeshProUGUI component in the Inspector
    [SerializeField] private TextMeshProUGUI unitInfoText;


    void Start()
    {
        //clear the text on start
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
            // Unit is selected
            gameObject.SetActive(true);


            unitInfoText.text = unit.UnitInfo();
        }
        else
        {
            // Unit is deselected
            gameObject.SetActive(false);
            unitInfoText.text = "";
        }
    }
}