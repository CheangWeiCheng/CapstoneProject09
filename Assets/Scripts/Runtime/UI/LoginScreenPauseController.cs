using UnityEngine;
using UnityEngine.InputSystem;

public class LoginScreenPauseController : MonoBehaviour
{
    [Header("Pause While Login Screen Is Active")]
    [SerializeField] private bool showCursor = true;
    [SerializeField] private bool unlockCursor = true;

    [Header("Player Gameplay Input")]
    [Tooltip("Drag the PlayerInput component from Ito_NestedPrefab here.")]
    [SerializeField] private PlayerInput gameplayPlayerInput;

    [Header("Direct Keyboard Input Scripts")]
    [Tooltip(
        "Scripts that manually read Keyboard.current or Input.GetKey. " +
        "These will be disabled while the Login Screen is active."
    )]
    [SerializeField] private MonoBehaviour[] directKeyboardInputScripts;

    private bool playerInputPreviousState;
    private bool[] directScriptPreviousStates;

    private bool inputBlockApplied;

    private void OnEnable()
    {
        ApplyLoginBlock();
    }

    private void Start()
    {
        ApplyLoginBlock();
    }

    private void Update()
    {
        // Keep the game paused even if another script
        // tries to resume it while Login Screen is open.
        if (Time.timeScale != 0f)
        {
            Time.timeScale = 0f;
        }

        // Keep gameplay PlayerInput disabled.
        if (gameplayPlayerInput != null &&
            gameplayPlayerInput.enabled)
        {
            gameplayPlayerInput.enabled = false;
        }

        // Keep all manually-polled gameplay keyboard
        // scripts disabled as well.
        if (directKeyboardInputScripts != null)
        {
            for (int i = 0; i < directKeyboardInputScripts.Length; i++)
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

        if (unlockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (showCursor)
        {
            Cursor.visible = true;
        }
    }

    private void OnDisable()
    {
        ReleaseLoginInputBlock();
    }

    private void ApplyLoginBlock()
    {
        Time.timeScale = 0f;

        if (unlockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (showCursor)
        {
            Cursor.visible = true;
        }

        if (inputBlockApplied)
        {
            return;
        }

        inputBlockApplied = true;

        // Remember whether PlayerInput was enabled
        // before Login Screen blocked it.
        if (gameplayPlayerInput != null)
        {
            playerInputPreviousState =
                gameplayPlayerInput.enabled;

            gameplayPlayerInput.enabled = false;

            Debug.Log(
                "LOGIN INPUT BLOCK: PlayerInput disabled."
            );
        }
        else
        {
            Debug.LogWarning(
                "LOGIN INPUT BLOCK: Gameplay PlayerInput is not assigned."
            );
        }

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

            if (script == this)
            {
                continue;
            }

            directScriptPreviousStates[i] =
                script.enabled;

            script.enabled = false;

            Debug.Log(
                "LOGIN INPUT BLOCK: Disabled " +
                script.GetType().Name
            );
        }
    }

    private void ReleaseLoginInputBlock()
    {
        if (!inputBlockApplied)
        {
            return;
        }

        inputBlockApplied = false;

        // Restore PlayerInput to whatever state it had
        // before the Login Screen appeared.
        if (gameplayPlayerInput != null)
        {
            gameplayPlayerInput.enabled =
                playerInputPreviousState;

            Debug.Log(
                "LOGIN INPUT BLOCK: PlayerInput restored."
            );
        }

        // Restore every manually-polled input script
        // to its previous enabled/disabled state.
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

        /*
         * IMPORTANT:
         *
         * Do NOT set Time.timeScale = 1 here.
         *
         * Your TutorialPopupController opens immediately
         * after Login Screen closes and should keep the game
         * paused until the player finishes the tutorial.
         */
    }
}