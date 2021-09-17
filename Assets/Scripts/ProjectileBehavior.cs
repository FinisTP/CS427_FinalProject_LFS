using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    public ThirdPersonMovement owner;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponentInParent<ThirdPersonMovement>() != owner)
            {
                // TODO: found a hider!!
                other.GetComponentInParent<ThirdPersonMovement>().photonView.RPC("AddHealth", Photon.Pun.RpcTarget.All, -50f);
                GameplayManager.instance.photonView.RPC("PlayEffectCommand", Photon.Pun.RpcTarget.All, "Smoke", transform.position);
                Destroy(gameObject);
                print("Found yer!!");
            }
        }
        else if (other.CompareTag("Obstacle"))
        {
            GameplayManager.instance.photonView.RPC("PlayEffectCommand", Photon.Pun.RpcTarget.All, "Smoke", transform.position);
            Destroy(gameObject);
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
