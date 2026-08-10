using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class QuestPanelLinker : MonoBehaviour
{
    [Header("Quest Button From Pause Menu")]
    [SerializeField] private Button questButton;

    [Header("Quest Panel From Level Scene")]
    [SerializeField] private GameObject questPanel;

    [Header("Pause Menu To Return To")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject mainMenuView;

    [Header("Optional Pause Menu View Controller")]
    [SerializeField] private PauseMenuViewController pauseMenuViewController;

    [Header("Back Button Inside Quest Panel")]
    [SerializeField] private Button backToPauseButton;

    private bool openedFromPauseMenu;
    private bool forceResumeGameplay;
    private bool forcePauseMenu;

    private void Start()
    {
        openedFromPauseMenu = false;
        forceResumeGameplay = false;
        forcePauseMenu = false;

        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }

        if (backToPauseButton != null)
        {
            backToPauseButton.gameObject.SetActive(false);
            backToPauseButton.onClick.RemoveListener(ReturnToPauseMenu);
            backToPauseButton.onClick.AddListener(ReturnToPauseMenu);
        }

        if (questButton != null)
        {
            questButton.onClick.RemoveListener(OpenQuestPanelFromPauseMenu);
            questButton.onClick.AddListener(OpenQuestPanelFromPauseMenu);
        }
    }

    private void Update()
    {
        if (QPressed())
        {
            HandleQPress();
            return;
        }

        if (EscapePressed())
        {
            HandleEscapePress();
        }
    }

    private void LateUpdate()
    {
        if (forceResumeGameplay)
        {
            ForceGameplayState();
        }

        if (forcePauseMenu)
        {
            ForcePauseMenuState();
        }
    }

    private void HandleQPress()
    {
        if (questPanel != null && questPanel.activeInHierarchy)
        {
            if (openedFromPauseMenu == false)
            {
                CloseQuestPanelToGameplay();
            }

            return;
        }

        if (pauseMenuPanel != null && pauseMenuPanel.activeInHierarchy)
        {
            return;
        }

        OpenQuestPanelFromKeyboard();
    }

    private void HandleEscapePress()
    {
        if (questPanel == null || questPanel.activeInHierarchy == false)
        {
            return;
        }

        if (openedFromPauseMenu)
        {
            ReturnToPauseMenu();
            return;
        }

        CloseQuestPanelToPauseMenu();
    }

    private bool QPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Q))
        {
            return true;
        }
#endif

        return false;
    }

    private bool EscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            return true;
        }
#endif

        return false;
    }

    private void OpenQuestPanelFromPauseMenu()
    {
        forceResumeGameplay = false;
        forcePauseMenu = false;
        openedFromPauseMenu = true;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (mainMenuView != null)
        {
            mainMenuView.SetActive(true);
        }

        if (questPanel != null)
        {
            questPanel.SetActive(true);
        }

        if (backToPauseButton != null)
        {
            backToPauseButton.gameObject.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OpenQuestPanelFromKeyboard()
    {
        forceResumeGameplay = false;
        forcePauseMenu = false;
        openedFromPauseMenu = false;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (mainMenuView != null)
        {
            mainMenuView.SetActive(true);
        }

        if (questPanel != null)
        {
            questPanel.SetActive(true);
        }

        if (backToPauseButton != null)
        {
            backToPauseButton.gameObject.SetActive(false);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ReturnToPauseMenu()
    {
        forceResumeGameplay = false;
        forcePauseMenu = true;
        openedFromPauseMenu = false;

        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }

        if (backToPauseButton != null)
        {
            backToPauseButton.gameObject.SetActive(false);
        }

        ForcePauseMenuState();
        StartCoroutine(StopForcePauseMenuAfterDelay());
    }

    private void CloseQuestPanelToPauseMenu()
    {
        forceResumeGameplay = false;
        forcePauseMenu = true;
        openedFromPauseMenu = false;

        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }

        if (backToPauseButton != null)
        {
            backToPauseButton.gameObject.SetActive(false);
        }

        ForcePauseMenuState();
        StartCoroutine(StopForcePauseMenuAfterDelay());
    }

    private void CloseQuestPanelToGameplay()
    {
        forcePauseMenu = false;
        forceResumeGameplay = true;
        openedFromPauseMenu = false;

        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (mainMenuView != null)
        {
            mainMenuView.SetActive(true);
        }

        if (backToPauseButton != null)
        {
            backToPauseButton.gameObject.SetActive(false);
        }

        ForceGameplayState();
        StartCoroutine(StopForceResumeAfterDelay());
    }

    private void ForceGameplayState()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ForcePauseMenuState()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        if (pauseMenuViewController != null)
        {
            pauseMenuViewController.ReturnToPauseMainMenu();
        }
        else if (mainMenuView != null)
        {
            mainMenuView.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator StopForceResumeAfterDelay()
    {
        yield return null;
        yield return null;
        yield return null;

        forceResumeGameplay = false;
        ForceGameplayState();

        Debug.Log("Quest closed from Q. Gameplay resumed. TimeScale: " + Time.timeScale);
    }

    private IEnumerator StopForcePauseMenuAfterDelay()
    {
        yield return null;
        yield return null;
        yield return null;

        forcePauseMenu = false;
        ForcePauseMenuState();

        Debug.Log("Quest closed to pause menu. TimeScale: " + Time.timeScale);
    }
}