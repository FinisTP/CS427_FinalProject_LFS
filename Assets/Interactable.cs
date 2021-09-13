using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;

public enum ItemType
{
    GENERATOR,
    BATTERY,
    MORPH,
    BULLET,
    HEALTH
}

public class Interactable : MonoBehaviourPunCallbacks
{
    public ItemType itemType;
    public GameObject controlledLight;
    public string interactionText;
    public float interactionTime;
    public Material generatorFixed;
    public bool mobile = false;
    private bool _isInteracted = false;

    private Action<ThirdPersonMovement> onItemInteracted;

    // for generator
    public bool isActivated = false;

    private void Start()
    {
        switch (itemType)
        {
            case ItemType.BATTERY:
                onItemInteracted += InteractBattery;
                break;
            case ItemType.GENERATOR:
                onItemInteracted += InteractGenerator;
                break;
            case ItemType.MORPH:
                onItemInteracted += InteractMorph;
                break;
            case ItemType.HEALTH:
                onItemInteracted += InteractHealth;
                break;
            case ItemType.BULLET:
                onItemInteracted += InteractBullet;
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        if (mobile)
        transform.Rotate(new Vector3(20, 50, 20) * Time.deltaTime);
    }
    public void GetInteraction()
    {
        if (isActivated || _isInteracted) return;
        GameplayManager.instance.uiPlayer.ShowInteraction(interactionText);
    }

    public void StartInteraction(ThirdPersonMovement player)
    {
        if (_isInteracted) return;
        if (itemType == ItemType.GENERATOR && (isActivated || player.toolCount <= 0)) return;
        photonView.RPC("SetInteractionStatus", RpcTarget.All, true);
        StartCoroutine(InteractDelay(player));
    }


    [PunRPC]
    public void SetInteractionStatus(bool state)
    {
        _isInteracted = state;
    }

    private void InteractHealth(ThirdPersonMovement player)
    {
        // player.toolCount++;
        // FindObjectOfType<MatchController>().SetBatteryCount(player.toolCount);
        photonView.RPC("DestroyObject", RpcTarget.All);
    }

    private void InteractBullet(ThirdPersonMovement player)
    {
        // player.bulletCount++;
        // FindObjectOfType<MatchController>().SetBatteryCount(player.toolCount);
        photonView.RPC("DestroyObject", RpcTarget.All);
    }

    private void InteractMorph(ThirdPersonMovement player)
    {

    }

    private void InteractBattery(ThirdPersonMovement player)
    {
        player.toolCount++;
        FindObjectOfType<MatchController>().SetBatteryCount(player.toolCount);
        photonView.RPC("DestroyObject", RpcTarget.All);
    }
    
    [PunRPC]
    public void DestroyObject()
    {
        FindObjectOfType<ItemSpawner>().DeleteItem(this);
        Destroy(gameObject, 3f);
        gameObject.SetActive(false);
    }

    private void InteractGenerator(ThirdPersonMovement player)
    {
        if (player.toolCount <= 0 || isActivated) return;
        player.toolCount--;
        
        photonView.RPC("Generator", RpcTarget.All);
        MatchController temp = FindObjectOfType<MatchController>();
        temp.SetBatteryCount(player.toolCount);
        temp.ActivateGenerator();
        temp.photonView.RPC("SetEnergy", RpcTarget.All, temp.maxEnergy);
    }

    [PunRPC]
    public void Generator()
    {
        isActivated = true;
        GetComponent<Renderer>().material = generatorFixed;
        controlledLight.SetActive(true);
    }
    
    IEnumerator InteractDelay(ThirdPersonMovement player)
    {
        float currentTime = 0f;
        GameplayManager.instance.uiPlayer.ShowInteraction("Work in progress, stay still...");
        GameplayManager.instance.uiPlayer.StartInteraction();
        while (currentTime < interactionTime)
        {
            if (player == null) StopAllCoroutines();
            if (player.controller.velocity.magnitude > 0.1f)
            {
                GameplayManager.instance.uiPlayer.EndInteraction();
                photonView.RPC("SetInteractionStatus", RpcTarget.All, false);
                StopAllCoroutines();
            }
            currentTime += Time.deltaTime;
            GameplayManager.instance.uiPlayer.UpdateInteraction(currentTime, interactionTime);
            photonView.RPC("SetInteractionStatus", RpcTarget.All, false);
            yield return null;
        }
        GameplayManager.instance.uiPlayer.EndInteraction();
        onItemInteracted?.Invoke(player);
    }

    private void OnDestroy()
    {
        
    }

}
