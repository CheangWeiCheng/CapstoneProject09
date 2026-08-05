using Game.Audio;
using UnityEngine;

public class EnemyDetectionTrigger : MonoBehaviour
{
    private IEnemyAI enemyAI;
    
    void Start()
    {
        enemyAI = GetComponentInParent<IEnemyAI>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyAI.OnPlayerDetected(other.transform);
            AudioDirector.ReportCombatState(this, true);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyAI.OnPlayerLost();
            AudioDirector.ReportCombatState(this, false);
        }
    }

    private void OnDisable()
    {
        AudioDirector.ReportCombatState(this, false);
    }
}
