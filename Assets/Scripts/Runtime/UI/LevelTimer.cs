using TMPro;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float movementThreshold = 0.1f;

    [Header("Level 1 Completion Goals")]
    [SerializeField] private GoalBehaviour goal1;
    [SerializeField] private GoalBehaviour goal2;
    [SerializeField] private GoalBehaviour goal3;

    [Header("Timer UI")]
    [SerializeField] private TMP_Text timerText;

    private Vector3 startingPlayerPosition;

    private bool playerPositionRecorded;
    private bool timerStarted;
    private bool timerStopped;

    private float elapsedTime;

    public bool TimerStarted
    {
        get { return timerStarted; }
    }

    public bool TimerStopped
    {
        get { return timerStopped; }
    }

    public float ElapsedTime
    {
        get { return elapsedTime; }
    }

    public int FinalTimeMilliseconds
    {
        get
        {
            return Mathf.RoundToInt(
                elapsedTime * 1000f
            );
        }
    }

    private void Start()
    {
        FindPlayer();

        if (playerTransform != null)
        {
            RecordStartingPlayerPosition();
        }

        elapsedTime = 0f;
        timerStarted = false;
        timerStopped = false;

        RefreshTimerText();
    }

    private void Update()
    {
        if (timerStopped)
        {
            return;
        }

        if (playerTransform == null)
        {
            FindPlayer();

            if (playerTransform == null)
            {
                return;
            }
        }

        if (!playerPositionRecorded)
        {
            RecordStartingPlayerPosition();
        }

        if (!timerStarted)
        {
            CheckForPlayerMovement();
            return;
        }

        if (AreAllGoalsCollected())
        {
            StopTimer();
            return;
        }

        elapsedTime += Time.unscaledDeltaTime;

        RefreshTimerText();
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    private void RecordStartingPlayerPosition()
    {
        startingPlayerPosition =
            playerTransform.position;

        playerPositionRecorded = true;
    }

    private void CheckForPlayerMovement()
    {
        Vector3 movement =
            playerTransform.position -
            startingPlayerPosition;

        // Ignore vertical movement so falling slightly at spawn
        // does not begin the timer.
        movement.y = 0f;

        float requiredDistanceSquared =
            movementThreshold * movementThreshold;

        if (movement.sqrMagnitude <
            requiredDistanceSquared)
        {
            return;
        }

        StartTimer();
    }

    public void StartTimer()
    {
        if (timerStarted || timerStopped)
        {
            return;
        }

        timerStarted = true;
        elapsedTime = 0f;

        RefreshTimerText();

        Debug.Log("Level 1 timer started.");
    }

    public void StopTimer()
    {
        if (!timerStarted || timerStopped)
        {
            return;
        }

        timerStopped = true;

        RefreshTimerText();

        Debug.Log(
            "Level 1 completed in " +
            FormatTime(elapsedTime)
        );
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        timerStarted = false;
        timerStopped = false;

        if (playerTransform != null)
        {
            RecordStartingPlayerPosition();
        }
        else
        {
            playerPositionRecorded = false;
        }

        RefreshTimerText();
    }

    private bool AreAllGoalsCollected()
    {
        if (goal1 == null ||
            goal2 == null ||
            goal3 == null)
        {
            return false;
        }

        return goal1.isCollected &&
               goal2.isCollected &&
               goal3.isCollected;
    }

    private void RefreshTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        timerText.text =
            FormatTime(elapsedTime);
    }

    public static string FormatTime(float timeInSeconds)
    {
        int totalMilliseconds =
            Mathf.Max(
                0,
                Mathf.RoundToInt(
                    timeInSeconds * 1000f
                )
            );

        int minutes =
            totalMilliseconds / 60000;

        int seconds =
            totalMilliseconds % 60000 / 1000;

        int milliseconds =
            totalMilliseconds % 1000;

        return
            $"{minutes:00}:" +
            $"{seconds:00}." +
            $"{milliseconds:000}";
    }
}