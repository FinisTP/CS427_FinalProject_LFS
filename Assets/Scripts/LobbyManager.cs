using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System;
public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager instance;
    public string PlayerList;

    public bool CanStartGame;
    private void Awake()
    {
        instance = this;
    }
}
