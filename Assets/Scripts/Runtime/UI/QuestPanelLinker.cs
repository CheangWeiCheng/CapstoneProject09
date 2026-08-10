using UnityEngine;
using UnityEngine.UI;

public class QuestPanelLinker : MonoBehaviour
{
    [Header("Quest Button From Ito Prefab")]
    [SerializeField] private Button questButton;

    [Header("Quest Panel From Level Scene")]
    [SerializeField] private GameObject questPanel;

    [Header("Pause Menu To Return To")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject mainMenuView;

    [Header("Back Button Inside Quest Panel")]
    [SerializeField] private Button backToPauseButton;

    private void Start()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }

        if (questButton != null)
        {
            questButton.onClick.RemoveListener(OpenQuestPanel);
            questButton.onClick.AddListener(OpenQuestPanel);
        }

        if (backToPauseButton != null)
        {
            backToPauseButton.onClick.RemoveListener(ReturnToPauseMenu);
            backToPauseButton.onClick.AddListener(ReturnToPauseMenu);
        }
    }

    private void OpenQuestPanel()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (mainMenuView != null)
        {
            mainMenuView.SetActive(false);
        }

        if (questPanel != null)
        {
            questPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ReturnToPauseMenu()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        if (mainMenuView != null)
        {
            mainMenuView.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}