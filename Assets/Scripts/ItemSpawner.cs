using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ItemCount
{
    public ItemType type;
    public int maxCount;
    public string itemPrefabName;
    [HideInInspector]
    public int currentCount;
}


public class ItemSpawner : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPositions;
    public ItemCount[] itemCounts;
    public float spawnDelay;
    private int totalOccupied = 0;
    private void Start()
    {
        // if (!PhotonNetwork.IsMasterClient) return;
        InvokeRepeating("SpawnItem", 5f, spawnDelay);
    }

    public void SpawnItem()
    {
        // if (!PhotonNetwork.IsMasterClient) return;
        // if (totalOccupied >= spawnPositions.Length) return;
        foreach(ItemCount ic in itemCounts)
        {
            if (ic.currentCount < ic.maxCount)
            {
                int time = 0;
                int randomPos = Random.Range(0, spawnPositions.Length);
                while (time < 10 && spawnPositions[randomPos].childCount > 0)
                {
                    time++;
                    randomPos = Random.Range(0, spawnPositions.Length);
                }
                GameObject newObj = PhotonNetwork.Instantiate("Props\\" + ic.itemPrefabName, spawnPositions[randomPos].position, spawnPositions[randomPos].rotation);
                newObj.transform.SetParent(spawnPositions[randomPos]);
                ic.currentCount++;
                totalOccupied++;
            }
        }
    }

    public void DeleteItem(Interactable interactable)
    {
        foreach (ItemCount ic in itemCounts)
        {
            if (ic.type == interactable.itemType)
            {
                ic.currentCount--;
                // totalOccupied--;
            }
        }
    }

}
