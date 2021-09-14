using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class BattleRoyaleController : MonoBehaviourPunCallbacks
{
    public Transform[] startPositions;
    public Transform defeatedPosition;
    public float matchTime;
    private GameplayManager manager = null;

    public Image healthBar;
    public TMP_Text bulletCount;
    public TMP_Text playerCount;

    private float currentTime;
    private int currentPlayerCount;

    private void Awake()
    {
        manager = GameplayManager.instance;
        // manager.startPosition = startPosHider;
        manager.defeatedRoom = defeatedPosition;
        currentTime = 0;
    }

    private void Start()
    {
        ThirdPersonMovement.LocalPlayerInstance.transform.position = startPositions[0].position;
        for (int i = 0; i < manager.playerList.Count; ++i)
        {
            if (i >= 4) return;
            if (manager.playerList[i] == null) continue;
            manager.playerList[i].gameObject.transform.position = startPositions[i].position;
        }
    }
}
