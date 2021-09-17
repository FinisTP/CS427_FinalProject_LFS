using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState {
    PATROL,
    INVESTIGATE,
    CHASE
}

public class EnemyBehavior : MonoBehaviour
{
    public Transform[] waypoints;
    public float timeBeforeNextWaypoint;
    public Vector3 currentTarget;
    private NavMeshAgent agent;
    public EnemyState currentState;
    private void Awake()
    {
        currentTarget = waypoints[0].position;
        agent = GetComponent<NavMeshAgent>();
        InvokeRepeating("GetRandomTarget", 1f, 100f);
    }
    private void Update()
    {
        if (currentTarget == null) return;
        agent.SetDestination(currentTarget);
        if (currentState == EnemyState.CHASE)
        {
            agent.speed = 30f;
            return;
        }
        else agent.speed = 10f;
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial
            || Vector3.Distance(transform.position, currentTarget) < 3f)
        {
            GetRandomTarget();
        }
    }

    public void GetRandomTarget()
    {
        if (currentState == EnemyState.PATROL)
        {
            currentTarget = waypoints[Random.Range(0, waypoints.Length)].position;
            while (Vector3.Distance(transform.position, currentTarget) < 1f)
            {
                currentTarget = waypoints[Random.Range(0, waypoints.Length)].position;
            }
        }
        
    }

    public void Chase(Transform player)
    {
        currentState = EnemyState.CHASE;
        currentTarget = player.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponentInParent<ThirdPersonMovement>().SetDefeated(15f);
        }
    }

}
