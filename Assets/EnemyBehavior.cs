using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    public Transform[] waypoints;
    public float timeBeforeNextWaypoint;
    private Transform currentTarget;
    private NavMeshAgent agent;
    private void Start()
    {
        currentTarget = waypoints[0];
        agent = GetComponent<NavMeshAgent>();
        InvokeRepeating("GetRandomTarget", 1f, 100f);
    }
    private void Update()
    {
        agent.SetDestination(currentTarget.position);
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid || Vector3.Distance(transform.position, currentTarget.position) < 3f)
        {
            GetRandomTarget();
        }
    }

    private void GetRandomTarget()
    {
        Transform temp = waypoints[Random.Range(0, waypoints.Length)];
        while (Vector3.Distance(currentTarget.position, temp.position) < 1f)
        {
            temp = waypoints[Random.Range(0, waypoints.Length)];
        }
        currentTarget = temp;
    }

}
