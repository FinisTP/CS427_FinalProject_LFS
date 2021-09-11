using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    public GameObject GameManagerPrefab;
    private void Awake()
    {
        PhotonNetwork.Instantiate("GameManager", Vector3.zero, Quaternion.identity);
    }
}
