using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum MatchPhase
{
    Idle,
    Hide,
    Seek,
    Reset
}

public class GameplayManager : MonoBehaviourPunCallbacks
{
    [Header("Status")]
    public bool gameEnded = false;
    public UIManager uiManager;
    public MatchPhase CurrentMatchPhase;
    public float HideTime = 60f;
    public float SeekTime = 300f;

    [Header("Players")]
    public string playerPrefabLocation;
    public Transform[] spawnPoints;
    public ThirdPersonMovement[] players;
    private int playersInGame;
    private List<int> pickedSpawnIndex;
    // public TMP_Text MicState;
    public List<string> PlayerPrefabs;

    public Transform StartPosition;
    public SoundManager SoundPlayer;
    public ParticleManager ParticlePlayer;
    public float currentTime;

    public GameObject tempPrefab;
    
    //instance
    public static GameplayManager instance = null;

    private void OnLevelWasLoaded(int level)
    {
        if (level == SceneManager.GetSceneByName("Lobby").buildIndex)
        {
            SoundPlayer.PlayBGM(SoundPlayer.GetTrackFromName("Lobby"));
        }
        else if (level == SceneManager.GetSceneByName("Level1").buildIndex)
        {
            SoundPlayer.StopAllTrack();
        }
    }

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
        print("Manager called!");
    }
    private void Start()
    {
        // if (!photonView.IsMine) return;
        SoundPlayer.PlayBGM(SoundPlayer.GetTrackFromName("Lobby"));
        pickedSpawnIndex = new List<int>();
        players = new ThirdPersonMovement[4];
        // print(PhotonNetwork.PlayerList.Length);
        
        ImInGame();
        DefaultObserverEventHandler.isTracking = false;
        // photonView.RPC("ImInGame", RpcTarget.All);
        CurrentMatchPhase = MatchPhase.Idle;
        uiManager.UpdatePlayerList();
        currentTime = 0;

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // photonView.RPC("ImInGame", RpcTarget.All, newPlayer);
        // base.OnPlayerEnteredRoom(newPlayer);
        LobbyManager.instance.UpdateLobby();
        uiManager.UpdatePlayerList();

    }


    private void Update()
    {
        if (CurrentMatchPhase == MatchPhase.Hide)
        {

        } 
        else if (CurrentMatchPhase == MatchPhase.Seek)
        {

        }

    }
    [PunRPC]
    void StartMatch()
    {
        CurrentMatchPhase = MatchPhase.Hide;
        int seekerId = Random.Range(0, players.Length);
        while (players[seekerId] == null) seekerId = Random.Range(0, players.Length);
        for (int i = 0; i < players.Length; ++i)
        {
            if (players[i] == null) continue;
            if (i == seekerId)
            {
                players[i].CurrentRole = Role.Seeker;
                players[i].GrantSeekerBuff();
            }
            else players[i].CurrentRole = Role.Hider;
        }
        foreach (ThirdPersonMovement tpm in players)
        {
            if (tpm != null)
            {
                tpm.transform.position = StartPosition.position;
                tpm.StartPhase();
            }
            
        }
        StartCoroutine(HidePhase());
    }

    

    public ThirdPersonMovement GetLocalPlayer()
    {
        foreach (ThirdPersonMovement tpm in players)
        {
            if (tpm != null && tpm.photonPlayer.IsLocal) return tpm;
        }
        return null;
    }

    private IEnumerator HidePhase()
    {
        currentTime = 0;
        while (currentTime < HideTime)
        {
            currentTime += Time.deltaTime;
            yield return null;
        }
        CurrentMatchPhase = MatchPhase.Seek;
        StartCoroutine(SeekPhase());
    }

    private IEnumerator SeekPhase()
    {
        currentTime = 0;
        while (currentTime < SeekTime)
        {
            currentTime += Time.deltaTime;
            yield return null;
        }
        CurrentMatchPhase = MatchPhase.Reset;
    }

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;
        //if (playersInGame == PhotonNetwork.PlayerList.Length)
        //{
            SpawnPlayer();
        //}
    }

    void SpawnPlayer()
    {
        int rand = Random.Range(0, spawnPoints.Length);
        // int rand2 = Random.Range(0, PlayerPrefabs.Length);
        while (pickedSpawnIndex.Contains(rand))
        {
            rand = Random.Range(0, spawnPoints.Length);
        }
        pickedSpawnIndex.Add(rand);
        GameObject playerObject = PhotonNetwork.Instantiate(PlayerPrefabs[0], spawnPoints[rand].position, Quaternion.identity);
        //intialize the player
        ThirdPersonMovement playerScript = playerObject.GetComponent<ThirdPersonMovement>();
        // playerScript.Initialize(PhotonNetwork.LocalPlayer);
        playerScript.photonView.RPC("Initialize", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer);
        
    }

    public void ToggleMicPlayer()
    {
        bool res = GetLocalPlayer().gameObject.GetComponent<LocalMic>().ToggleMic();
        uiManager.ToggleMic(res);
    }

    public ThirdPersonMovement GetPlayer(int playerID)
    {
        return players.First(x => x.id == playerID);
    }
    public ThirdPersonMovement GetPlayer(GameObject playerObj)
    {
        return players.First(x => x.gameObject == playerObj);
    }

    public void BackToLobby()
    {
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Lobby");
    }

}