using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviourPunCallbacks
{

    [Header("Menus")]
    public GameObject settingsMenu;
    public GameObject instructionsMenu;
    public GameObject currentMenu;
    public GameObject mainMenu;
    public GameObject lobbyMenu;
    public string startScene;

    [Header("Main Menu")]
    public Button createRoomBtn;
    public Button joinRoomBtn;
    public TMP_Text usernameTag;

    private void Start()
    {
        if (createRoomBtn != null && joinRoomBtn != null)
        {
            createRoomBtn.interactable = false;
            joinRoomBtn.interactable = false;
        } 
    }

    private void Update()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public override void OnConnectedToMaster()
    {
        if (createRoomBtn != null && joinRoomBtn != null)
        {
            createRoomBtn.interactable = true;
            joinRoomBtn.interactable = true;
        }
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
        NetworkManager.instance.roomName = roomNameInput.text;
    }

    public void OnJoinRoomBtn(TMP_Text roomNameInput)
    {
        NetworkManager.instance.JoinRoom(roomNameInput.text);
        NetworkManager.instance.roomName = roomNameInput.text;
    }
    public void OnPlayerNameUpdate(TMP_Text playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
        usernameTag.text = "Player: " + playerNameInput.text;
    }
    
    public void OnLeaveLobbyBtn()
    {
        PhotonNetwork.LeaveRoom();
        SetMenu(mainMenu);
    }
}