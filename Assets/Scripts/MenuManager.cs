using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
public class MenuManager : MonoBehaviourPunCallbacks
{
    [Header("Menus")]
    public GameObject currentMenu;
    public GameObject mainMenu;
    public GameObject lobbyMenu;
    public string SceneToLoad;
    [Header("Main Menu")]
    public Button createRoomBtn;
    public Button joinRoomBtn;
    [Header("Lobby Menu")]
    public TMP_Text roomName;
    public TMP_Text playerList;
    public Button startGameBtn;
    private void Start()
    {
        createRoomBtn.interactable = false;
        joinRoomBtn.interactable = false;
    }
    public override void OnConnectedToMaster()
    {
        createRoomBtn.interactable = true;
        joinRoomBtn.interactable = true;
    }
    public void SetMenu(GameObject menu)
    {
        currentMenu.SetActive(false);
        menu.SetActive(true);
        currentMenu = menu;
    }
    public void OnCreateRoomBtn(TMP_Text roomNameInput)
    {
        NetworkManager.instance.CreateRoom(roomNameInput.text);
        roomName.text = roomNameInput.text;
    }
    public void OnJoinRoomBtn(TMP_Text roomNameInput)
    {
        NetworkManager.instance.JoinRoom(roomNameInput.text);
        roomName.text = roomNameInput.text;
    }
    public void OnPlayerNameUpdate(TMP_Text playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }
    public override void OnJoinedRoom()
    {
        SetMenu(lobbyMenu);
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
    }
    [PunRPC]
    public void UpdateLobbyUI()
    {
        playerList.text = "";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.IsMasterClient)
            {
                playerList.text += player.NickName + " (Host) \n";
            }
            else
            {
                playerList.text += player.NickName + "\n";
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            startGameBtn.interactable = true;
        }
        else
        {
            startGameBtn.interactable = false;
        }
    }
    public void OnLeaveLobbyBtn()
    {
        PhotonNetwork.LeaveRoom();
        SetMenu(mainMenu);
    }
    public void OnStartGameBtn()
    {
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, SceneToLoad);
    }
}