using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class HideNSeekController : MonoBehaviourPunCallbacks
{
    [Header("Match status")] // to be replaced with MatchPrefs
    public MatchPhase matchPhase = MatchPhase.IDLE;
    public float hideTime = 60f;
    public float seekTime = 300f;
    public Transform startPosHider;
    public Transform defeatPos;
    public Transform startPosSeeker;
    public TMP_Text CounterNumber;
    public TMP_Text Command;
    public TMP_Text HiderCount;
    public GameObject[] blockades;
    private float currentTime;
    private int foundPlayer = 0;
    private GameplayManager manager = null;

    private void Awake()
    {
        manager = GameplayManager.instance;
        manager.startPosition = startPosHider;
        manager.defeatedRoom = defeatPos;
        currentTime = 0;
    }

    private void Start()
    {
        // A match should have at least one seeker
        
        if (!PhotonNetwork.IsMasterClient) return;
        int seekerId = Random.Range(0, manager.playerList.Count);
        while (manager.playerList[seekerId] == null) seekerId = Random.Range(0, manager.playerList.Count);

        photonView.RPC("MatchInformationHideAndSeek", RpcTarget.All, seekerId);

        NetworkHelper.SetRoomProperty("Time", currentTime);
        StartCoroutine(HidePhase());
    }

    // Update is called once per frame
    void Update()
    {
        if (GameplayManager.instance.isGameOver) return;
        ThirdPersonMovement localTPM = ThirdPersonMovement.LocalPlayerInstance;
        HiderCount.text = $"{foundPlayer}/{manager.playerList.Count - 1} hiders found";
        switch (matchPhase)
        {
            case MatchPhase.HIDE:
                Command.text = "HIDE PHASE";
                // photonView.RPC("UpdateCounter", RpcTarget.All,);
                CounterNumber.text = ((int)(hideTime - currentTime)).ToString();
                if (localTPM.currentRole == global::Role.SEEKER)
                {
                    // Command.text = "Please patiently wait until hiding time runs out.";
                }
                else if (localTPM.currentRole == global::Role.HIDER)
                {
                    // Command.text = "Find a hiding place before time runs out!";
                }
                break;
            case MatchPhase.SEEK:
                Command.text = "SEEK PHASE";
                // photonView.RPC("UpdateCounter", RpcTarget.All, ((int)(manager.SeekTime - manager.currentTime)).ToString());
                CounterNumber.text = ((int)(seekTime - currentTime)).ToString();
                if (localTPM.currentRole == global::Role.SEEKER)
                {
                    // Command.text = "Find the prey and shoot to kill them before time runs out!";
                }
                else if (localTPM.currentRole == global::Role.HIDER)
                {
                    // Command.text = "Stay away from the hunter's sight!";
                }
                break;
            default:
                CounterNumber.text = "";
                Command.text = "";
                break;
        }
    }

    private IEnumerator HidePhase()
    {
        
        Command.text = "Hide Phase";
        currentTime = 0;
        while (currentTime < hideTime)
        {
            currentTime += 1f;
            NetworkHelper.SetRoomProperty("Time", currentTime);
            photonView.RPC("UpdateTimer", RpcTarget.All, currentTime);
            yield return new WaitForSeconds(1f);
        }
        photonView.RPC("ChangeMatchPhaseToSeek", RpcTarget.All);
        // matchPhase = MatchPhase.SEEK;
        StartCoroutine(SeekPhase());
    }

    [PunRPC]
    void ChangeMatchPhaseToSeek()
    {
        matchPhase = MatchPhase.SEEK;
        foreach (ThirdPersonMovement tpm in manager.playerList)
        {
            tpm.playerNickName.text = "";
        }
    }

    [PunRPC]
    void UpdateTimer(float time)
    {
        currentTime = time; // float.Parse(PhotonNetwork.CurrentRoom.CustomProperties["Time"].ToString());
    }

    [PunRPC]
    void MatchInformationHideAndSeek(int seekerId)
    {
        matchPhase = MatchPhase.HIDE;
        manager.playerList[seekerId].currentRole = Role.SEEKER;
        manager.playerList[seekerId].GrantSeekerBuff();
        manager.playerList[seekerId].transform.position = startPosSeeker.position;

        for (int i = 0; i < manager.playerList.Count; ++i)
        {
            if (manager.playerList[i] == null || i == seekerId) continue;
            manager.playerList[i].gameObject.transform.position = startPosHider.position;
            manager.playerList[i].currentRole = Role.HIDER;
        }
        ThirdPersonMovement.LocalPlayerInstance.AnnounceRole();
    }

    [PunRPC]
    public void FoundPlayer()
    {
        foundPlayer++;
        if (foundPlayer == manager.playerList.Count - 1)
        manager.WinGame("Seeker wins! All hiders have been found!");
        HiderCount.text = $"{foundPlayer}/{manager.playerList.Count - 1} players found";
    }

    [PunRPC]
    void OpenGate()
    {
        foreach (GameObject go in blockades)
        {
            go.SetActive(false);
        }
    }

    private IEnumerator SeekPhase()
    {
        matchPhase = MatchPhase.SEEK;
        Command.text = "Seek Phase";
        currentTime = 0;
        photonView.RPC("OpenGate", RpcTarget.All);
        while (currentTime < seekTime)
        {
            currentTime += 1f;
            NetworkHelper.SetRoomProperty("Time", currentTime);
            photonView.RPC("UpdateTimer", RpcTarget.Others, currentTime);
            yield return new WaitForSeconds(1f);
        }
        manager.WinGame("Time's up! Hiders won!");
    }
}
