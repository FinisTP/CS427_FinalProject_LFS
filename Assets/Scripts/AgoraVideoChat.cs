using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using Photon.Pun;
using Photon.Realtime;
using System;

public class AgoraVideoChat : MonoBehaviourPunCallbacks
{
    private string appID = "f6b9135c864e4ca4bc2cf393fbfddf74";
    [SerializeField]
    private string channel = "unity3d";
    private string originalChannel;
    private IRtcEngine mRtcEngine = null;
    [SerializeField] private uint myUID = 0;
    private GameObject videoObject;
    private bool inited = false;

    // [SerializeField]
    // private RectTransform content;
    [SerializeField]
    private Transform spawnPoint;
    [SerializeField]
    private float spaceBetweenUserVideos = 150f;
    private List<GameObject> playerVideoList = new List<GameObject>();
    private int cameraPermission = 0;

    void Start()
    {
        
        if (!photonView.IsMine)
        {
            return;
        }
        
        // IRtcEngine.Destroy();
        // Setup Agora Engine and Callbacks.
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();
        }
        
        originalChannel = channel;
        mRtcEngine = IRtcEngine.GetEngine(appID);
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccessHandler;
        mRtcEngine.OnUserJoined = OnUserJoinedHandler;
        mRtcEngine.OnLeaveChannel = OnLeaveChannelHandler;
        mRtcEngine.OnUserOffline = OnUserOfflineHandler;
        mRtcEngine.DisableAudio();
        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();
        mRtcEngine.JoinChannel(channel, null, 0);
        print("Initialized");
        
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            photonView.RPC("ToggleShareVideo", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void ToggleShareVideo()
    {
        if (videoObject != null)
        {
            videoObject.SetActive(!videoObject.activeInHierarchy);
            VideoSurface newVideoSurface = videoObject.GetComponent<VideoSurface>();
            // newVideoSurface.SetForUser(myUID);
            newVideoSurface.SetEnable(true);
        }
    }

    private void OnDestroy()
    {
        if (mRtcEngine != null)
        {
            mRtcEngine.LeaveChannel();
            mRtcEngine = null;
            IRtcEngine.Destroy();
        }
    }

    private void OnApplicationQuit()
    {
        if (mRtcEngine != null)
        {
            mRtcEngine.LeaveChannel();
            mRtcEngine = null;
            IRtcEngine.Destroy();
        }
    }

    // Local Client Joins Channel.
    private void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        
        if (!photonView.IsMine)
            return;
        print("Called locally!");
        myUID = uid;
        Debug.LogFormat("I: {0} joined channel: {1}.", uid.ToString(), channelName);
        // photonView.RPC("CreateUserVideoSurface", RpcTarget.AllBuffered, (int)uid, true);
        // photonView.RPC("SetInited", RpcTarget.AllBuffered);
        CreateUserVideoSurface((int)uid, true);
        // inited = true;
    }

    // Remote Client Joins Channel.
    private void OnUserJoinedHandler(uint uid, int elapsed)
    {
        // print("Called!");
        if (!photonView.IsMine)
            return;
        print("Called client!");

        // GameObject go = GameObject.Find(uid.ToString());
        // if (go != null) return;
        
        // photonView.RPC("CreateUserVideoSurface", RpcTarget.AllBuffered, (int)uid, false);
        // photonView.RPC("SetInited", RpcTarget.AllBuffered);
        CreateUserVideoSurface((int)uid, false);
        // inited = true;
    }

    [PunRPC]
    void SetInited()
    {
        inited = true;
    }

    // Local user leaves channel.
    private void OnLeaveChannelHandler(RtcStats stats)
    {
        if (!photonView.IsMine)
            return;
        foreach (GameObject player in playerVideoList)
        {
            Destroy(player.gameObject);
        }
        playerVideoList.Clear();
    }

    // Remote User Leaves the Channel.
    private void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        if (!photonView.IsMine)
            return;
    }

    [Header("Player Video Panel Properties")]
    [SerializeField]
    private GameObject userVideoPrefab;


    private int Offset = 100;
    [PunRPC]
    public void CreateUserVideoSurface(int uid, bool isLocalUser)
    {
        myUID = unchecked((uint)uid);
        print(gameObject.name + " - ID: " + uid.ToString());
        // Avoid duplicating Local player video screen
        for (int i = 0; i < playerVideoList.Count; i++)
        {
            if (playerVideoList[i].name == uid.ToString())
            {
                return;
            }
        }
        // Get the next position for newly created VideoSurface
        // float spawnY = playerVideoList.Count * spaceBetweenUserVideos;
        // Vector3 spawnPosition = new Vector3(0, -spawnY, 0);
        // Create Gameobject holding video surface and update properties
        // if (videoObject != null) return;
        videoObject = Instantiate(userVideoPrefab, spawnPoint.position, spawnPoint.rotation);
        
        if (videoObject == null)
        {
            Debug.LogError("CreateUserVideoSurface() - newUserVideoIsNull");
            return;
        }
        videoObject.transform.rotation = Quaternion.identity;
        videoObject.name = uid.ToString();
        videoObject.transform.SetParent(spawnPoint, false);
        videoObject.transform.localPosition = new Vector3(0, 0, 0);

        videoObject.transform.localScale = new Vector3(1, 1, 1);
        // newUserVideo.transform.rotation = Quaternion.Euler(Vector3.right * -180);
        playerVideoList.Add(videoObject);
        // Update our VideoSurface to reflect new users
        VideoSurface newVideoSurface = videoObject.GetComponent<VideoSurface>();
        
        if (newVideoSurface == null)
        {
            Debug.LogError("CreateUserVideoSurface() - VideoSurface component is null on newly joined user");
        }
        if (isLocalUser == false)
        {
            newVideoSurface.SetForUser(unchecked((uint)uid));
        }
        newVideoSurface.SetEnable(true);

        newVideoSurface.SetGameFps(30);
        videoObject.SetActive(false);

        // inited = true;
        // Update our "Content" container that holds all the image planes
        // content.sizeDelta = new Vector2(0, playerVideoList.Count * spaceBetweenUserVideos + 140);
        // UpdatePlayerVideoPostions();
    }

    private void UpdatePlayerVideoPostions()
    {
        for (int i = 0; i < playerVideoList.Count; i++)
        {
            playerVideoList[i].GetComponent<RectTransform>().anchoredPosition = Vector2.down * 150 * i;
        }
    }

}
