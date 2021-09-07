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
    public MatchPhase CurrentMatchPhase = MatchPhase.Idle;
    public float HideTime = 60f;
    public float SeekTime = 300f;

    [Header("Players")]
    public string playerPrefabLocation;
    public Transform[] spawnPoints;
    public List<ThirdPersonMovement> PlayerList = new List<ThirdPersonMovement>();
    // public ThirdPersonMovement[] players;
    private int playersInGame;
    private List<int> pickedSpawnIndex = new List<int>();
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

    }

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }
    private void Start()
    {
        ImInGame();
        uiManager.UpdatePlayerList();
        currentTime = 0;
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
        // Selecting random role in the master client side, then pass them on to all other players
        CurrentMatchPhase = MatchPhase.Hide;
        int seekerId = Random.Range(0, PlayerList.Count);
        while (PlayerList[seekerId] == null) seekerId = Random.Range(0, PlayerList.Count);
        for (int i = 0; i < PlayerList.Count; ++i)
        {
            if (PlayerList[i] == null) continue;
            if (i == seekerId)
            {
                PlayerList[i].CurrentRole = Role.Seeker;
                PlayerList[i].GrantSeekerBuff();
            }
            else PlayerList[i].CurrentRole = Role.Hider;

            if (!PlayerList[i].photonPlayer.CustomProperties.ContainsKey("Role"))
            {
                PlayerList[i].photonPlayer.CustomProperties.Add("Role", PlayerList[i].CurrentRole);
            }
            else PlayerList[i].photonPlayer.CustomProperties["Role"] = PlayerList[i].CurrentRole;
        }
        NetworkHelper.SetRoomProperty("Time", currentTime);
        StartCoroutine(HidePhase());
        photonView.RPC("MatchInformation", RpcTarget.All);
    }

    [PunRPC]
    void MatchInformation()
    {
        ThirdPersonMovement.LocalPlayerInstance.CurrentRole = (Role)ThirdPersonMovement.LocalPlayerInstance.photonPlayer.CustomProperties["Role"];
        if (ThirdPersonMovement.LocalPlayerInstance.CurrentRole == Role.Hider)
        {
            ModalWindowPanel.Instance.ShowModal("Match Started!", null, "You are the prey in this match! You have 60 seconds to find " +
                "a place to hide before the hunter wakes up!", "Okay");
        }
        else if (ThirdPersonMovement.LocalPlayerInstance.CurrentRole == Role.Seeker)
        {
            ModalWindowPanel.Instance.ShowModal("Match Started!", null, "You are the hunter in this match! You can start seeking the prey " +
                "after 60 seconds!", "Okay");
        }
        // ThirdPersonMovement.LocalPlayerInstance.StartPhase();
    }

    private IEnumerator HidePhase()
    {
        currentTime = 0;
        while (currentTime < HideTime)
        {
            currentTime += Time.deltaTime;
            NetworkHelper.SetRoomProperty("Time", currentTime);
            photonView.RPC("UpdateTimer", RpcTarget.All);
            yield return null;
        }
        CurrentMatchPhase = MatchPhase.Seek;
        StartCoroutine(SeekPhase());
    }

    [PunRPC]
    void UpdateTimer()
    {
        currentTime = float.Parse(PhotonNetwork.CurrentRoom.CustomProperties["Time"].ToString());
    }

    private IEnumerator SeekPhase()
    {
        currentTime = 0;
        while (currentTime < SeekTime)
        {
            currentTime += Time.deltaTime;
            NetworkHelper.SetRoomProperty("Time", currentTime);
            photonView.RPC("UpdateTimer", RpcTarget.All, currentTime);
            yield return null;
        }
        CurrentMatchPhase = MatchPhase.Reset;
    }

    void ImInGame()
    {
        // Pick a random spawn position for new player
        
        if (ThirdPersonMovement.LocalPlayerInstance == null)
        {
            int rand = Random.Range(0, spawnPoints.Length);
            while (pickedSpawnIndex.Contains(rand))
            {
                rand = Random.Range(0, spawnPoints.Length);
            }
            pickedSpawnIndex.Add(rand);
            GameObject playerObject = PhotonNetwork.Instantiate(PlayerPrefabs[rand], spawnPoints[rand].position, Quaternion.identity);
            ThirdPersonMovement playerScript = playerObject.GetComponent<ThirdPersonMovement>();

            playerScript.Initialize(PhotonNetwork.LocalPlayer);
            PhotonNetwork.LocalPlayer.CustomProperties.Add("PlayerMovement", playerScript);
         
            
            photonView.RPC("ReceiveUpdatedPlayerList", RpcTarget.All);
        }
        
    }

    [PunRPC]
    void ReceiveUpdatedPlayerList()
    {
        PlayerList.Clear();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; ++i)
        {
            print("Getting player list!");
            PlayerList.Add(PhotonNetwork.PlayerList[i].CustomProperties["PlayerMovement"] as ThirdPersonMovement);
            // ThirdPersonMovement tpm = PhotonNetwork.PlayerList[i].CustomProperties["PlayerMovement"] as ThirdPersonMovement;
            // print(tpm.id);
        }
    }

    public void ToggleMicPlayer()
    {
        bool res = ThirdPersonMovement.LocalPlayerInstance.gameObject.GetComponent<LocalMic>().ToggleMic();
        uiManager.ToggleMic(res);
    }

    public int GetIdOfPlayer(ThirdPersonMovement player)
    {
        for (int i = 0; i < PlayerList.Count; ++i)
            if (PlayerList[i].gameObject == player.gameObject) return i;
        return -1;
    }

    public ThirdPersonMovement GetPlayer(int playerID)
    {
        foreach (ThirdPersonMovement tpm in PlayerList)
        {
            if (tpm == null) print("Its nulklll");
            else print(tpm.id);
            if (tpm != null && tpm.id == playerID) return tpm;
        }
        return null;
    }
    public ThirdPersonMovement GetPlayer(GameObject playerObj)
    {
        return PlayerList.First(x => x.gameObject == playerObj);
    }
    public void BackToLobby()
    {
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Lobby");
    }

}