using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelTimer : MonoBehaviour
{
    [Header("Timer Display")]
    [SerializeField]
    private TMP_Text timerText;

    [Header("Pause Timer While These Are Open")]
    [SerializeField]
    private GameObject[] pauseTimerWhileActive;

    [Header("Firebase")]
    [SerializeField]
    private FirebaseGameCompletionService
        firebaseGameCompletionService;

    [Header("Movement Detection")]
    [SerializeField]
    private float gamepadDeadzone = 0.15f;

    private float elapsedTime;

    private bool timerStarted;
    private bool timerCompleted;

    public float ElapsedTime
    {
        get
        {
            return elapsedTime;
        }
    }

    public int ElapsedMilliseconds
    {
        get
        {
            return Mathf.FloorToInt(
                elapsedTime * 1000f
            );
        }
    }

    public bool TimerStarted
    {
        get
        {
            return timerStarted;
        }
    }

    public bool TimerCompleted
    {
        get
        {
            return timerCompleted;
        }
    }

    private void Start()
    {
        elapsedTime = 0f;
        timerStarted = false;
        timerCompleted = false;

        UpdateTimerText();

        Debug.Log(
            "Game completion timer is ready."
        );
    }

    private void Update()
    {
        if (timerCompleted)
        {
            return;
        }

        /*
         * Login screen, pause menu, or anything else
         * that pauses the whole game.
         */
        if (Time.timeScale <= 0f)
        {
            return;
        }

        /*
         * Shop, inventory, leaderboard, etc. can also
         * explicitly pause the timer through the Inspector.
         */
        if (IsTimerPausedByUI())
        {
            return;
        }

        /*
         * Timer has not started yet.
         * Wait for actual movement input.
         */
        if (!timerStarted)
        {
            if (!HasMovementInput())
            {
                return;
            }

            StartTimer();
        }

        /*
         * Once started, count normally.
         */
        elapsedTime += Time.deltaTime;

        UpdateTimerText();
    }

    private bool HasMovementInput()
    {
        /*
         * Keyboard movement:
         * W A S D
         */
        if (Keyboard.current != null)
        {
            if (
                Keyboard.current.wKey.isPressed ||
                Keyboard.current.aKey.isPressed ||
                Keyboard.current.sKey.isPressed ||
                Keyboard.current.dKey.isPressed
            )
            {
                return true;
            }
        }

        /*
         * Controller movement:
         * Left Stick
         */
        if (Gamepad.current != null)
        {
            Vector2 leftStick =
                Gamepad.current.leftStick.ReadValue();

            if (leftStick.magnitude >=
                gamepadDeadzone)
            {
                return true;
            }
        }

        return false;
    }

    private void StartTimer()
    {
        if (timerStarted ||
            timerCompleted)
        {
            return;
        }

        timerStarted = true;

        Debug.Log(
            "GAME TIMER STARTED."
        );
    }

    private bool IsTimerPausedByUI()
    {
        if (pauseTimerWhileActive == null)
        {
            return false;
        }

        foreach (
            GameObject pauseObject
            in pauseTimerWhileActive
        )
        {
            if (
                pauseObject != null &&
                pauseObject.activeInHierarchy
            )
            {
                return true;
            }
        }

        return false;
    }

    public void CompleteLevel()
    {
        CompleteGame();
    }

    public void CompleteGame()
    {
        if (timerCompleted)
        {
            return;
        }

        if (!timerStarted)
        {
            Debug.LogWarning(
                "Game completion attempted before " +
                "the timer had started."
            );

            return;
        }

        timerCompleted = true;
        timerStarted = false;

        UpdateTimerText();

        int finalTimeMilliseconds =
            ElapsedMilliseconds;

        string formattedFinalTime =
            GetFormattedTime();

        Debug.Log(
            "GAME COMPLETED: " +
            formattedFinalTime
        );

        if (firebaseGameCompletionService == null)
        {
            Debug.LogError(
                "Firebase Game Completion Service " +
                "has not been assigned to LevelTimer."
            );

            return;
        }

        firebaseGameCompletionService
            .SaveBestCompletionTime(
                finalTimeMilliseconds,
                formattedFinalTime
            );
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        timerStarted = false;
        timerCompleted = false;

        UpdateTimerText();

        Debug.Log(
            "Game completion timer reset."
        );
    }

    public string GetFormattedTime()
    {
        int totalMilliseconds =
            Mathf.FloorToInt(
                elapsedTime * 1000f
            );

        int minutes =
            totalMilliseconds / 60000;

        int seconds =
            totalMilliseconds / 1000 % 60;

        int milliseconds =
            totalMilliseconds % 1000;

        return string.Format(
            "{0:00}:{1:00}.{2:000}",
            minutes,
            seconds,
            milliseconds
        );
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        timerText.text =
            GetFormattedTime();
    }
}