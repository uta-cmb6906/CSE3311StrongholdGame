using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    private Button button;

    void Awake()
    {
        // Get the Button component attached to this GameObject
        button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("UpgradeButton requires a Button component on the same GameObject.");
            return;
        }

        // Add the listener for the click event
        button.onClick.AddListener(OnUpgradeClicked);
    }

    void OnUpgradeClicked()
    {
        // Only allow ending the turn if the GameManager is initialized
        if (GameManager.Instance != null)
        {
            // Call the new method in GameManager to transition to the next state
            GameManager.Instance.TryUpgrade();
        }
    }
}