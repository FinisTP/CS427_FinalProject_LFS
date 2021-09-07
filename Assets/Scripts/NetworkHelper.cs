using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;

public class NetworkHelper
{
    public static void SetRoomProperty(object k, object v)
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(k))
        {
            PhotonNetwork.CurrentRoom.CustomProperties.Add(k, v);
        }
        else PhotonNetwork.CurrentRoom.CustomProperties[k] = v;
    }

    public static void SetPlayerProperty(Player player, object k, object v)
    {
        if (!player.CustomProperties.ContainsKey(k))
        {
            player.CustomProperties.Add(k, v);
        }
        else player.CustomProperties[k] = v;
    }

}
