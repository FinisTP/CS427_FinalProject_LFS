using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Player List")]
    public GameObject PlayerListObj;
    public TMP_Text PlayerList;

    [Header("Microphone")]
    public Image MicImage;
    public Sprite MuteIcon;
    public Sprite UnmuteIcon;

    private void Start()
    {
        PlayerListObj.SetActive(false);
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

    public void UpdatePlayerList()
    {
        PlayerList.text = LobbyManager.instance.playerList;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            PlayerListObj.SetActive(!PlayerListObj.activeInHierarchy);
        }
    }

}
