using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialPopupController : MonoBehaviour
{
    [Header("Login")]
    [SerializeField] private GameObject loginScreen;

    [Header("Tutorial UI")]
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private GameObject normalGuideImage;
    [SerializeField] private GameObject attackGuideImage;

    [Header("Buttons")]
    [SerializeField] private Button nextGuideButton;
    [SerializeField] private Button previousGuideButton;
    [SerializeField] private Button closeTutorialButton;

    [Header("Gameplay Input Blocking")]
    [Tooltip("Drag the PlayerInput component from Ito_NestedPrefab here.")]
    [SerializeField] private PlayerInput gameplayPlayerInput;

    [Tooltip(
        "Scripts that directly read keyboard input. " +
        "These are disabled until the tutorial is completed."
    )]
    [SerializeField] private MonoBehaviour[] directKeyboardInputScripts;

    private bool tutorialStarted;
    private bool tutorialCompleted;

    private bool inputBlockApplied;
    private bool playerInputPreviousState;
    private bool[] directScriptPreviousStates;

    private void Start()
    {
        tutorialStarted = false;
        tutorialCompleted = false;
        inputBlockApplied = false;

        if (tutorialUI != null)
        {
            tutorialUI.SetActive(false);
        }

        ShowNormalGuide();

        if (nextGuideButton != null)
        {
            nextGuideButton.onClick.RemoveListener(ShowAttackGuide);
            nextGuideButton.onClick.AddListener(ShowAttackGuide);
        }

        if (previousGuideButton != null)
        {
            previousGuideButton.onClick.RemoveListener(ShowNormalGuide);
            previousGuideButton.onClick.AddListener(ShowNormalGuide);
        }

        if (closeTutorialButton != null)
        {
            closeTutorialButton.onClick.RemoveListener(CloseTutorial);
            closeTutorialButton.onClick.AddListener(CloseTutorial);
        }
    }

    private void Update()
    {
        // Tutorial is currently open.
        if (tutorialStarted && !tutorialCompleted)
        {
            MaintainTutorialInputBlock();
            return;
        }

        // Tutorial has already been completed.
        if (tutorialStarted)
        {
            return;
        }

        if (loginScreen == null)
        {
            return;
        }

        // Login Screen has closed.
        if (!loginScreen.activeInHierarchy)
        {
            OpenTutorial();
        }
    }

    private void OpenTutorial()
    {
        if (tutorialStarted)
        {
            return;
        }

        tutorialStarted = true;
        tutorialCompleted = false;

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (tutorialUI != null)
        {
            tutorialUI.SetActive(true);
        }

        ShowNormalGuide();

        BlockGameplayInput();

        Debug.Log(
            "TUTORIAL: Tutorial opened. Gameplay input blocked."
        );
    }

    private void BlockGameplayInput()
    {
        if (inputBlockApplied)
        {
            return;
        }

        inputBlockApplied = true;

        // Disable PlayerInput.
        if (gameplayPlayerInput != null)
        {
            playerInputPreviousState =
                gameplayPlayerInput.enabled;

            gameplayPlayerInput.enabled = false;

            Debug.Log(
                "TUTORIAL INPUT BLOCK: PlayerInput disabled."
            );
        }
        else
        {
            Debug.LogWarning(
                "TUTORIAL INPUT BLOCK: Gameplay PlayerInput is not assigned."
            );
        }

        // Disable scripts that manually check keyboard buttons.
        if (directKeyboardInputScripts == null)
        {
            directScriptPreviousStates = null;
            return;
        }

        directScriptPreviousStates =
            new bool[directKeyboardInputScripts.Length];

        for (int i = 0; i < directKeyboardInputScripts.Length; i++)
        {
            MonoBehaviour script =
                directKeyboardInputScripts[i];

            if (script == null)
            {
                continue;
            }

            // Never disable this tutorial controller itself.
            if (script == this)
            {
                continue;
            }

            directScriptPreviousStates[i] =
                script.enabled;

            script.enabled = false;

            Debug.Log(
                "TUTORIAL INPUT BLOCK: Disabled " +
                script.GetType().Name
            );
        }
    }

    private void MaintainTutorialInputBlock()
    {
        // Keep the game paused until tutorial is completed.
        if (Time.timeScale != 0f)
        {
            Time.timeScale = 0f;
        }

        // Prevent another script from turning PlayerInput back on.
        if (gameplayPlayerInput != null &&
            gameplayPlayerInput.enabled)
        {
            gameplayPlayerInput.enabled = false;
        }

        // Prevent manually-polled gameplay scripts
        // from being enabled during the tutorial.
        if (directKeyboardInputScripts != null)
        {
            for (int i = 0;
                 i < directKeyboardInputScripts.Length;
                 i++)
            {
                MonoBehaviour script =
                    directKeyboardInputScripts[i];

                if (script == null)
                {
                    continue;
                }

                if (script == this)
                {
                    continue;
                }

                if (script.enabled)
                {
                    script.enabled = false;
                }
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ReleaseGameplayInput()
    {
        if (!inputBlockApplied)
        {
            return;
        }

        inputBlockApplied = false;

        // Restore PlayerInput to the state it had
        // before the tutorial opened.
        if (gameplayPlayerInput != null)
        {
            gameplayPlayerInput.enabled =
                playerInputPreviousState;

            Debug.Log(
                "TUTORIAL INPUT BLOCK: PlayerInput restored."
            );
        }

        // Restore direct keyboard scripts to their
        // previous enabled/disabled states.
        if (directKeyboardInputScripts != null &&
            directScriptPreviousStates != null)
        {
            int count = Mathf.Min(
                directKeyboardInputScripts.Length,
                directScriptPreviousStates.Length
            );

            for (int i = 0; i < count; i++)
            {
                MonoBehaviour script =
                    directKeyboardInputScripts[i];

                if (script == null)
                {
                    continue;
                }

                if (script == this)
                {
                    continue;
                }

                script.enabled =
                    directScriptPreviousStates[i];
            }
        }
    }

    private void ShowNormalGuide()
    {
        if (normalGuideImage != null)
        {
            normalGuideImage.SetActive(true);
        }

        if (attackGuideImage != null)
        {
            attackGuideImage.SetActive(false);
        }

        if (nextGuideButton != null)
        {
            nextGuideButton.gameObject.SetActive(true);
        }

        if (previousGuideButton != null)
        {
            previousGuideButton.gameObject.SetActive(false);
        }

        if (closeTutorialButton != null)
        {
            closeTutorialButton.gameObject.SetActive(false);
        }
    }

    private void ShowAttackGuide()
    {
        if (normalGuideImage != null)
        {
            normalGuideImage.SetActive(false);
        }

        if (attackGuideImage != null)
        {
            attackGuideImage.SetActive(true);
        }

        if (nextGuideButton != null)
        {
            nextGuideButton.gameObject.SetActive(false);
        }

        if (previousGuideButton != null)
        {
            previousGuideButton.gameObject.SetActive(true);
        }

        if (closeTutorialButton != null)
        {
            closeTutorialButton.gameObject.SetActive(true);
        }
    }

    public void CloseTutorial()
    {
        if (tutorialCompleted)
        {
            return;
        }

        tutorialCompleted = true;

        if (tutorialUI != null)
        {
            tutorialUI.SetActive(false);
        }

        ReleaseGameplayInput();

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log(
            "TUTORIAL: Tutorial completed. Gameplay input restored."
        );
    }
}