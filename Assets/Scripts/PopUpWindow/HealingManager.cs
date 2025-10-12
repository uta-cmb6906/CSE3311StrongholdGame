using UnityEngine;
using TMPro;

public class HealingManager : MonoBehaviour
{
    public static HealingManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject healConfirmationPanel;
    [SerializeField] private TextMeshProUGUI costText;

    private BaseUnit _selectedUnit;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Ensure the panel is hidden on start
        if (healConfirmationPanel != null)
        {
            healConfirmationPanel.SetActive(false);
        }
    }


    public void ShowHealingConfirmation(BaseUnit unitToHeal)
    {


        if (unitToHeal == null || unitToHeal.Health() >= unitToHeal.maxHealth)
        {
            Debug.Log("Cannot heal: No unit selected or unit is already at max health.");
            return;
        }

        if (GameManager.Instance.GetGold(Team.Player) < GameManager.Instance.HealCost)
        {
            Debug.Log("Cannot heal: Not enough gold.");
            return;
        }

        _selectedUnit = unitToHeal;


        if (costText != null)
        {
            costText.text = $"Heal Cost: {GameManager.Instance.HealCost} Gold";
        }

        if (healConfirmationPanel != null)
        {
            healConfirmationPanel.SetActive(true);
        }
    }


    public void ConfirmHeal()
    {
        if (_selectedUnit != null)
        {
            Team playerTeam = Team.Player;
            int cost = GameManager.Instance.HealCost;

            if (GameManager.Instance.TrySpendGold(playerTeam, cost))
            {

                _selectedUnit.HealUnit();
                Debug.Log($"Unit {_selectedUnit.name} healed for {cost} gold.");
            }
            else
            {
                Debug.LogError("Error spending gold. Healing cancelled.");
            }
        }

        CloseHealingConfirmation();
    }


    /// Closes the confirmation panel. Called by the 'No' button or on turn start.

    public void CloseHealingConfirmation()
    {
        _selectedUnit = null; // Clear the selected unit reference
        if (healConfirmationPanel != null)
        {
            healConfirmationPanel.SetActive(false);
        }
    }
}
