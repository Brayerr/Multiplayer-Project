using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public class OnlineGameManager : MonoBehaviourPunCallbacks
{
    public static OnlineGameManager Instance { get; private set; }


    private const string SET_PLAYER_CONTROLLER = nameof(SetPlayerController);

    [SerializeField] private List<int> activePlayers = new List<int>();
    private List<PlayerController> playerControllers = new List<PlayerController>();

    private PlayerController localPlayerController;
    private PlayerCam localPlayerCam;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

        PlayerController.PlayerDied += AskToRemovePlayer;

        GameObject go;
        PlayerController pc;

        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties[Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY] == null)
            {
                Debug.Log($"{PhotonNetwork.LocalPlayer.NickName} joined the game mid session, giving random skin"); // Player entered the room without a skin (probably mid-game)
                int characterskindID = Random.Range(0, 6);

                go = PhotonNetwork.Instantiate($"PlayerPrefabs/playerPrefab{characterskindID}", new Vector3(0, 3, -8), transform.rotation);

                localPlayerCam.SetOrientation(localPlayerController.orientation);
                localPlayerController.lookAt.UpdatePlayerName(localPlayerController.photonView.Owner.NickName);
                photonView.RPC("AddPlayer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
                return;
            }

            go = PhotonNetwork.Instantiate($"PlayerPrefabs/playerPrefab{PhotonNetwork.LocalPlayer.CustomProperties[Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY]}", new Vector3(0, 3, -8), transform.rotation);


            localPlayerCam.SetOrientation(localPlayerController.orientation);
            localPlayerController.lookAt.UpdatePlayerName(localPlayerController.photonView.Owner.NickName);
            photonView.RPC("AddPlayer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    private void OnDestroy()
    {

        PlayerController.PlayerDied -= AskToRemovePlayer;
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

    public void AskToRemovePlayer()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RemovePlayer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
            print("activating remove RPC");
        }
    }

    [PunRPC]
    public void AddPlayer(int actorNum , PhotonMessageInfo info)
    {
        Debug.Log($"{nameof(AddPlayer)}, msgInfonum {info.Sender.ActorNumber}, actornum {actorNum}");
        if (PhotonNetwork.IsMasterClient) activePlayers.Add(info.Sender.ActorNumber);
        print($"{info.Sender.ActorNumber} joined the list ");
    }

    [PunRPC]
    public void RemovePlayer(int actorNum, PhotonMessageInfo info)
    {
        Debug.Log($"{nameof(RemovePlayer)}, msgInfonum {info.Sender.ActorNumber}, actornum {actorNum}");
        if (PhotonNetwork.IsMasterClient)
        {
            activePlayers.Remove(info.Sender.ActorNumber);
            print($"removed player {info.Sender.ActorNumber}");
            if (activePlayers.Count <= 1) EndGameLoop();
        }
    }

    public void EndGameLoop()
    {
        if (PhotonNetwork.IsMasterClient) photonView.RPC(Constants.END_GAME_RPC, RpcTarget.AllViaServer);
    }


    [PunRPC]
    public void EndGameRPC()
    {
        print("restarting");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Exit the room
        PhotonNetwork.RemovePlayerCustomProperties(Constants.ProprtiesToClearOnLeaveRoom);
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }
}
