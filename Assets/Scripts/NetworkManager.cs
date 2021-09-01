using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
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
    }
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public bool CreateRoom(string roomName)
    {
        return PhotonNetwork.CreateRoom(roomName);
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
}