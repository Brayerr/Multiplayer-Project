using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnlineGameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject PlayerPrefab;

    const string PLAYER_PREFAB_NAME = "PlayerCapsule";

    private const string GAME_STATRTED_RPC = nameof(GameStarted);

    Vector3 StartLoc = new Vector3(0,1,0);

    bool hasGameStarted = false;
    bool hasNewPlayer = false;

    PlayerController localPlayerController;


    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(GAME_STATRTED_RPC,RpcTarget.AllViaServer);
        }
        else
        {

        }
    }

    [PunRPC]
    public void GameStarted()
    {
        hasGameStarted = true;
        localPlayerController.canControl = true;
        Debug.Log("You can now control the character");
    }

    [PunRPC]
    public void PlayerJoinedSession()
    {
        localPlayerController.canControl = true;
        Debug.Log(PhotonNetwork.LocalPlayer.ActorNumber + "You can now control the character");
    }

    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Ready and connected");
            localPlayerController = PhotonNetwork.Instantiate(PLAYER_PREFAB_NAME, StartLoc, PlayerPrefab.transform.rotation).GetComponent<PlayerController>();
        }

        if(PhotonNetwork.IsMasterClient)
        {
            print("master actor id: " + PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        Debug.Log($"Master client has been switched the mc is: {newMasterClient.NickName}");
    }
}
