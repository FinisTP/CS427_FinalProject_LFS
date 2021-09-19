using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviourPunCallbacks
{
    [Header("Player List")]
    public GameObject PlayerListObj;
    public TMP_Text PlayerList;

    [Header("Microphone")]
    public Image MicImage;
    public Sprite MuteIcon;
    public Sprite UnmuteIcon;

    [Header("Interaction")]
    public TMP_Text interactionText;
    public GameObject interactionProgress;
    public TMP_Text gameOverText;

    [Header("Pause menu")]
    public GameObject PauseMenu;
    private GameObject currentMenu = null;

    public bool isPausing = false;

    private void Start()
    {
        PlayerListObj.SetActive(false);
        interactionText.text = "";
        gameOverText.text = "";
        interactionProgress.SetActive(false);
        PauseMenu.SetActive(false);
        isPausing = false;
    }
    public void ToggleMic(bool state)
    {
        if (state)
        {
            // MicState.text = "Is enabling voice transmission";
            MicImage.sprite = UnmuteIcon;
        }
        else
        {
            // MicState.text = "Is muting";
            MicImage.sprite = MuteIcon;
        }
    }

    [PunRPC]
    public void UpdatePlayerList()
    {
        PlayerList.text = LobbyManager.instance.playerList;
    }

    private void Update()
    {
        if (currentMenu == null) isPausing = false;
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        if (Input.GetKeyDown(KeyCode.F1))
        {
            PlayerListObj.SetActive(!PlayerListObj.activeInHierarchy);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentMenu == null)
            {
                currentMenu = PauseMenu;
                currentMenu.SetActive(true);
                isPausing = true;
            }
            else
            {
                DisableMenu();
            }
        }
    }

    public void DisableMenu()
    {
        currentMenu.SetActive(false);
        isPausing = false;
        currentMenu = null;
    }

    public void ToggleMenu(GameObject menu)
    {
        currentMenu.SetActive(false);
        currentMenu = menu;
        currentMenu.SetActive(true);
    }

    public void ShowInteraction(string text)
    {
        if (!string.IsNullOrEmpty(text))
            interactionText.text = "(E) " + text;
        else interactionText.text = "";
    }

    public void StartInteraction()
    {
        interactionProgress.SetActive(true);
        interactionProgress.GetComponent<Slider>().value = 0;
    }

    public void UpdateInteraction(float value, float maxValue)
    {
        interactionProgress.GetComponent<Slider>().value = value / maxValue;
    }

    [PunRPC]
    public void DisplayGameOverMessage(string msg)
    {
        gameOverText.text = msg;
    }

    public void EndInteraction()
    {
        if (interactionProgress == null) return;
        interactionProgress.SetActive(false);
    }

    public void LeaveCommand()
    {
        DisableMenu();
        
        PhotonNetwork.LeaveRoom();
        // PhotonNetwork.Disconnect();  
    }

    

    public void QuitGame()
    {
        PhotonNetwork.Disconnect();
        Application.Quit(0);
    }



}
