using Game.Audio;
using UnityEngine;
using System.Collections.Generic;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private float shortHitStopDuration = 0.05f;
    [SerializeField] private float longHitStopDuration = 0.1f;
    [SerializeField] private Attack attack; // Reference to the main brain
    [SerializeField] private ThirdPersonController playerController;
    [SerializeField] private Collider weaponCollider;
    
    // Track enemies hit during this attack
    private HashSet<EnemyBehaviour> hitEnemies = new HashSet<EnemyBehaviour>();
    private bool hasBounced; // To prevent multiple bounces on spike attack

    void Start()
    {
        DeactivateHitbox();
    }
    void Awake()
    {
        if (attack == null)
        {
            attack = FindFirstObjectByType<Attack>();
        }
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<ThirdPersonController>();
        }
    }
    public void ActivateHitbox()
    {
        weaponCollider.enabled = true;
        hitEnemies.Clear();
        hasBounced = false;
    }
    public void DeactivateHitbox()
    {
        weaponCollider.enabled = false;
    }
    private void ResetDashes()
    {
        if (playerController.dashUpgradeObtained)
        {
            playerController.availableDashes = 2;
        }
        else
        {
            playerController.availableDashes = 1;
        }
    }

    private void ResetJumps()
    {
        playerController.availableJumps = 1;
        playerController.availableAerialPushes = 1;
        playerController.availableChargeAttackJumps = 1;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent(out EnemyBehaviour enemy))
            {
                // Check if we've already hit this enemy during this attack
                if (hitEnemies.Contains(enemy))
                    return; // Already hit, skip
                
                // Add to hit list first (prevents re-entrancy issues)
                hitEnemies.Add(enemy);

                // Calculate knockback direction from player to enemy
                Vector3 knockbackDir = (other.transform.position - attack.transform.position).normalized;
                knockbackDir.y = 0; // Keep it on a flat plane

                // Use the data from the Weapon script through the Attack reference
                var weaponData = attack.currentWeapon;
                Rigidbody playerRB = playerController.GetComponent<Rigidbody>();

                float hitStopDuration = shortHitStopDuration; // Default hitstop duration

                switch (attack.currentAttackType)
                {
                    case Attack.AttackType.Finisher:
                        enemy.currentState = EnemyBehaviour.EnemyState.Knockback;
                        enemy.TakeDamage(weaponData.finisherDamage, weaponData.finisherPayback, true);
                        enemy.Knockback(knockbackDir, attack.defaultForce + playerController.targetVelocity.magnitude * 0.8f, true);
                        
                        ResetDashes();
                        hitStopDuration = longHitStopDuration; // Longer hitstop for finisher

                        break;
                        
                    case Attack.AttackType.Charged:
                        enemy.currentState = EnemyBehaviour.EnemyState.Knockback;
                        // Use charge level for damage
                        int damage;
                        int payback;
                        if (attack.chargeLevel >= 2)
                        {
                            damage = weaponData.chargeDamage;
                            payback = weaponData.chargePayback;
                        }
                        else
                        {
                            damage = weaponData.finisherDamage;
                            payback = weaponData.finisherPayback;
                        }
                        
                        enemy.TakeDamage(damage, payback);

                        // Calculate horizontal knockback with slide speed
                        float horizontalForce = attack.defaultForce + playerController.targetVelocity.magnitude * 0.8f;
                        Vector3 horizontalKnockback = knockbackDir * horizontalForce;
                        
                        // Calculate vertical knockback
                        Vector3 verticalKnockback = Vector3.up * playerController.doubleJumpForce;
                        
                        // Combine into final knockback vector
                        Vector3 totalKnockback = horizontalKnockback + verticalKnockback;
                        
                        // Pass the direction (normalized) and force (magnitude) separately
                        enemy.Knockback(totalKnockback.normalized, totalKnockback.magnitude, false);

                        ResetDashes();
                        playerController.pauseFastFall = false;
                        hitStopDuration = longHitStopDuration; // Longer hitstop for charged attack

                        break;
                        
                    case Attack.AttackType.Launcher:
                        if (attack.isCharging)
                        {
                            enemy.currentState = EnemyBehaviour.EnemyState.Spiked;
                            // Use charge level for damage
                            int varDamage;
                            int varPayback;
                            if (attack.chargeLevel >= 2)
                            {
                                varDamage = weaponData.chargeDamage;
                                varPayback = weaponData.chargePayback;
                            }
                            else
                            {
                                varDamage = weaponData.finisherDamage;
                                varPayback = weaponData.finisherPayback;
                            }
                            
                            enemy.TakeDamage(varDamage, varPayback);
                            hitStopDuration = longHitStopDuration; // Longer hitstop for charged launcher
                            enemy.launchTimer = enemy.launchDelay;
                        }
                        else
                        {
                            enemy.currentState = EnemyBehaviour.EnemyState.Knockback;
                            enemy.TakeDamage(weaponData.normalDamage, weaponData.normalPayback);
                        }
                        
                        // Calculate horizontal knockback with slide speed
                        float launcherHorizontalForce = playerController.targetVelocity.magnitude * 0.8f;
                        Vector3 launcherHorizontalKnockback = knockbackDir * launcherHorizontalForce;
                        
                        // Calculate vertical knockback
                        Vector3 launcherVerticalKnockback = Vector3.up * attack.appliedJuggleForce;
                        
                        // Combine into final knockback vector
                        Vector3 launcherTotalKnockback = launcherHorizontalKnockback + launcherVerticalKnockback;
                        enemy.Knockback(launcherTotalKnockback.normalized, launcherTotalKnockback.magnitude, false);

                        ResetDashes();

                        break;
                        
                    case Attack.AttackType.GroundSlam:
                        enemy.currentState = EnemyBehaviour.EnemyState.Knockback;
                        enemy.TakeDamage(weaponData.normalDamage, weaponData.normalPayback);
                        enemy.Knockback(Vector3.down, 10f, false);
                        break;
                        
                    case Attack.AttackType.DashSlam:
                        enemy.currentState = EnemyBehaviour.EnemyState.Knockback;
                        enemy.TakeDamage(weaponData.normalDamage, weaponData.normalPayback);
                        Vector3 dsHorizontalKnockback = knockbackDir * (attack.bounceForce + Mathf.Max(0, playerController.targetVelocity.magnitude - playerController.moveSpeed) * 0.8f);
                        Vector3 dsVerticalKnockback = Vector3.down * 10f;
                        Vector3 dsTotalKnockback = dsHorizontalKnockback + dsVerticalKnockback;
                        enemy.Knockback(dsTotalKnockback.normalized, dsTotalKnockback.magnitude, false);
                        break;
                    
                    case Attack.AttackType.Spike:
                        enemy.TakeDamage(weaponData.normalDamage, weaponData.normalPayback);
                        
                        Vector3 spikeHorizontalKnockback = knockbackDir * (1 + playerController.targetVelocity.magnitude * 0.8f);
                        
                        if (enemy.IsGrounded)
                        {
                            enemy.currentState = EnemyBehaviour.EnemyState.Knockback;
                            enemy.Knockback(spikeHorizontalKnockback.normalized, spikeHorizontalKnockback.magnitude, false);
                        }
                        else
                        {
                            enemy.currentState = EnemyBehaviour.EnemyState.Spiked;
                            Vector3 spikeVerticalKnockback = Vector3.down * 10f;
                            Vector3 spikeTotalKnockback = spikeHorizontalKnockback + spikeVerticalKnockback;
                            enemy.Knockback(spikeTotalKnockback.normalized, spikeTotalKnockback.magnitude, false);
                        }
                        
                        if (!hasBounced)
                        {
                            playerRB.linearVelocity = new Vector3(playerRB.linearVelocity.x, 0, playerRB.linearVelocity.z);
                            playerRB.AddForce(Vector3.up * playerController.doubleJumpForce, ForceMode.Impulse);
                            hasBounced = true; // Ensure we only bounce once per spike attack
                        }

                        ResetDashes();
                        ResetJumps();
                        playerController.StopAttacking();
                        playerController.pauseFastFall = false;

                        break;

                    case Attack.AttackType.BoundSpike:
                        int BSdamage;
                        int BSpayback;
                        if (attack.chargeLevel >= 2)
                        {
                            BSdamage = weaponData.chargeDamage;
                            BSpayback = weaponData.chargePayback;
                        }
                        else
                        {
                            BSdamage = weaponData.finisherDamage;
                            BSpayback = weaponData.finisherPayback;
                        }
                        enemy.TakeDamage(BSdamage, BSpayback);

                        Vector3 bsHorizontalKnockback = knockbackDir * (1 + playerController.targetVelocity.magnitude * 0.8f);
                        if (enemy.IsGrounded)
                        {
                            enemy.currentState = EnemyBehaviour.EnemyState.Knockback;
                            Vector3 bsVerticalKnockback = Vector3.up * (attack.launcherForce - 1);
                            Vector3 bsTotalKnockback = bsHorizontalKnockback + bsVerticalKnockback;
                            enemy.Knockback(bsTotalKnockback.normalized, bsTotalKnockback.magnitude, false);
                        }
                        else
                        {
                            enemy.currentState = EnemyBehaviour.EnemyState.Rebound; // Set state to Rebound to trigger launcher effect on landing
                            Vector3 bsVerticalKnockback = Vector3.down * 10f;
                            Vector3 bsTotalKnockback = bsHorizontalKnockback + bsVerticalKnockback;
                            enemy.Knockback(bsTotalKnockback.normalized, bsTotalKnockback.magnitude, false);
                        }

                        if (!hasBounced)
                        {
                            playerRB.linearVelocity = new Vector3(playerRB.linearVelocity.x, 0, playerRB.linearVelocity.z);
                            playerRB.AddForce(Vector3.up * playerController.doubleJumpForce, ForceMode.Impulse);
                            hasBounced = true;
                        }

                        ResetDashes();
                        ResetJumps();
                        playerController.StopAttacking();
                        playerController.pauseFastFall = false;
                        hitStopDuration = longHitStopDuration; // Longer hitstop for bound spike

                        break;

                    case Attack.AttackType.AerialPush:
                        enemy.currentState = EnemyBehaviour.EnemyState.Knockback;
                        enemy.TakeDamage(weaponData.normalDamage, weaponData.normalPayback);
                        
                        Vector3 apHorizontalPushback = knockbackDir * (attack.bounceForce + Mathf.Max(0, playerController.targetVelocity.magnitude - playerController.moveSpeed) * 0.8f);
                        Vector3 apVerticalPushback = Vector3.up * playerController.shortJumpForce;
                        Vector3 apKnockback = knockbackDir * (Mathf.Max(playerController.moveSpeed * 0.5f, playerController.targetVelocity.magnitude) * 0.8f);
                        enemy.Push(apHorizontalPushback, apVerticalPushback, apKnockback);

                        ResetDashes();

                        break;

                    case Attack.AttackType.WeakPush:
                        if (enemy.currentState != EnemyBehaviour.EnemyState.Knockback &&
                            enemy.currentState != EnemyBehaviour.EnemyState.Spiked &&
                            enemy.currentState != EnemyBehaviour.EnemyState.Rebound && enemy.IsGrounded)
                        {
                            enemy.currentState = EnemyBehaviour.EnemyState.Hitstun;
                        }
                        else
                        {
                            enemy.currentState = EnemyBehaviour.EnemyState.Knockback;
                        }
                        enemy.TakeDamage(weaponData.normalDamage, weaponData.normalPayback, true);
                        float wpKnockback = Mathf.Max(1f, playerController.targetVelocity.magnitude * 0.8f);
                        enemy.Knockback(knockbackDir, wpKnockback, true);

                        ResetDashes();

                        break;

                    case Attack.AttackType.Normal:
                    default:
                        if (enemy.currentState != EnemyBehaviour.EnemyState.Knockback &&
                            enemy.currentState != EnemyBehaviour.EnemyState.Spiked &&
                            enemy.currentState != EnemyBehaviour.EnemyState.Rebound && enemy.IsGrounded)
                        {
                            enemy.currentState = EnemyBehaviour.EnemyState.Hitstun;
                        }
                        else
                        {
                            enemy.currentState = EnemyBehaviour.EnemyState.Knockback;
                        }
                        enemy.TakeDamage(weaponData.normalDamage, weaponData.normalPayback, true);
                        float knockbackForce = Mathf.Max(1f, playerController.targetVelocity.magnitude * 0.8f);
                        enemy.Knockback(knockbackDir, knockbackForce, true);
                        
                        ResetDashes();

                        break;
                }
                AudioDirector.TryPlayMove(GetHitCue(), other.transform.position);

                // Apply hitstop
                HitStopManager.TriggerHitStop(hitStopDuration);
            }
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other); // For if the attack is triggered before the weapon has fully left the enemy's hitbox, ensuring they still get hit
    }

    private AudioCue GetHitCue()
    {
        switch (attack.currentAttackType)
        {
            case Attack.AttackType.Charged:
            case Attack.AttackType.Finisher:
            case Attack.AttackType.BoundSpike:
                return AudioCue.ChargedAttackHit;
            case Attack.AttackType.Launcher:
                return AudioCue.LauncherHit;
            case Attack.AttackType.GroundSlam:
            case Attack.AttackType.DashSlam:
            case Attack.AttackType.Spike:
                return AudioCue.GroundSlamHit;
            default:
                return AudioCue.BasicAttackHit;
        }
    }
}
