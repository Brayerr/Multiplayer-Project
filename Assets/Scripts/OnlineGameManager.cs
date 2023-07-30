using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;


public class OnlineGameManager : MonoBehaviourPunCallbacks
{
    public static OnlineGameManager Instance { get; private set; }


    private const string SET_PLAYER_CONTROLLER = nameof(SetPlayerController);
    private const string SPAWN_PLAYER = nameof(SpawnPlayer);
    private const string ASK_FOR_SPAWN = nameof(AskForSpawnPoint);

    private List<PlayerController> playerControllers = new List<PlayerController>();

    private PlayerController localPlayerController;
    private PlayerCam localPlayerCam;

    [SerializeField] SpawnPoint[] spawnPoints;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            //ask master to initialize player
            //after initialize, either spawn player or give old player back
            //if spawn player then choose spawn location
        }
    }



    #region RPC

    [PunRPC]
    void InitializePlayer(PhotonMessageInfo info)
    {
        Player newPlayer = info.Sender;
        if (!PhotonNetwork.IsMasterClient)
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
        photonView.RPC(ASK_FOR_SPAWN, RpcTarget.MasterClient);
    }

    [PunRPC]
    public void AskForSpawnPoint(PhotonMessageInfo photonMessageInfo)
    {
        int rnd = Random.Range(0, spawnPoints.Length);
        while (spawnPoints[rnd].isTaken == true)
        {
            rnd = (rnd + 1) % spawnPoints.Length;
        }
        spawnPoints[rnd].isTaken = true;
        photonView.RPC(SPAWN_PLAYER, photonMessageInfo.Sender, rnd);
    }

    [PunRPC]
    public void SpawnPlayer(int i)
    {
        localPlayerController = PhotonNetwork.Instantiate($"PlayerPrefabs/playerPrefab {PhotonNetwork.LocalPlayer.CustomProperties[Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY]}",
            spawnPoints[i].transform.position,
            transform.rotation).GetComponent<PlayerController>();

        localPlayerCam.SetOrientation(localPlayerController.orientation);
    }

    [PunRPC]
    public void RespawnPlayer()
    {
        localPlayerController.transform.position = spawnPoints[localPlayerController.spawnPoint].transform.position;
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
        print("set player cam");
        localPlayerCam = newPlayerCam;
    }

    public void AddPlayerController(PlayerController playerController)
    {
        playerControllers.Add(playerController);
    }

    void InitializeSpawnPoints()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPoints[i].AssignID(i);
        }
    }
}
