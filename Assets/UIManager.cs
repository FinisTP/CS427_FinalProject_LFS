using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Player List")]
    public TMP_Text PlayerList;

    [Header("Microphone")]
    public Image MicImage;
    public Sprite MuteIcon;
    public Sprite UnmuteIcon;
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

    public void UpdatePlayerList(string[] playerNames)
    {
        PlayerList.text = "";
        foreach (string name in playerNames)
        {

        }
    }
}
