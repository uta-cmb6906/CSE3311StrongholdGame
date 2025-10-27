using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BuildCastleButton : MonoBehaviour
{
    private Button button;
    [SerializeField] private GameObject confirmationWindow;
    [SerializeField] private GameObject unaffordableWindow;
    private bool needConfirmation = false;

    void Awake()
    {
        // Get the Button component attached to this GameObject
        button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("BuildCastleButton requires a Button component on the same GameObject.");
            return;
        }

        // Add the listener for the click event
        button.onClick.AddListener(OnBuildCastleClicked);
    }

    void OnBuildCastleClicked()
    {
        // Only allow ending the turn if the GameManager is initialized
        if (GameManager.Instance != null)
        {
            if (!needConfirmation)
            {
                StartCoroutine(Confirm());
            }
            else
            {
                confirmationWindow.SetActive(false);
                needConfirmation = false;

                // Call the new method in GameManager to transition to the next state
                if (!GameManager.Instance.TryBuyCastle())
                {
                    StartCoroutine(Unaffordable());
                }
            }
        }
    }

    private IEnumerator Confirm()
    {
        confirmationWindow.SetActive(true);
        needConfirmation = true;
        yield return new WaitForSeconds(3);
        confirmationWindow.SetActive(false);
        needConfirmation = false;
    }

    private IEnumerator Unaffordable()
    {
        unaffordableWindow.SetActive(true);
        yield return new WaitForSeconds(3);
        unaffordableWindow.SetActive(false);
    }
}