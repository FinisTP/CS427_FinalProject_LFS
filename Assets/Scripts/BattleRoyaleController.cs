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
        currentPlayerCount = 0;
    }

    private void Update()
    {
        if (GameplayManager.instance.isGameOver) return;
        float hp = ThirdPersonMovement.LocalPlayerInstance.health;
        float maxHp = ThirdPersonMovement.LocalPlayerInstance.maxHealth;
        Vector3 scale = healthBar.GetComponent<RectTransform>().localScale;
        scale.x = hp / maxHp;
        healthBar.GetComponent<RectTransform>().localScale = scale;
        SetBullet(1);

        if (!PhotonNetwork.IsMasterClient) return;
        if (currentPlayerCount == PhotonNetwork.PlayerList.Length - 1) GameplayManager.instance.WinGame("Game over! Only 1 player remains!");
    }

    [PunRPC]
    void CommenceMatch()
    {
        bulletCount.text = "x0";
        playerCount.text = $"0/{PhotonNetwork.PlayerList.Length} players taken down";
        ThirdPersonMovement.LocalPlayerInstance.transform.position = startPositions[0].position;
        for (int i = 0; i < manager.playerList.Count; ++i)
        {
            if (i >= 4) return;
            if (manager.playerList[i] == null) continue;
            manager.playerList[i].gameObject.transform.position = startPositions[i].position;
        }

        ModalWindowPanel.Instance.ShowModal("Battle Royale game", null, "Welcome to the Battle Royale! Draw bullets from the crystal " +
            "and shoot down all of the remaining players before the force field closes up!", "Okay");
    }

    public void SetBullet(int amount)
    {
        bulletCount.text = "x" + ThirdPersonMovement.LocalPlayerInstance.bulletCount;
    }

    public void KillPlayer()
    {
        currentPlayerCount++;
        playerCount.text = $"{currentPlayerCount}/{PhotonNetwork.PlayerList.Length} players taken down";
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("CommenceMatch", RpcTarget.All);
    }
}
