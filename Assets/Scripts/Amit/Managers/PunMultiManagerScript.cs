using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon.StructWrapping;
using Unity.VisualScripting;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class PunMultiManagerScript : MonoBehaviourPunCallbacks
{
    [Header("Welcome Panel")]
    [SerializeField] private GameObject welcomePanel;
    [Space]
    [SerializeField] private TMP_InputField playerNickname;
    [SerializeField] private TMP_Text welcomePrompt;
    [SerializeField] private Button selectNickName;
    // Second Screen
    [SerializeField] private TMP_Text welcomePrompt2;
    [SerializeField] private Button joinServer;
    [Space]

    [Header("Lobby Panel")]
    [SerializeField] private GameObject lobbyPanel;
    [Space]

    [Header("ServerSearch")]
    [SerializeField] private Button findServerButton;
    [SerializeField] private Slider findServerMaxPlayerSlider;

    [Header("Room Info Panel")]
    [SerializeField] TMP_Text masterStatus;
    [SerializeField] TMP_Text lobbyStatus;
    [Space]
    [SerializeField] private TMP_Text chooseRoomPrompt;
    [SerializeField] private TMP_InputField chooseRoomInputField;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private TMP_Text maxPlayerText;
    [SerializeField] private Slider maxPlayerSlider;
    [SerializeField] private TMP_Text timeToDisconnectText;
    [SerializeField] private Slider timeToDisconnectSlider;
    [Space]
    [SerializeField] private TMP_Text selctedRoomName;
    [SerializeField] private TMP_Text selectedRoomListPrompt;
    [SerializeField] private TMP_Text selctedRoomPlayerList;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button exitButton;
    [Space]
    [Header("Room Scroll View")]
    [SerializeField] GameObject scrollViewContent;
    [SerializeField] GameObject scrollbarVertical;
    [SerializeField] GameObject roomUIPrefab;
    [SerializeField] TMP_Text defualtScrollPrompt;
    [Space]

    [Header("Room Panel")]
    [SerializeField] private GameObject roomPanel;
    [Space]

    [SerializeField] private TMP_Text roomPanelRoomName;
    [SerializeField] private TMP_Text roomPanelRoomNumberOfPlayer;
    [SerializeField] private Button startOrJoinGameButton;
    [SerializeField] private TMP_Text startOrJoinGameText;
    [Space]

    [SerializeField] private GameObject playerUIPrefab;
    [SerializeField] private GameObject playerUIContext;

    //const string PLAYER_PREFAB_NAME = "PlayerCapsule";
    //private const string SCORE_KEY_NAME = "Score";
    //public const string LOAD_GAME_NAME = "JoinOrStartGame";
    //public const string PING_HASHTABLE_NAME = "ping";
    public List<RoomInfo> cachedRoomInfos = new();
    List<string> RoomNames = new();

    private bool isMasterClient => PhotonNetwork.IsMasterClient;

    private string currentSelctedRoom;

    List<GameObject> UIRoomList => new();


    #region Event Methods
    public void NickNameCreated()
    {
        if (playerNickname.text.Length >= 3)
        {
            PhotonNetwork.NickName = playerNickname.text;

            welcomePrompt.gameObject.SetActive(false);
            selectNickName.gameObject.SetActive(false);
            playerNickname.gameObject.SetActive(false);



            welcomePrompt2.text = $"Welcome <color=green>{PhotonNetwork.NickName}</color> press the button to join the server";
            welcomePrompt2.gameObject.SetActive(true);
            joinServer.gameObject.SetActive(true);

            if (playerNickname.text == "BlackBetty")
            {
                welcomePrompt2.text = $"Welcome <color=black>{PhotonNetwork.NickName}</color> press the button to join the server";
                AudioSource audio = gameObject.AddComponent<AudioSource>();
                AudioClip clip = Resources.Load<AudioClip>("SFX/Ram Jam  Black Betty");
                audio.clip = clip;
                audio.loop = false;
                audio.volume = 1;
                audio.Play();
            }
        }
        else
        {
            welcomePrompt.color = Color.red;
        }
    }

    public void PhotonPunLogin()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        if (lobbyPanel != null)
        {
            welcomePanel.gameObject.SetActive(false);
            lobbyPanel.gameObject.SetActive(true);
            createRoomButton.interactable = false;
        }
    }

    public void CreateRoom()
    {
        byte roomMax = (byte)int.Parse(maxPlayerSlider.value.ToString());
        int emptyRoomTtl = int.Parse(timeToDisconnectSlider.value.ToString());
        PhotonNetwork.CreateRoom(chooseRoomInputField.text, new RoomOptions() { MaxPlayers = roomMax, PlayerTtl = 30000,  EmptyRoomTtl = emptyRoomTtl, CleanupCacheOnLeave = false }, null);
        createRoomButton.interactable = false;
    }

    #endregion

    #region Unity Methods
    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Player Is connected and ready");
            welcomePrompt.gameObject.SetActive(false);
            selectNickName.gameObject.SetActive(false);
            playerNickname.gameObject.SetActive(false);
            welcomePanel.gameObject.SetActive(false);
            PhotonPunLogin();
        }

        if (welcomePanel != null && lobbyPanel != null)
        {
            welcomePanel.gameObject.SetActive(true);
            lobbyPanel.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("One of the panels was not found");
        }
    }

    private void FixedUpdate()
    {
        if (PhotonNetwork.InLobby && createRoomButton.gameObject.activeInHierarchy)
        {
            maxPlayerText.text = maxPlayerSlider.value.ToString();
            timeToDisconnectText.text = timeToDisconnectSlider.value.ToString();

            if (chooseRoomInputField.text.Length > 3) createRoomButton.interactable = true;
            else createRoomButton.interactable = false;
        }
    }

    #endregion

    #region PhotonNetwork Overrides
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        if (masterStatus != null)
        {
            SetUsersUniqueID();
            masterStatus.color = Color.green;
            masterStatus.text = "Connected to Master";
            PhotonNetwork.EnableCloseConnection = true;
            PhotonNetwork.JoinLobby();
            PlayerCustomPropPing();
        }
    }

    

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        if (masterStatus != null)
        {
            masterStatus.color = Color.red;
            masterStatus.text = "Disconnected from Master";
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        if (lobbyStatus != null)
        {
            lobbyStatus.color = Color.green;
            lobbyStatus.text = "Connected to Lobby";
        }
    }

    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        if (lobbyStatus != null)
        {
            lobbyStatus.color = Color.red;
            lobbyStatus.text = "Disconnted from Lobby";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        UIRoomClear();

        Debug.Log($"OnRoomListUpdate Override Called, there are {cachedRoomInfos.Count} rooms in the list");

        foreach (RoomInfo roomInfo in roomList)
        {
            // Add room info if no room has the name
            if (!RoomNames.Contains(roomInfo.Name))
            {
                RoomNames.Add(roomInfo.Name);
                cachedRoomInfos.Add(roomInfo);
                continue;
            }

            // Remove and Add room if the name exists but something else has been changed
            if (RoomNames.Contains(roomInfo.Name))
            {
                RoomInfo roomToRemove = null;
                RoomInfo roomToAdd = null;

                foreach(var room in cachedRoomInfos)
                {
                    if (room.Name == roomInfo.Name)
                    {
                        roomToRemove = room;

                        if (!roomInfo.RemovedFromList)
                        {
                            roomToAdd = roomInfo;
                            continue;
                        }

                        RoomNames.Remove(roomInfo.Name);
                    }
                }
                if (roomToRemove != null) { cachedRoomInfos.Remove(roomToRemove); }
                if (roomToAdd != null) { cachedRoomInfos.Add(roomToAdd);}
            }

        }

        if (cachedRoomInfos.Count > 0)
        {
            defualtScrollPrompt.gameObject.SetActive(false);
            Debug.Log("Rooms Created");
            foreach (var roominfo in cachedRoomInfos)
            {
                Debug.Log(roominfo.Name);
                if (roominfo.PlayerCount > 0)
                {
                    UIRoomInstantion(roominfo);
                }
            }
        }

        else
        {
            defualtScrollPrompt.gameObject.SetActive(true);
            Debug.Log("Room List Updated But No Rooms Was Found");
        }
    }

    public override void OnCreatedRoom()
    {
        if (isMasterClient) { PhotonNetwork.EnableCloseConnection = true; }
        Debug.Log(PhotonNetwork.CurrentRoom.EmptyRoomTtl);
        base.OnCreatedRoom();
        joinRoomButton.interactable = false;
        selctedRoomName.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            selctedRoomPlayerList.text = $"{player.NickName}\n";
        }
        CreateRoomSwitch(false);
        LobbyToRoomSwitch(true);
        RoomHandler();
    }



    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        createRoomButton.interactable = true;
        chooseRoomPrompt.text = "Failed to Create Room";
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        joinRoomButton.interactable = false;
        CreateRoomSwitch(false);
        LobbyToRoomSwitch(true);
        RoomHandler();
        Debug.Log("JoinedRoom");

    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log("Failed To Join");
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        LobbyToRoomSwitch(false);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // refresh room ui! like on room list update

        base.OnPlayerEnteredRoom(newPlayer);
        RoomHandler();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        RoomHandler();
    }

    #endregion

    #region Debug

    [ContextMenu("DebugServerDisconnect")]
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    [ContextMenu("DebugNumberOfRooms")]
    public void NumberOfRooms() => Debug.Log(PhotonNetwork.CountOfRooms);

    [ContextMenu("DebugLobbyDisconnect")]
    public void LobbyDisconnect() => PhotonNetwork.LeaveLobby();

    [ContextMenu("DebugLobbyConnect")]
    public void LobbyConnect() => PhotonNetwork.JoinLobby();
    #endregion

    #region Handlers
    void UIRoomClear()
    {
        var childcount = scrollViewContent.transform.childCount;
        for (int i = 0; i < childcount; i++)
        {
            if (scrollViewContent.transform.GetChild(i).tag == "Destructable")
            {
                Destroy(scrollViewContent.transform.GetChild(i).gameObject);
            }
        }
    }

    #endregion

    #region Unsorted

    [ContextMenu("LoadLevel")]
    public void StartAndLoadLevel()
    {
        PhotonNetwork.LoadLevel(1);
    }


    public void UpdateScoreProprety()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("You tried to set protpreies while disconnected");
            return;
        }

        // Get the playerscore
        ExitGames.Client.Photon.Hashtable scoreHashtable = new();
        PhotonNetwork.LocalPlayer.SetCustomProperties(scoreHashtable);
    }


    public void RoomPicked(RoomInfo roominfo)
    {
        if (roominfo != null)
        {
            Debug.Log("HasInfo");
            selctedRoomName.text = "Room: " + roominfo.Name;
            selctedRoomPlayerList.text = $"{roominfo.PlayerCount}/{roominfo.MaxPlayers}";
            CreateRoomSwitch(false);
            joinRoomButton.interactable = true;
            currentSelctedRoom = roominfo.Name;
            joinRoomButton.onClick.AddListener(JoinRoom);
        }
    }


    void JoinRoom()
    {
        PhotonNetwork.JoinRoom(currentSelctedRoom, null);
    }


    void UIRoomInstantion(RoomInfo roominfo)
    {
        var tmpTempList = roomUIPrefab.GetComponentsInChildren<TMP_Text>();
        tmpTempList[0].text = roominfo.Name;
        tmpTempList[1].text = $"{roominfo.PlayerCount}/{roominfo.MaxPlayers}";
        GameObject go;
        UIRoomList.Add(go = Instantiate<GameObject>(roomUIPrefab, scrollViewContent.transform));
        go.GetComponentInChildren<MyRoomInfo>().SetRoomInfo(roominfo);
    }

    public void SelectedRoomsCount(int index) => currentSelctedRoom += index;

    public void RoomHandler()
    {
        // destroy player list on every update
        var childcount = playerUIContext.transform.childCount;
        for (int i = 0; i < childcount; i++)
        {
            if (playerUIContext.transform.GetChild(i).tag == "Destructable")
            {
                Debug.Log("PlayerListCleard");
                Destroy(playerUIContext.transform.GetChild(i).gameObject);
            }
        }


        if (roomPanel.activeInHierarchy)
        {
            roomPanelRoomName.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
            roomPanelRoomNumberOfPlayer.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";

            if (isMasterClient)
            {
                startOrJoinGameButton.interactable = false;
                startOrJoinGameText.text = "Start Game";
            }
            else
            {
                startOrJoinGameButton.interactable = false;
                startOrJoinGameText.text = "Join Game?";
            }
        }

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            Debug.Log($"{player.NickName}");
            var playerUI = Instantiate(playerUIPrefab, playerUIContext.transform);
            PlayerTabIdentity actor = playerUI.GetComponent<PlayerTabIdentity>();
            actor.SetView(photonView);
            actor.SetPlayer(player);
            actor.KickButtonVisable(isMasterClient);
            TMP_Text[] playerListUI = playerUI.GetComponentsInChildren<TMP_Text>();
            playerListUI[0].text = player.NickName;
            playerListUI[1].text = ExtractPlayerPing(player);
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount > 0)
        {
            startOrJoinGameButton.interactable = true;
        }
    }

    public void LobbyToRoomSwitch(bool switchToRoom)
    {
        if (lobbyPanel != null && roomPanel != null)
            if (switchToRoom)
            {
                lobbyPanel.SetActive(false);
                roomPanel.SetActive(true);
            }
            else
            {
                lobbyPanel.SetActive(true);
                roomPanel.SetActive(false);
            }
    }

    public void CreateRoomSwitch(bool createRoom)
    {
        currentSelctedRoom = null;
        Debug.Log($"Room Switch is called with {createRoom} boolean");
        if (createRoom)
        {
            chooseRoomInputField.gameObject.SetActive(true);
            chooseRoomPrompt.gameObject.SetActive(true);
            createRoomButton.gameObject.SetActive(true);
            maxPlayerSlider.gameObject.SetActive(true);
            timeToDisconnectSlider.gameObject.SetActive(true);
            //
            selctedRoomName.gameObject.SetActive(false);
            selctedRoomPlayerList.gameObject.SetActive(false);
            selectedRoomListPrompt.gameObject.SetActive(false);
            joinRoomButton.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(false);
        }
        else
        {
            chooseRoomInputField.gameObject.SetActive(false);
            chooseRoomPrompt.gameObject.SetActive(false);
            createRoomButton.gameObject.SetActive(false);
            maxPlayerSlider.gameObject.SetActive(false);
            timeToDisconnectSlider.gameObject.SetActive(false);
            //
            selctedRoomName.gameObject.SetActive(true);
            selctedRoomPlayerList.gameObject.SetActive(true);
            selectedRoomListPrompt.gameObject.SetActive(true);
            joinRoomButton.gameObject.SetActive(true);
            exitButton.gameObject.SetActive(true);
        }
    }

    [ContextMenu("Debug Exit Room")]
    public void ExitRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    public void StartGame()
    {
        if (isMasterClient)
        {
            photonView.RPC(Constants.LOAD_GAME_NAME, RpcTarget.AllBuffered);
        }
    }

    private void SetUsersUniqueID()
    {
        Hashtable hashtable = new Hashtable();
        hashtable.Add(Constants.USER_UNIQUE_ID, SystemInfo.deviceUniqueIdentifier);
        PhotonNetwork.SetPlayerCustomProperties(hashtable);
    }

    private static void PlayerCustomPropPing()
    {
        Hashtable pingHashtable = new();
        pingHashtable.Add(Constants.PING_HASHTABLE_NAME, PhotonNetwork.GetPing());
        PhotonNetwork.SetPlayerCustomProperties(pingHashtable);
    }

    public void SearchForServersButton()
    {
        findServerButton.interactable = false;

        if (!FindServersWithSameMaxPlayerCount((int)findServerMaxPlayerSlider.value))
        {
            // didnt find any available servers with the desired player count
            StartCoroutine(FindServerButtonInteractableDelay(2f));
            return;
        }
    }

    IEnumerator FindServerButtonInteractableDelay(float timeToWait)
    {
        TMP_Text text = findServerButton.GetComponentInChildren<TMP_Text>();
        text.text = "<color=red>No Server Found</color>";
        yield return new WaitForSeconds(timeToWait);
        text.text = "Search Servers";
        findServerButton.interactable = true;
    }

    bool FindServersWithSameMaxPlayerCount(int desiredCount)
    {
        foreach (var roominfo in cachedRoomInfos)
        {
            if (roominfo.MaxPlayers == desiredCount && roominfo.PlayerCount < roominfo.MaxPlayers)
            {
                // Add Join room logic
                PhotonNetwork.JoinRoom(roominfo.Name);
                return true;
            }
        }

        return false;
    }

    public static string ExtractPlayerPing(Player player)
    {
        int playerPing = (int)player.CustomProperties[Constants.PING_HASHTABLE_NAME];
        if (playerPing > -1)
        {
            return playerPing.ToString() + " ms";
        }
        else return "N/A";
    }

    [PunRPC]
    void JoinOrStartGame()
    {
        PhotonNetwork.LoadLevel(1);
    }

    [PunRPC]
    public void KickPlayer(Player player)
    {
        if (PhotonNetwork.IsMasterClient && player != PhotonNetwork.MasterClient)
        {
            if (PhotonNetwork.CloseConnection(player))
            {
                bool isPlayerKicked = true;
                foreach (var dePlayer in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    if (dePlayer == player)
                    {
                        isPlayerKicked = false;
                    }
                }
                Debug.Log(player.NickName + ", " + isPlayerKicked);
            }
        }
    }
}