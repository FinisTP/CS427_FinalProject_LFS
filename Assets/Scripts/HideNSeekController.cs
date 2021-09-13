using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HideNSeekController : MonoBehaviour
{
    public TMP_Text CounterNumber;
    public TMP_Text Command;
    public Image EnergyBar;
    public Interactable[] generators;
    // Update is called once per frame
    void Update()
    {
        ThirdPersonMovement localTPM = ThirdPersonMovement.LocalPlayerInstance;
        switch (GameplayManager.instance.matchPhase)
        {
            case MatchPhase.HIDE:
                // photonView.RPC("UpdateCounter", RpcTarget.All,);
                CounterNumber.text = ((int)(GameplayManager.instance.hideTime - GameplayManager.instance.currentTime)).ToString();
                if (localTPM.currentRole == global::Role.SEEKER)
                {
                    Command.text = "Please patiently wait until hiding time runs out.";
                }
                else if (localTPM.currentRole == global::Role.HIDER)
                {
                    Command.text = "Find a hiding place before time runs out!";
                }
                break;
            case MatchPhase.SEEK:
                // photonView.RPC("UpdateCounter", RpcTarget.All, ((int)(manager.SeekTime - manager.currentTime)).ToString());
                CounterNumber.text = ((int)(GameplayManager.instance.seekTime - GameplayManager.instance.currentTime)).ToString();
                if (localTPM.currentRole == global::Role.SEEKER)
                {
                    Command.text = "Find the prey and shoot to kill them before time runs out!";
                }
                else if (localTPM.currentRole == global::Role.HIDER)
                {
                    Command.text = "Stay away from the hunter's sight!";
                }
                break;
            default:
                CounterNumber.text = "";
                Command.text = "";
                break;
        }
    }
}
