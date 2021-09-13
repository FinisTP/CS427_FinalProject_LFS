using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public enum PortalType
{
    SURVIVAL,
    HIDENSEEK,
    ROYALE
}

public class PortalBehavior : MonoBehaviourPunCallbacks
{
    private Action OnConfirmStartGame;
    private bool startedGame = false;
    public GameObject Counter;
    public TMP_Text CounterNumber;
    public PortalType portalType;
    private void Start()
    {
        OnConfirmStartGame += StartGame;
        Counter.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !startedGame)
        {
            if (!photonView.IsMine) return;
            if (other.GetComponentInParent<ThirdPersonMovement>().isMasterClient && ValidGameState())
            {
                ModalWindowPanel.Instance.ShowModal("Start the match", null, "Do you want to start the match now?", "Yes", "No",
                    OnConfirmStartGame);
            } else
            {
                ModalWindowPanel.Instance.ShowModal("Start the match", null, 
                    "Only the host can start the game when there are two or more players!", "Okay");
            }
        }
    }

    private bool ValidGameState()
    {
        return true; // PhotonNetwork.PlayerList.Length >= 2;
    }

    [PunRPC]
    private void ShowCountdown()
    {
        Counter.SetActive(true);
        StartCoroutine(CountdownMatchStart());
    }

    private void StartGame()
    {
        startedGame = true;
        photonView.RPC("ShowCountdown", RpcTarget.All);
        
    }

    private IEnumerator CountdownMatchStart()
    {
        // yield return new WaitForSeconds(1f);
        CounterNumber.text = "3";
        yield return new WaitForSeconds(1f);
        CounterNumber.text = "2";
        yield return new WaitForSeconds(1f);
        CounterNumber.text = "1";
        yield return new WaitForSeconds(1f);
        CounterNumber.text = "Ready!";
        yield return new WaitForSeconds(1f);
        Commence();
    }

    private void Commence()
    {
        if (photonView.IsMine && PhotonNetwork.IsMasterClient)
        {
            switch (portalType)
            {
                case PortalType.HIDENSEEK:
                    GameplayManager.instance.StartMatchHideAndSeek();
                    break;
                case PortalType.ROYALE:
                    GameplayManager.instance.StartMatchBattleRoyale();
                    break;
                case PortalType.SURVIVAL:
                    GameplayManager.instance.StartMatchSurvival();
                    break;
            }
            
        }

        switch (portalType)
        {
            case PortalType.HIDENSEEK:
                NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "HideAndSeek");
                break;
            case PortalType.ROYALE:
                NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "BattleRoyale");
                break;
            case PortalType.SURVIVAL:
                NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Survival");
                break;
        }

        
    }
}
