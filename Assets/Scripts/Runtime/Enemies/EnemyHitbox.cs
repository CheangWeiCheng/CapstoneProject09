using Game.Audio;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [SerializeField] private Collider weaponCollider;
    [SerializeField] private bool colliderOffByDefault = true;
    [SerializeField] private int DamageAmount = 10;
    [SerializeField] private AudioCue hitCue = AudioCue.BasicAttackHit;
    [SerializeField] private float shortHitStopDuration = 0.05f;
    [SerializeField] private float longHitStopDuration = 0.1f;

    void Start()
    {
        if (colliderOffByDefault)
        {
            DeactivateHitbox();
        }
    }
    
    public void ActivateHitbox()
    {
        weaponCollider.enabled = true;
    }
    public void DeactivateHitbox()
    {
        weaponCollider.enabled = false;
    }

    public void SetHitCue(AudioCue cue)
    {
        hitCue = cue;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HealthBehaviour playerHealth = other.GetComponentInParent<HealthBehaviour>();
            PlayerBehaviour playerBehaviour = other.GetComponentInParent<PlayerBehaviour>();
            ThirdPersonController playerController = other.GetComponentInParent<ThirdPersonController>();
            if (playerController != null)
            {
                if (playerController.WasRecentlyDashing(0.1f))
                {
                    return;
                }
            }
            if (playerBehaviour != null && playerBehaviour.isDead)
            {
                return;
            }
            if (playerHealth != null)
            {
                playerHealth.ApplyDamage(playerBehaviour, DamageAmount);
                AudioDirector.TryPlayEnemyHit(hitCue, transform.position);
                HitStopManager.TriggerHitStop(shortHitStopDuration);
            }
            Debug.Log("Hit player for " + DamageAmount + " damage.");
            DeactivateHitbox();
        }
    }
}
