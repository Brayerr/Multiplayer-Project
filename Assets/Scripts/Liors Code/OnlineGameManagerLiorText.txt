using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class OnlineGameManager : MonoBehaviourPunCallbacks
{
    public static OnlineGameManager Instance { get; private set; }
    
    public const string NETWORK_PLAYER_PREFAB_NAME = "NetworkPlayerObject";
 
    private const string GAME_STARTED_RPC = nameof(GameStarted);
    private const string COUNTDOWN_STARTED_RPC = nameof(CountdownStarted);
    private const string ASK_FOR_RANDOM_SPAWN_POINT_RPC = nameof(PlayerInitialProcess);
    private const string SPAWN_PLAYER_CLIENT_RPC = nameof(SpawnPlayer);

    private int someVariable;
    public bool hasGameStarted = false;

    [SerializeField] private TextMeshProUGUI gameModeText;
    [SerializeField] private TextMeshProUGUI playersScoreText;
    [SerializeField] private TextMeshProUGUI currentSpawnPointsInfoText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Button startGameButtonUI;
    [SerializeField] private SpawnPoint[] spawnPoints;

    private List<PlayerController> playerControllers = new List<PlayerController>();
    private PlayerController localPlayerController;

    private bool isCountingForStartGame;
    private float timeLeftForStartGame = 0;
    
    public void StartGameCountdown()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int countdownRandomTime = Random.Range(3, 8);
            photonView.RPC(COUNTDOWN_STARTED_RPC,
                RpcTarget.AllViaServer, countdownRandomTime );
            startGameButtonUI.interactable = false;
        }
    }
    
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        Debug.Log("Masterclient has been switched!" + Environment.NewLine
        + "Masterclient is now actor number " + newMasterClient.ActorNumber);
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if(otherPlayer.IsInactive)
            Debug.Log("Player " + otherPlayer.NickName + " Left the room, he has 10 seconds to come back.");
        else
        {
            Debug.Log("Player " + otherPlayer.NickName +" Will not comeback");
        }
    }

    public void SetPlayerController(PlayerController newLocalController)
    {
        localPlayerController = newLocalController;
    }

    public void AddPlayerController(PlayerController playerController)
    {
        playerControllers.Add(playerController);
    }

    #region RPCS

    [PunRPC]
    void CountdownStarted(int countdownTime)
    {
        isCountingForStartGame = true;
        timeLeftForStartGame = countdownTime;
        countdownText.gameObject.SetActive(true);
    }

    [PunRPC]
    void GameStarted(PhotonMessageInfo info)
    {
        hasGameStarted = true;
        localPlayerController.canControl = true;
        isCountingForStartGame = false;
        Debug.Log("Game Started!!! WHOW");
    }

    [PunRPC]
    void PlayerInitialProcess(PhotonMessageInfo messageInfo)
    {
        Player newPlayer = messageInfo.Sender;
        if (PhotonNetwork.IsMasterClient)
        {
            base.OnPlayerEnteredRoom(newPlayer);

            bool isReturningPlayer = false;
            Player oldPlayer = null;
            
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (!player.CustomProperties.ContainsKey(Constants.PLAYER_INITIALIZED) ||
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

            Debug.Log("A player has joined, and he is " + isReturningPlayer + " for returning");
            if (isReturningPlayer)
            {
                foreach (PhotonView photonView in PhotonNetwork.PhotonViewCollection)
                {
                    if (photonView.Owner.ActorNumber == oldPlayer.ActorNumber)
                    {
                        photonView.TransferOwnership(newPlayer);
                    }
                }

                foreach (PlayerController playerController in playerControllers)
                {
                    if (playerController.photonView.Owner.ActorNumber == oldPlayer.ActorNumber)
                    {
                        Debug.Log("Old position is " + playerController.transform.position);
                    }
                }
                photonView.RPC("SetPlayerController", newPlayer);
            }
            else
            {
                newPlayer.SetCustomProperties(new Hashtable{ { Constants.PLAYER_INITIALIZED, true } });
                
                List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();
                foreach (SpawnPoint spawnPoint in spawnPoints)
                {
                    if(!spawnPoint.taken)
                        availableSpawnPoints.Add(spawnPoint);
                }

                SpawnPoint chosenSpawnPoint =
                    availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
                chosenSpawnPoint.taken = true;
        
                bool[] takenSpawnPoints = new bool[spawnPoints.Length];
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    takenSpawnPoints[i] = spawnPoints[i].taken;
                }
                photonView.RPC(SPAWN_PLAYER_CLIENT_RPC,
                    messageInfo.Sender, chosenSpawnPoint.ID,
                    takenSpawnPoints);
            }
        }
    }

    [PunRPC]
    void SpawnPlayer(int spawnPointID, bool[] takenSpawnPoints)
    {
        SpawnPoint spawnPoint = GetSpawnPointByID(spawnPointID);
        PhotonNetwork.Instantiate(NETWORK_PLAYER_PREFAB_NAME, 
                    spawnPoint.transform.position, 
                    spawnPoint.transform.rotation)
                .GetComponent<PlayerController>();
        
        for (int i = 0; i < takenSpawnPoints.Length; i++)
        {
            spawnPoints[i].taken = takenSpawnPoints[i];
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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // localPlayerController =
            //     PhotonNetwork.Instantiate(NETWORK_PLAYER_PREFAB_NAME, 
            //             spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].position, 
            //             spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].rotation)
            //         .GetComponent<PlayerController>();
            photonView.RPC(ASK_FOR_RANDOM_SPAWN_POINT_RPC, RpcTarget.MasterClient);
            if (PhotonNetwork.IsMasterClient)
            {
                Hashtable hashtable = new Hashtable();
                hashtable.Add(Constants.MATCH_STARTED, false);
                PhotonNetwork.CurrentRoom.SetCustomProperties(
                    hashtable);
                startGameButtonUI.interactable = true;
            }

            gameModeText.text = PhotonNetwork.CurrentRoom.CustomProperties[Constants.GAME_MODE].ToString();
            foreach (KeyValuePair<int, Player>
                         player in PhotonNetwork.CurrentRoom.Players)
            {
                if (player.Value.CustomProperties
                    .ContainsKey(Constants.PLAYER_STRENGTH_SCORE_PROPERTY_KEY))
                {
                    playersScoreText.text +=
                        player.Value.CustomProperties[Constants.PLAYER_STRENGTH_SCORE_PROPERTY_KEY]
                            += Environment.NewLine;
                }
            }
        }
    }

    private void Update()
    {
        if (isCountingForStartGame)
        {
            timeLeftForStartGame -= Time.deltaTime;
            countdownText.text = Mathf.Ceil(timeLeftForStartGame).ToString();
            if (timeLeftForStartGame <= 0)
            {
                isCountingForStartGame = false;
                if (PhotonNetwork.IsMasterClient)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable.Add(Constants.MATCH_STARTED, true);
                    PhotonNetwork.CurrentRoom.SetCustomProperties(
                        hashtable);
                    photonView.RPC(GAME_STARTED_RPC, RpcTarget.AllViaServer);
                }
            }
        }

        string spawnPointsText = string.Empty;

        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            spawnPointsText += spawnPoint.ID + " " + spawnPoint.taken + Environment.NewLine;
        }

        currentSpawnPointsInfoText.text = spawnPointsText;
    }

    private void OnValidate()
    {
        int currentID = 0;
        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            spawnPoint.ID = currentID++;
        }
    }

    private SpawnPoint GetSpawnPointByID(int targetID)
    {
        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.ID == targetID)
                return spawnPoint;
        }

        return null;
    }


}
