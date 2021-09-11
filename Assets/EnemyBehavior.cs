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
    private Transform currentTarget;
    private NavMeshAgent agent;
    public EnemyState currentState;
    private void Awake()
    {
        currentTarget = waypoints[0];
        agent = GetComponent<NavMeshAgent>();
        InvokeRepeating("GetRandomTarget", 1f, 100f);
    }
    private void Update()
    {
        agent.SetDestination(currentTarget.position);
        if (currentState == EnemyState.CHASE)
        {
            agent.speed = 50f;
            return;
        }
        else agent.speed = 10f;
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial
            || Vector3.Distance(transform.position, currentTarget.position) < 3f)
        {
            GetRandomTarget();
        }
    }

    private void GetRandomTarget()
    {
        if (currentState == EnemyState.PATROL)
        {
            currentTarget = waypoints[Random.Range(0, waypoints.Length)];
            while (Vector3.Distance(transform.position, currentTarget.position) < 1f)
            {
                currentTarget = waypoints[Random.Range(0, waypoints.Length)];
            }
        }
        
    }

    public void Chase(Transform player)
    {
        currentState = EnemyState.CHASE;
        currentTarget = player;
    }

}
