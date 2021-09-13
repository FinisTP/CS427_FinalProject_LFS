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

    [Header("Interaction")]
    public TMP_Text interactionText;
    public GameObject interactionProgress;

    private void Start()
    {
        PlayerListObj.SetActive(false);
        interactionText.text = "";
        interactionProgress.SetActive(false);
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

    public void EndInteraction()
    {
        interactionProgress.SetActive(false);
    }

}
