using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System;

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
    public TMP_Text Username;

    public Action OnJoinRoomSuccess;

    private void Start()
    {
        createRoomBtn.interactable = false;
        joinRoomBtn.interactable = false;

        OnJoinRoomSuccess += OnJoinedRoomSuccess;
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
        bool res = NetworkManager.instance.CreateRoom(roomNameInput.text);
        NetworkManager.instance.roomName = roomNameInput.text;
        
        if (res) ModalWindowPanel.Instance.ShowModal("Confirm room creation", null,
            "Do you want to create and join the room " + roomNameInput.text + "?", "Yes", "No", OnJoinRoomSuccess);

        else ModalWindowPanel.Instance.ShowModal("Room creation failed", null,
            "The room with the name " + roomNameInput.text + " is already available. Please retry using a different name.", "Okay");
    }

    public void OnJoinRoomBtn(TMP_Text roomNameInput)
    {
        bool res = NetworkManager.instance.JoinRoom(roomNameInput.text);
        NetworkManager.instance.roomName = roomNameInput.text;

        /*

        if (res) ModalWindowPanel.Instance.ShowModal("Confirm room join", null,
             "Do you want to join the room " + roomNameInput.text + "?", "Yes", "No", OnJoinRoomSuccess);

        else 
            ModalWindowPanel.Instance.ShowModal("Room join failed", null,
            "The room with the name " + roomNameInput.text + " is not available. Please retry using a valid name.", "Okay");
        */
    }
    public void OnPlayerNameUpdate(TMP_Text playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
        Username.text = "Player: " + playerNameInput.text;
    }
    public void OnJoinedRoomSuccess()
    {
        // SetMenu(lobbyMenu);
        // NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, SceneToLoad);
        StartCoroutine(JoinRoom());
    }

    public override void OnCreatedRoom()
    {
        // PhotonNetwork.LoadLevel("Lobby");
    }

    public override void OnJoinedRoom()
    {
        LobbyManager.instance.UpdateLobby();
    }

    IEnumerator JoinRoom()
    {
        LobbyManager.instance.UpdateLobby();
        yield return new WaitForSeconds(1f);
        PhotonNetwork.LoadLevel("Lobby");
        yield return null;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        LobbyManager.instance.UpdateLobby();
    }
    
    public void OnLeaveLobbyBtn()
    {
        PhotonNetwork.LeaveRoom();
        SetMenu(mainMenu);
    }
    public void OnStartGameBtn()
    {
        // NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, SceneToLoad);
    }
}