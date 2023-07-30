using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class OnlineGameManager : MonoBehaviourPunCallbacks
{
    public static OnlineGameManager Instance { get; private set; }


    private const string SET_PLAYER_CONTROLLER = nameof(SetPlayerController);

    private List<PlayerController> playerControllers = new List<PlayerController>();
    
    private PlayerController localPlayerController;
    private PlayerCam localPlayerCam;


    private void Start()
    {
        if(PhotonNetwork.IsConnectedAndReady)
        {
            GameObject go = PhotonNetwork.Instantiate($"PlayerPrefabs/playerPrefab {PhotonNetwork.LocalPlayer.CustomProperties[Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY]}", Vector3.zero, transform.rotation);

            localPlayerCam.SetOrientation(localPlayerController.orientation);
        }
    }

    #region RPC

    [PunRPC]
    void InitializePlayer(PhotonMessageInfo info)
    {
        Player newPlayer = info.Sender;
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        base.OnPlayerEnteredRoom(newPlayer);

        bool isReturningPlayer = false;
        Player oldPlayer = null;


        //if new player contains playerInitialized and sets isReturningPlayer bool
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.ContainsKey(Constants.PLAYER_INITIALIZED_KEY) ||
                    player.ActorNumber == newPlayer.ActorNumber || !player.IsInactive)
                continue;

            if (player.CustomProperties[Constants.USER_UNIQUE_ID]
                    .Equals(newPlayer.CustomProperties[Constants.USER_UNIQUE_ID]))
            {
                oldPlayer = player;
                isReturningPlayer = true;
                break;
            }
        }

        print($"player {newPlayer.NickName} has joined, isReturningPlayer: {isReturningPlayer}");
        
        if (isReturningPlayer)
        {
            foreach (PhotonView photonView in PhotonNetwork.PhotonViewCollection)
            {
                if (photonView.Owner.ActorNumber == oldPlayer.ActorNumber)
                {
                    photonView.TransferOwnership(newPlayer);
                }
            }

            photonView.RPC(SET_PLAYER_CONTROLLER, newPlayer);

        }
        else
        {
            newPlayer.SetCustomProperties(new Hashtable { { Constants.PLAYER_INITIALIZED_KEY, true } });
        }
    }

    [PunRPC]
    void SetPlayerController()
    {
        foreach (PlayerController playerController in playerControllers)
        {
            if (playerController.photonView.Controller.ActorNumber
                == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                localPlayerController = playerController;
                break;
            }
        }
    }

    #endregion

    public void SetPlayerController(PlayerController newLocalController)
    {
        localPlayerController = newLocalController;
    }

    public void SetPlayerCam(PlayerCam newPlayerCam)
    {
        localPlayerCam = newPlayerCam;
    }

    public void AddPlayerController(PlayerController playerController)
    {
        playerControllers.Add(playerController);
    }
}
