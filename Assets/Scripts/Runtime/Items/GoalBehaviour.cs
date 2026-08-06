using Game.Audio;
using UnityEngine;

public class GoalBehaviour : MonoBehaviour
{
    [SerializeField] MeshRenderer statue;
    [SerializeField] Material newStatueMaterial;
    [SerializeField] DoorBehaviour door;
    [SerializeField] AudioClip goalAudioClip; // Reference to the AudioClip component for playing sounds
    [HideInInspector] public bool isCollected = false; // Flag to prevent double collection
    [SerializeField] bool isFinalGoal = false; // Flag to indicate if this is the final goal

    /// <summary>
    /// Method to collect the goal
    /// This method will be called when the player interacts with the goal
    /// It takes a PlayerBehaviour object as a parameter
    /// This allows the goal to modify the player's inventory
    /// The goal can be used to unlock locked doors
    /// The method is public so it can be accessed from other scripts
    /// </summary>

    public void Collect(PlayerBehaviour player)
    {
        // Logic for collecting the goal
        if (isCollected) return; // Prevent double collection
        if (!AudioDirector.TryPlayCollection(transform.position) && goalAudioClip)
        {
            AudioSource.PlayClipAtPoint(goalAudioClip, transform.position); // Play the goal collection sound
        }
        isCollected = true; // Mark as collected
        if (statue != null) statue.material = newStatueMaterial;
        if (door != null) door.UnlockDoor();
        StartCoroutine(player.FadeInAndOutOfBlack(Color.white, isFinalGoal));
        if (GetComponent<MeshRenderer>() != null)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect(other.GetComponent<PlayerBehaviour>());
        }
    }
}
