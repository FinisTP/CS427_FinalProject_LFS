using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;

public class InteractableBehavior : MonoBehaviour
{
    public List<GameObject> objectsInRange = new List<GameObject>();
    private GameObject currentTargeted;
    public ThirdPersonMovement player;
    private void Update()
    {
        if (GameplayManager.instance.isGameOver) return;
        if (!player.photonView.IsMine) return;
        if (objectsInRange.Count == 0)
        {
            // print("No Object!");
            EndInteraction();
            return;
        }
        GameObject[] sorted = objectsInRange.Where(obj => obj != null && 
            Vector3.Distance(transform.position, obj.transform.position) < 10f).ToArray();
        if (sorted.Length <= 0)
        {
            objectsInRange.Clear();
            EndInteraction();
            return;
        }
        currentTargeted = sorted.OrderBy(obj => Vector3.Distance(transform.position, obj.transform.position)).First();
        if (currentTargeted != null)
        {
            // currentTargeted.layer = LayerMask.NameToLayer("Highlight");
            currentTargeted.GetComponent<Interactable>().GetInteraction();
            if (Input.GetKeyDown(KeyCode.E))
            {
                // interact with object
                currentTargeted.GetComponent<Interactable>().StartInteraction(player);
            }
        }
        else
        {
            
            EndInteraction();
            return;
        }
    }

    private void EndInteraction()
    {
        currentTargeted = null;
        
        GameplayManager.instance.uiPlayer.ShowInteraction("");
        GameplayManager.instance.uiPlayer.EndInteraction();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable"))
            objectsInRange.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            // other.gameObject.layer = LayerMask.NameToLayer("Interactable");
            objectsInRange.Remove(other.gameObject);
        }
    }
}
