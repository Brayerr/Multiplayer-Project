using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
using System.Linq;
using Photon.Pun.UtilityScripts;

public class OnlineGameManager : MonoBehaviourPunCallbacks
{
    public static OnlineGameManager Instance { get; private set; }


    private const string SET_PLAYER_CONTROLLER = nameof(SetPlayerController);
    private const string INITIALIZE_PLAYER = nameof(InitializePlayer);
    private const string SPAWN_PLAYER = nameof(SpawnPlayer);
    private const string UPDATE_SPAWN_POINTS = nameof(UpdateSpawnPoints);

    [SerializeField] private List<int> activePlayers = new List<int>();
    private List<PlayerController> playerControllers = new List<PlayerController>();

    private PlayerController localPlayerController;
    private PlayerCam localPlayerCam;

    [SerializeField] SpawnPoint[] spawnPoints;
    int playersInitialized = 0;

    public event System.Action<int> PlayerInitialized;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {


        PlayerController pc;

        if (PhotonNetwork.IsConnectedAndReady)
        {
            photonView.RPC(INITIALIZE_PLAYER, RpcTarget.MasterClient);


            //go = PhotonNetwork.Instantiate($"PlayerPrefabs/playerPrefab{PhotonNetwork.LocalPlayer.CustomProperties[Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY]}", new Vector3(0, 3, -8), transform.rotation);


            //localPlayerCam.SetOrientation(localPlayerController.orientation);
            //localPlayerController.lookAt.UpdatePlayerName(localPlayerController.photonView.Owner.NickName);
            //photonView.RPC("AddPlayer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        else
        {
            print("lol");
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

        //base.OnPlayerEnteredRoom(newPlayer);

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
            print("old player actor num: " + oldPlayer.ActorNumber);
            print("new player actor num: " + newPlayer.ActorNumber);

            foreach (PhotonView view in PhotonNetwork.PhotonViewCollection)
            {
                if (view.OwnerActorNr == oldPlayer.ActorNumber)
                {
                    print($"transfered view: {view} with AcNum: {view.OwnerActorNr} to new player");
                    view.TransferOwnership(newPlayer);
                    //transfer all properties?
                }
            }
            newPlayer.SetCustomProperties(oldPlayer.CustomProperties);

            photonView.RPC(SET_PLAYER_CONTROLLER, newPlayer, oldPlayer.ActorNumber);
        }
        else
        {
            newPlayer.SetCustomProperties(new Hashtable { { Constants.PLAYER_INITIALIZED_KEY, true } });
            print(newPlayer.CustomProperties.ToString());

            int rnd = Random.Range(0, spawnPoints.Length);
            while (spawnPoints[rnd].isTaken == true)
            {
                rnd = (rnd + 1) % spawnPoints.Length;
            }
            spawnPoints[rnd].isTaken = true;
            photonView.RPC(SPAWN_PLAYER, info.Sender, rnd);
        }
        playersInitialized++;
        if (playersInitialized >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC("TestSpawnPoints", RpcTarget.All);
            //ShareSpawnPoints();
            //photonView.RPC("TestSpawnPoints", RpcTarget.All);
        }

    }

    //[PunRPC]
    //public void AskForSpawnPoint(PhotonMessageInfo photonMessageInfo)
    //{
    //    int rnd = Random.Range(0, spawnPoints.Length);
    //    while (spawnPoints[rnd].isTaken == true)
    //    {
    //        rnd = (rnd + 1) % spawnPoints.Length;
    //    }
    //    spawnPoints[rnd].isTaken = true;
    //    photonView.RPC(SPAWN_PLAYER, photonMessageInfo.Sender, rnd);
    //}

    [PunRPC]
    public void SpawnPlayer(int i)
    {
        localPlayerController = PhotonNetwork.Instantiate($"PlayerPrefabs/playerPrefab{PhotonNetwork.LocalPlayer.CustomProperties[Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY]}",
            spawnPoints[i].transform.position,
            transform.rotation).GetComponent<PlayerController>();

        localPlayerCam.SetOrientation(localPlayerController.orientation);
        photonView.RPC("AddPlayer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    public void RespawnPlayer()
    {
        localPlayerController.transform.position = spawnPoints[localPlayerController.spawnPoint].transform.position;
    }

    [PunRPC]
    void SetPlayerController(int oldActorNum)
    {
        PlayerInitialized.Invoke(oldActorNum);
        //foreach (PlayerController playerController in playerControllers)
        //{
        //    if (playerController.photonView.OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber)
        //    {
        //        print("set controller of returning player");
        //        localPlayerController = playerController;
        //        localPlayerCam.SetOrientation(localPlayerController.orientation);
        //        break;
        //    }
        //}
    }

    [PunRPC]
    public void AddPlayer(int actorNum, PhotonMessageInfo info)
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

    [PunRPC]
    public void EndGameRPC()
    {
        print("restarting");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Exit the room
        PhotonNetwork.RemovePlayerCustomProperties(Constants.ProprtiesToClearOnLeaveRoom);
        PhotonNetwork.LeaveRoom();
        StartCoroutine(DisplayScore());
        SceneManager.LoadScene(0);
    }

    [PunRPC]
    public void UpdateSpawnPoints(string data)
    {
        SpawnPoint[] spawnData = JsonUtility.FromJson<SpawnPoint[]>(data);
        spawnPoints = spawnData;
        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            print(spawnPoint.isTaken);
        }
    }

    [PunRPC]
    public void TestSpawnPoints()
    {
        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            print(spawnPoint.isTaken);
        }
    }

    #endregion

    public void SetPlayerControllerLocally(PlayerController newLocalController)
    {
        localPlayerController = newLocalController;
    }

    public void SetPlayerCam(PlayerCam newPlayerCam)
    {
        print("set player cam");
        localPlayerCam = newPlayerCam;
    }

    public PlayerController GetLocalPlayerController()
    {
        return localPlayerController;
    }

    public void AskToRemovePlayer()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RemovePlayer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
            print("activating remove RPC");
        }
    }

    public void EndGameLoop()
    {
        if (PhotonNetwork.IsMasterClient) photonView.RPC(Constants.END_GAME_RPC, RpcTarget.AllViaServer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        print($"{otherPlayer.NickName} left room, isInactive: {otherPlayer.IsInactive}");

        if (otherPlayer.IsInactive)
        {
            //player can still return
            print("if is inactive, u can see this");
        }
        else
        {
            //player ded
            print("if is not inactive (meaning completely gone), u can see this");
        }
    }

    void InitializeSpawnPoints()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPoints[i].AssignID(i);
        }
    }

    void ShareSpawnPoints()
    {
        string spawnData = JsonUtility.ToJson(spawnPoints);
        photonView.RPC(UPDATE_SPAWN_POINTS, RpcTarget.All, spawnData);
    }

    IEnumerator DisplayScore()
    {
        OnlineScoreManager.Instance.scoreboard.SetActive(true);
        yield return new WaitForSeconds(5);
        OnlineScoreManager.Instance.scoreboard.SetActive(false);

    }
}