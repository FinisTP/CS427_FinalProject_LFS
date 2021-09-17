using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyDetection : MonoBehaviour
{
    public EnemyBehavior enemy;
    public float detectionRadius = 30f;
    public List<ThirdPersonMovement> playersInSight = new List<ThirdPersonMovement>();

    private void Update()
    {
        if (GameplayManager.instance.isGameOver) return;
        if (playersInSight.Count <= 0)
        {
            if (enemy.currentState == EnemyState.CHASE)
            {
                enemy.currentState = EnemyState.PATROL;
                enemy.GetRandomTarget();
            }
            
            return;
        }
        ThirdPersonMovement[] sorted = playersInSight.Where(player => !player.isDefeated).ToArray();
        if (sorted.Length > 0)
        {
            ThirdPersonMovement closest = sorted.OrderBy(player => Vector3.Distance(transform.position, player.transform.position)).First();
            enemy.Chase(closest.transform);
        }
        else
        {
            enemy.currentState = EnemyState.PATROL;
            return;
        }
        /*
        foreach (ThirdPersonMovement tpm in playersInSight)
        {
            if (tpm.isMoving)
            {
                RaycastHit hit;
                Ray sight = new Ray(transform.position, (tpm.transform.position - transform.position).normalized);
                if (Physics.Raycast(sight, out hit, detectionRadius))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        enemy.Chase(hit.collider.transform);
                    }
                }
            }
        }
        */
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playersInSight.Add(other.GetComponentInParent<ThirdPersonMovement>());

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playersInSight.Remove(other.GetComponentInParent<ThirdPersonMovement>());
    }
}
