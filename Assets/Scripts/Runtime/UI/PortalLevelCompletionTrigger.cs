using UnityEngine;

public class PortalLevelCompletionTrigger : MonoBehaviour
{
    [Header("Level Timer")]
    [SerializeField]
    private LevelTimer levelTimer;

    [Header("Player Detection")]
    [SerializeField]
    private string playerTag = "Player";

    private bool levelCompleted;

    private void Awake()
    {
        if (levelTimer == null)
        {
            levelTimer =
                FindObjectOfType<LevelTimer>();
        }
    }

    private void OnTriggerEnter(
        Collider other
    )
    {
        if (levelCompleted)
        {
            return;
        }

        if (!IsPlayer(other))
        {
            return;
        }

        if (levelTimer == null)
        {
            Debug.LogError(
                "PortalLevelCompletionTrigger could not " +
                "find the LevelTimer."
            );

            return;
        }

        levelCompleted = true;

        levelTimer.CompleteLevel();

        Debug.Log(
            "Player entered the completion portal."
        );
    }

    private bool IsPlayer(
        Collider other
    )
    {
        if (other == null)
        {
            return false;
        }

        Transform currentTransform =
            other.transform;

        while (currentTransform != null)
        {
            if (currentTransform.CompareTag(
                playerTag
            ))
            {
                return true;
            }

            currentTransform =
                currentTransform.parent;
        }

        return false;
    }
}