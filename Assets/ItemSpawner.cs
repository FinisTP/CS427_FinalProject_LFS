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
    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient || !photonView.IsMine) return;
        InvokeRepeating("SpawnItem", 5f, spawnDelay);
    }

    public void SpawnItem()
    {
        foreach(ItemCount ic in itemCounts)
        {
            if (ic.currentCount < ic.maxCount)
            {
                int randomPos = Random.Range(0, spawnPositions.Length);
                while (spawnPositions[randomPos].childCount > 0)
                {
                    randomPos = Random.Range(0, spawnPositions.Length);
                }
                GameObject newObj = PhotonNetwork.Instantiate(ic.itemPrefabName, spawnPositions[randomPos].position, spawnPositions[randomPos].rotation);
                newObj.transform.SetParent(spawnPositions[randomPos]);
                ic.currentCount++;
            }
        }
    }

    public void DeleteItem(Interactable interactable)
    {
        foreach (ItemCount ic in itemCounts)
        {
            if (ic.type == interactable.itemType) ic.currentCount--;
        }
    }

}
