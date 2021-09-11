using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System;
public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager instance;
    public string playerList;

    public bool CanStartGame;
    private void Awake()
    {
        instance = this;
    }

    public void UpdateLobby()
    {
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }

    [PunRPC]
    public void UpdateLobbyUI()
    {
        playerList = "";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.IsMasterClient)
            {
                playerList += player.NickName + " (Host) \n";
            }
            else
            {
                playerList += player.NickName + " \n";
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            CanStartGame = true;
        }
        else
        {
            CanStartGame = false;
        }
        if (GameplayManager.instance != null) GameplayManager.instance.uiPlayer.UpdatePlayerList();
    }
}
