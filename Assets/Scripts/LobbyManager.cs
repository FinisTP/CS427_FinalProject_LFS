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
    public string PlayerList;

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
        PlayerList = "";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.IsMasterClient)
            {
                PlayerList += player.NickName + " (Host) \n";
            }
            else
            {
                PlayerList += player.NickName + " \n";
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
        // NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget., "Lobby");
    }
}
