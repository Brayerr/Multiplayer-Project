using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class OnlineCharacterSelectionManager : MonoBehaviourPun
{
    private const string LOCK_IN_CHARACTER_RPC = nameof(LockInCharacter);
    private const string UPDATE_AVAILABLE_CHARACTERS_RPC = nameof(UpdateAvailableCharacters);
    private const string CHECK_IF_EVERYONE_READY = nameof(CheckIfEveryoneReady);

    [Header("Selection Buttons")]
    [SerializeField] Button selectButton;
    [SerializeField] Button NextCharacterButton;
    [SerializeField] Button PreviousCharacterButton;
    [SerializeField] Button readyButton;
    [SerializeField] Button startButton;
    [Space]

    [Header("Characters")]
    [SerializeField] GameObject[] characters;
    [SerializeField] Transform ViewSpawnLocation;
    [SerializeField] float spawnOffset = 20f;

    List<int> availableCharacterIDs = new List<int>();
    List<int> selectedCharacterIDs = new List<int>();

    int currentCharacterIndex = 0;

    private void Awake()
    {
        InitializeAvailableCharacterIDs();

        CheckIfButtonsInteractable();
    }

    private void InitializeAvailableCharacterIDs()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            availableCharacterIDs.Add(i);
        }
    }

    private void Start()
    {
        LoadCharactersInScene();
    }


    #region RPC

    [PunRPC]
    public void LockInCharacter(int characterID, PhotonMessageInfo photonMessageInfo)
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            print("Go away padawan, this is for masters only");
            return;
        }

        Player[] players = PhotonNetwork.PlayerList;
        //for each player in room
        foreach (var player in players)
        {
            //if player character id exists
            if (player.CustomProperties.ContainsKey(Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY))
            {
                //check if id = current character index
                if ((int)player.CustomProperties[Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY] == characterID)
                {
                    print("someone has this character already");
                    return;
                }
            }
        }

        print("player " + photonMessageInfo.Sender.ActorNumber + " locked in to character " + characterID);
        ExitGames.Client.Photon.Hashtable playerHashtable;
        playerHashtable = photonMessageInfo.Sender.CustomProperties;
        playerHashtable.Add(Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY, characterID);
        photonMessageInfo.Sender.SetCustomProperties(playerHashtable);

        photonView.RPC(UPDATE_AVAILABLE_CHARACTERS_RPC, RpcTarget.AllViaServer, characterID);
    }

    [PunRPC]
    public void UpdateAvailableCharacters(int characterID)
    {
        availableCharacterIDs.Remove(characterID);
        selectedCharacterIDs.Add(characterID);
    }



    [PunRPC]
    public void CheckIfEveryoneReady()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            print("not master client tried updating ready");
            return;
        }

        //get all players
        Player[] players = PhotonNetwork.PlayerList;

        //for each player in room
        foreach (var player in players)
        {
            //if player has key ready
            if (player.CustomProperties.ContainsKey(Constants.PLAYER_READY_PROPERTY_KEY))
            {
                //if ready == false
                if ((bool)player.CustomProperties[Constants.PLAYER_READY_PROPERTY_KEY] == false)
                {
                    print($"{player.NickName} isnt ready");
                    return;
                }
            }
            else
            {
                print($"{player.NickName} isnt ready");
                return;
            }
        }

        //set room property everyone ready
        //for future use if master disconnects/changes and everyone ready
        PhotonNetwork.CurrentRoom.CustomProperties.Add(Constants.ROOM_EVERYONE_READY_KEY, true);

        print("Everyone should be ready");
        EnableStartButton();
    }

    #endregion

    public void UpdatePlayerReady()
    {
        readyButton.interactable = false;

        PhotonNetwork.LocalPlayer.CustomProperties.Add(Constants.PLAYER_READY_PROPERTY_KEY, true);

        photonView.RPC(CHECK_IF_EVERYONE_READY, RpcTarget.MasterClient);
    }

    public void TryLockInCharacter()
    {
        DisableSelectionButtons();
        photonView.RPC(LOCK_IN_CHARACTER_RPC, RpcTarget.MasterClient, currentCharacterIndex);
        //check if id is in customproperties
        StartCoroutine(CheckIfPlayerCustomPropertyUpdated(Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY));
    }

    public void LoadCharactersInScene()
    {
        int counter = 0;
        foreach (GameObject character in characters)
        {
            Vector3 spawnPosition = new Vector3(ViewSpawnLocation.position.x + spawnOffset * counter, ViewSpawnLocation.position.y, ViewSpawnLocation.position.z);
            counter++;
            GameObject newCharacter = Instantiate(character, spawnPosition, Quaternion.identity);
        }
    }

    /// <summary>
    /// Coroutine. checks every second for 3 seconds if the player custom properties updated with the requested key
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckIfPlayerCustomPropertyUpdated(string CUSTOM_PROPERTY_KEY)
    {
        //every 1 seconds check if contains key
        for (int i = 0; i < 3; i++)
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY))
            {
                EnableReadyButton();
                print("your character is: " + currentCharacterIndex);
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY))
        {
            print("your character is: " + currentCharacterIndex);
            EnableReadyButton();
        }
        else
        {
            EnableSelectionButtons();
            print("something fucked up along the way, try again");
        }
    }


    #region UI Button Controls
    void UpdateSelectButton()
    {
        if (!availableCharacterIDs.Contains(currentCharacterIndex))
        {
            selectButton.interactable = false;
        }
        else
        {
            selectButton.interactable = true;
        }
    }

    public void MoveToNextCharacter()
    {
        currentCharacterIndex++;

        CheckIfButtonsInteractable();

        Camera.main.transform.DOMoveX(spawnOffset * currentCharacterIndex, 0.5f).SetEase(Ease.OutSine);
    }

    public void MoveToPrevCharacter()
    {
        currentCharacterIndex--;

        CheckIfButtonsInteractable();

        Camera.main.transform.DOMoveX(spawnOffset * currentCharacterIndex, 0.5f).SetEase(Ease.OutSine);
    }

    void CheckIfButtonsInteractable()
    {
        if (characters.Length == 0)
        {
            PreviousCharacterButton.interactable = false;
            NextCharacterButton.interactable = false;
        }
        else if (currentCharacterIndex == 0 && currentCharacterIndex == characters.Length - 1)
        {
            PreviousCharacterButton.interactable = false;
            NextCharacterButton.interactable = false;
        }
        else if (currentCharacterIndex > 0 && currentCharacterIndex < characters.Length - 1)
        {
            PreviousCharacterButton.interactable = true;
            NextCharacterButton.interactable = true;
        }
        else if (currentCharacterIndex - 1 <= 0)
        {
            NextCharacterButton.interactable = true;
            PreviousCharacterButton.interactable = false;
        }
        else if (currentCharacterIndex + 1 >= characters.Length - 1)
        {
            PreviousCharacterButton.interactable = true;
            NextCharacterButton.interactable = false;
        }

        UpdateSelectButton();
    }

    void DisableSelectionButtons()
    {
        selectButton.interactable = false;
        NextCharacterButton.interactable = false;
        PreviousCharacterButton.interactable = false;
    }

    void EnableSelectionButtons()
    {
        CheckIfButtonsInteractable();
    }

    void EnableReadyButton()
    {
        selectButton.enabled = false;
        readyButton.enabled = true;
    }

    void EnableStartButton()
    {
        readyButton.enabled = false;
        startButton.enabled = true;
    }

    #endregion


    #region Debug

    [ContextMenu("Print Player Custom Properties")]
    public void PrintCustomProperties()
    {
        print(PhotonNetwork.LocalPlayer.CustomProperties[Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY]);
    }

    #endregion
}
