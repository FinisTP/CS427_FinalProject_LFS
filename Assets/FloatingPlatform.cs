using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingPlatform : MonoBehaviour
{
    public Transform[] waypoints;
    public float waitTime;
    public float smoothSpeed = 1f;
    private int currentTarget;
    private void Start()
    {
        InvokeRepeating("ChangePosition", waitTime, waitTime);
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (Vector3.Distance(transform.position, waypoints[currentTarget].position) > 1f)
        {
            transform.position = Vector3.Lerp(transform.position, waypoints[currentTarget].position, Time.deltaTime * smoothSpeed);
        }
    }
    void ChangePosition()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        currentTarget = (currentTarget + 1) % waypoints.Length;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.root.parent = transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.GetComponentInParent<ThirdPersonMovement>().gameObject.transform.parent = null;
        }
    }
}
