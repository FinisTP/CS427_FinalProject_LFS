using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using TMPro;
using UnityEngine.UI;
public class GameplayManager : MonoBehaviourPunCallbacks
{
    [Header("Status")]
    public bool gameEnded = false;

    [Header("Players")]
    public string playerPrefabLocation;
    public Transform[] spawnPoints;
    public PlayerController[] players;
    private int playersInGame;
    private List<int> pickedSpawnIndex;
    public TMP_Text MicState;
    public PlayerController LocalPlayer;
    
    [Header("Reference")]
    public GameObject imageTarget;

    [Header("Microphone")]
    public Image MicImage;
    public Sprite MuteIcon;
    public Sprite UnmuteIcon;

    //instance
    public static GameplayManager instance;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        pickedSpawnIndex = new List<int>();
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
        DefaultObserverEventHandler.isTracking = false;
    }
    private void Update()
    {
        
        Debug.Log("is tracking " + DefaultObserverEventHandler.isTracking);
        foreach (GameObject gameObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (gameObj.name == "Player(Clone)")
{
                gameObj.transform.SetParent(imageTarget.transform);
            }
        }
        for (int i = 1; i < imageTarget.transform.childCount; i++)
        {
            imageTarget.transform.GetChild(i).gameObject.SetActive(DefaultObserverEventHandler.isTracking);
        }
        
    }
    [PunRPC]
    void ImInGame()
    {
        playersInGame++;
        if (playersInGame == PhotonNetwork.PlayerList.Length)
        {
            SpawnPlayer();
        }
    }

    public void ToggleMic(bool state)
    {
        if (state)
        {
            MicState.text = "Is enabling voice transmission";
            MicImage.sprite = UnmuteIcon;
        }
        else
        {
            MicState.text = "Is muting";
            MicImage.sprite = MuteIcon;
        }
    }

    void SpawnPlayer()
    {
        int rand = Random.Range(0, spawnPoints.Length);
        while (pickedSpawnIndex.Contains(rand))
        {
            rand = Random.Range(0, spawnPoints.Length);
        }
        pickedSpawnIndex.Add(rand);
        GameObject playerObject = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[rand].position, Quaternion.identity);
        //intialize the player
        PlayerController playerScript = playerObject.GetComponent<PlayerController>();
        playerScript.photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }
    public PlayerController GetPlayer(int playerID)
    {
        return players.First(x => x.id == playerID);
    }
    public PlayerController GetPlayer(GameObject playerObj)
    {
        return players.First(x => x.gameObject == playerObj);
    }
}