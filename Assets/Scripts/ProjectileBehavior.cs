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
                Destroy(gameObject);
                print("Found yer!!");
            }
        }
        else if (other.CompareTag("Obstacle"))
        {
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

    private void OnDestroy()
    {
        // GetComponent<AudioSource>().Play();
        // GameplayManager.instance.soundPlayer.PlayClip("Laser", 0.5f);
        GameplayManager.instance.photonView.RPC("PlayEffectCommand", Photon.Pun.RpcTarget.All, "Smoke", transform.position);
    }
}
