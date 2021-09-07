using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MatchController : MonoBehaviourPunCallbacks
{
    public TMP_Text CounterNumber;
    public TMP_Text Command;
    public TMP_Text Role;
    public Image HealthBar;

    private GameplayManager manager;

    private void Start()
    {
        manager = GameplayManager.instance;
    }
    private void Update()
    {
        ThirdPersonMovement localTPM = ThirdPersonMovement.LocalPlayerInstance;
        Role.text = localTPM.CurrentRole.ToString();
        switch (manager.CurrentMatchPhase)
        {
            case MatchPhase.Hide:
                // photonView.RPC("UpdateCounter", RpcTarget.All,);
                CounterNumber.text = ((int)(manager.HideTime - manager.currentTime)).ToString();
                if (localTPM.CurrentRole == global::Role.Seeker)
                {
                    Command.text = "Please patiently wait until hiding time runs out.";
                } else if (localTPM.CurrentRole == global::Role.Hider)
                {
                    Command.text = "Find a hiding place before time runs out!";
                }
                break;
            case MatchPhase.Seek:
                // photonView.RPC("UpdateCounter", RpcTarget.All, ((int)(manager.SeekTime - manager.currentTime)).ToString());
                CounterNumber.text = ((int)(manager.HideTime - manager.currentTime)).ToString();
                if (localTPM.CurrentRole == global::Role.Seeker)
                {
                    Command.text = "Find the prey and shoot to kill them before time runs out!";
                }
                else if (localTPM.CurrentRole == global::Role.Hider)
                {
                    Command.text = "Stay away from the hunter's sight!";
                }
                break;
            default:
                break;
        }
    }

    [PunRPC]
    void UpdateCounter(string time)
    {
        
    }

}
