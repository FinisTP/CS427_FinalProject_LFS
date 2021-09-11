using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using Photon.Voice.PUN;
using UnityStandardAssets.CrossPlatformInput;

public class LocalMic : MonoBehaviour
{
    // public KeyCode PushButton = KeyCode.M;
    public Recorder VoiceRecorder;
    private PhotonView view;
    

    private void Start()
    {
        view = GetComponent<PhotonView>();
        VoiceRecorder.TransmitEnabled = false;
    }

    public bool ToggleMic()
    {
        if (view.IsMine)
        {
            VoiceRecorder.TransmitEnabled = !VoiceRecorder.TransmitEnabled;
        }
        return VoiceRecorder.TransmitEnabled;
    }
}
