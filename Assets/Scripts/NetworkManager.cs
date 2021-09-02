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
        return PhotonNetwork.CreateRoom(roomName, new RoomOptions());
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

    public override void OnCreatedRoom()
    {
        // base.OnCreatedRoom();
    }

    public override void OnJoinedRoom()
    {
        // base.OnJoinedRoom();
    }



}