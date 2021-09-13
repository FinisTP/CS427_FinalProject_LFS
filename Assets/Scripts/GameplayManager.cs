using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Players do task to keep machines working, bots find the players
// Turn into objects or hide behind objects
// Use monitor, radar to track enemies, survive till dawn

public enum MatchPhase
{
    IDLE,
    HIDE,
    SEEK,
    RESET
}

public class GameplayManager : MonoBehaviourPunCallbacks
{
    [Header("Managers")]
    public SoundManager soundPlayer;
    public ParticleManager particlePlayer;
    public UIManager uiPlayer;

    [Header("Match status")] // to be replaced with MatchPrefs
    public MatchPhase matchPhase = MatchPhase.IDLE;
    public float hideTime = 60f;
    public float seekTime = 300f;
    public Transform startPosition;
    public float currentTime { get { return _currentTime; } }
    private float _currentTime;

    [Header("Players")]
    public Transform[] spawnPoints; // Spawning in starting room
    public List<ThirdPersonMovement> playerList = new List<ThirdPersonMovement>();
    private List<int> _pickedSpawnIndex = new List<int>();
    private bool _toggleMouse = false;
    public List<string> playerPrefabs;
    public bool _isChatting = false;
    
    //instance
    public static GameplayManager instance = null;

    public int playerListSize = 0;
    public Transform defeatedRoom;
    

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _toggleMouse = false;

        InitializeLocalPlayer();
        uiPlayer.UpdatePlayerList();
        _currentTime = 0;
    }

    private void Update()
    {
        if (_isChatting) return;

        playerListSize = playerList.Count;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _toggleMouse = !_toggleMouse;
            if (_toggleMouse)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMicPlayer();
        }
    }

    public void StartMatchBattleRoyale()
    {
        
    }

    public void StartMatchSurvival()
    {
        // photonView.RPC("MatchInformationSurvival", RpcTarget.All);
    }


    public void StartMatchHideAndSeek()
    {
        // A match should have at least one seeker
        int seekerId = Random.Range(0, playerList.Count);
        while (playerList[seekerId] == null) seekerId = Random.Range(0, playerList.Count);

        photonView.RPC("MatchInformation", RpcTarget.All, seekerId);

        NetworkHelper.SetRoomProperty("Time", currentTime);
        StartCoroutine(HidePhase());
    }

    [PunRPC]
    void MatchInformation(int seekerId)
    {
        matchPhase = MatchPhase.HIDE;
        playerList[seekerId].currentRole = Role.SEEKER;
        playerList[seekerId].GrantSeekerBuff();
        playerList[seekerId].transform.position = startPosition.position;

        for (int i = 0; i < playerList.Count; ++i)
        {
            if (playerList[i] == null || i == seekerId) continue;
            playerList[i].gameObject.transform.position = startPosition.position;
            playerList[i].currentRole = Role.HIDER;
        }
        ThirdPersonMovement.LocalPlayerInstance.AnnounceRole();
    }

    [PunRPC]
    void UpdateTimer(float time)
    {
        _currentTime = time; // float.Parse(PhotonNetwork.CurrentRoom.CustomProperties["Time"].ToString());
    }

    [PunRPC]
    void ChangeMatchPhaseToSeek()
    {
        matchPhase = MatchPhase.SEEK;
        foreach (ThirdPersonMovement tpm in playerList)
        {
            tpm.playerNickName.text = "";
        }
    }

    [PunRPC]
    public void PlayEffectCommand(string name, Vector3 location)
    {
        particlePlayer.PlayEffectGlobal(name, location);
    }

    [PunRPC]
    void ResetMatch()
    {
        if (!photonView.IsMine || !PhotonNetwork.IsMasterClient) return;
        matchPhase = MatchPhase.RESET;
        PhotonNetwork.DestroyAll();
        PhotonNetwork.Destroy(GameplayManager.instance.gameObject);
        
        
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Lobby");
    }

    private IEnumerator HidePhase()
    {
        _currentTime = 0;
        while (_currentTime < hideTime)
        {
            _currentTime += 1f;
            NetworkHelper.SetRoomProperty("Time", _currentTime);
            photonView.RPC("UpdateTimer", RpcTarget.Others, _currentTime);
            yield return new WaitForSeconds(1f);
        }
        photonView.RPC("ChangeMatchPhaseToSeek", RpcTarget.All);
        // matchPhase = MatchPhase.SEEK;
        StartCoroutine(SeekPhase());
    }


    private IEnumerator SeekPhase()
    {
        _currentTime = 0;
        while (_currentTime < seekTime)
        {
            _currentTime += 1f;
            NetworkHelper.SetRoomProperty("Time", _currentTime);
            photonView.RPC("UpdateTimer", RpcTarget.Others, _currentTime);
            yield return new WaitForSeconds(1f);
        }
        WinGame();
    }

    public void WinGame()
    {
        photonView.RPC("ResetMatch", RpcTarget.MasterClient);
    }

    void InitializeLocalPlayer()
    {

        int rand = Random.Range(0, spawnPoints.Length);
        while (_pickedSpawnIndex.Contains(rand))
        {
            rand = Random.Range(0, spawnPoints.Length);
        }
        _pickedSpawnIndex.Add(rand);
        GameObject playerObject = PhotonNetwork.Instantiate(playerPrefabs[rand], spawnPoints[rand].position, Quaternion.identity);
        ThirdPersonMovement playerScript = playerObject.GetComponent<ThirdPersonMovement>();

        playerScript.photonView.RPC("Initialize", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
        // PhotonNetwork.LocalPlayer.CustomProperties.Add("PlayerMovement", playerScript);
        StartCoroutine(Wait());
        

    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(1);
        photonView.RPC("ReceiveUpdatedPlayerList", RpcTarget.All);
    }


    [PunRPC]
    public void ReceiveUpdatedPlayerList()
    {
        
        playerList.Clear();

        GameObject[] objPlayer = GameObject.FindGameObjectsWithTag("Player");
        List<ThirdPersonMovement> tpmTemp = new List<ThirdPersonMovement>();
        foreach(GameObject tpm in objPlayer)
        {
            tpm.TryGetComponent<ThirdPersonMovement>(out ThirdPersonMovement tmpFrag);
            if (tmpFrag != null)
            {
                tpmTemp.Add(tmpFrag);
            }
            
            
        }
        print("Length: " + tpmTemp.Count);
        ThirdPersonMovement[] tpms = tpmTemp.ToArray();
        ThirdPersonMovement[] tpmsSorted = new ThirdPersonMovement[tpms.Length];
        for (int i = 0; i < tpms.Length; ++i)
        {
            if (tpms[i] != null)
            {
                print(tpms[i].photonPlayer.ActorNumber);
                tpmsSorted[tpms[i].photonPlayer.ActorNumber - 1] = tpms[i];
            }
            
        }
        playerList = new List<ThirdPersonMovement>(tpmsSorted);
        print("Player count: " + playerList.Count);
    }

    public void ToggleMicPlayer()
    {
        uiPlayer.ToggleMic(ThirdPersonMovement.LocalPlayerInstance.gameObject.GetComponent<LocalMic>().ToggleMic());
    }

    public ThirdPersonMovement GetPlayer(int playerID)
    {
        foreach (ThirdPersonMovement tpm in playerList)
        {
            if (tpm != null && tpm.id == playerID) return tpm;
        }
        return null;
    }
    public ThirdPersonMovement GetPlayer(GameObject playerObj)
    {
        return playerList.First(x => x.gameObject == playerObj);
    }
}