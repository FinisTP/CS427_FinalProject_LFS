using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
	public Transform player;
	public Transform destination;

	private bool playerIsOverlapping = false;

    private void Start()
    {
		// player = ThirdPersonMovement.LocalPlayerInstance.transform;
    }

    // Update is called once per frame
    void Update()
	{
		if (playerIsOverlapping)
		{
			player.position = destination.position;
			player.rotation = destination.rotation;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			ThirdPersonMovement tpm = other.GetComponentInParent<ThirdPersonMovement>();
			if (tpm != null)
            {
				player = tpm.transform;
				playerIsOverlapping = true;
			}
			
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player")
		{
			ThirdPersonMovement tpm = other.GetComponentInParent<ThirdPersonMovement>();
			if (tpm != null)
			{
				player = null;
				playerIsOverlapping = false;
			}
			
		}
	}
}
