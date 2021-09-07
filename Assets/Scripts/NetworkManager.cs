using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;
    public string roomName;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            gameObject.SetActive(false);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public bool CreateRoom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 4;
        return PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    public bool JoinRoom(string roomName)
    {
        if (PhotonNetwork.PlayerList.Length <= 4)
        {
            return PhotonNetwork.JoinRoom(roomName);
        }
        return false;
    }
    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }

    public void QuitGame()
    {
        PhotonNetwork.Disconnect();
        Application.Quit();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        ModalWindowPanel.Instance.ShowModal("Failed to join the room", null, message, "Okay");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        ModalWindowPanel.Instance.ShowModal("Room creation failed", null, message, "Okay");
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        LobbyManager.instance.UpdateLobby();
        PhotonNetwork.LoadLevel("Lobby");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        LobbyManager.instance.UpdateLobby();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        LobbyManager.instance.UpdateLobby();
    }




}