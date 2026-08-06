using TMPro;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    [Header("Timer Display")]
    [SerializeField]
    private TMP_Text timerText;

    [Header("Player")]
    [SerializeField]
    private Transform playerTransform;

    [SerializeField]
    private float movementStartDistance = 0.1f;

    [Header("Pause Timer While These Are Open")]
    [SerializeField]
    private GameObject[] pauseTimerWhileActive;

    [Header("Firebase")]
    [SerializeField]
    private FirebaseGameCompletionService
        firebaseGameCompletionService;

    private Vector3 startingPlayerPosition;

    private float elapsedTime;

    private bool timerStarted;
    private bool timerCompleted;
    private bool startingPositionRecorded;

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
        FindPlayerIfMissing();
        FindFirebaseServiceIfMissing();

        RecordStartingPosition();
        UpdateTimerText();
    }

    private void Update()
    {
        if (timerCompleted)
        {
            return;
        }

        FindPlayerIfMissing();

        if (!startingPositionRecorded)
        {
            RecordStartingPosition();
        }

        if (IsTimerPaused())
        {
            return;
        }

        if (!timerStarted)
        {
            CheckForPlayerMovement();
        }

        if (!timerStarted)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        UpdateTimerText();
    }

    private void FindPlayerIfMissing()
    {
        if (playerTransform != null)
        {
            return;
        }

        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (playerObject != null)
        {
            playerTransform =
                playerObject.transform;
        }
    }

    private void FindFirebaseServiceIfMissing()
    {
        if (firebaseGameCompletionService != null)
        {
            return;
        }

        firebaseGameCompletionService =
            FindObjectOfType<
                FirebaseGameCompletionService
            >();
    }

    private void RecordStartingPosition()
    {
        if (playerTransform == null)
        {
            return;
        }

        startingPlayerPosition =
            playerTransform.position;

        startingPositionRecorded = true;
    }

    private void CheckForPlayerMovement()
    {
        if (playerTransform == null ||
            !startingPositionRecorded)
        {
            return;
        }

        Vector2 startingHorizontalPosition =
            new Vector2(
                startingPlayerPosition.x,
                startingPlayerPosition.z
            );

        Vector2 currentHorizontalPosition =
            new Vector2(
                playerTransform.position.x,
                playerTransform.position.z
            );

        float horizontalDistance =
            Vector2.Distance(
                startingHorizontalPosition,
                currentHorizontalPosition
            );

        if (horizontalDistance <
            movementStartDistance)
        {
            return;
        }

        timerStarted = true;

        Debug.Log(
            "Game completion timer started."
        );
    }

    private bool IsTimerPaused()
    {
        if (Time.timeScale <= 0f)
        {
            return true;
        }

        if (pauseTimerWhileActive == null)
        {
            return false;
        }

        foreach (
            GameObject pauseObject
            in pauseTimerWhileActive
        )
        {
            if (pauseObject != null &&
                pauseObject.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    public void CompleteLevel()
    {
        if (timerCompleted)
        {
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
            "Game completed in " +
            formattedFinalTime +
            " (" +
            finalTimeMilliseconds +
            " milliseconds)."
        );

        FindFirebaseServiceIfMissing();

        if (firebaseGameCompletionService == null)
        {
            Debug.LogError(
                "FirebaseGameCompletionService " +
                "could not be found."
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
        startingPositionRecorded = false;

        FindPlayerIfMissing();
        RecordStartingPosition();
        UpdateTimerText();
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