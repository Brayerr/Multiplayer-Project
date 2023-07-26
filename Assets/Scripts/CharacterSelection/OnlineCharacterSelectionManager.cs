using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlineCharacterSelectionManager : MonoBehaviourPun
{
    private const string LOCK_IN_CHARACTER_RPC = nameof(LockInCharacter);
    private const string UPDATE_AVAILABLE_CHARACTERS_RPC = nameof(UpdateAvailableCharacters);

    [Header("Selection Buttons")]
    [SerializeField] Button selectButton;
    [SerializeField] Button NextCharacterButton;
    [SerializeField] Button PreviousCharacterButton;
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

    [ContextMenu("Print Player Custom Properties")]
    public void PrintCustomProperties()
    {
        print(PhotonNetwork.LocalPlayer.CustomProperties[Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY]);
    }

    #region RPC

    [PunRPC]
    public void LockInCharacter(PhotonMessageInfo photonMessageInfo)
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
                if ((int)player.CustomProperties[Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY] == currentCharacterIndex)
                {
                    print("someone has this character already");
                    return;
                }
            }
        }

        print("only i am locking in to character " + currentCharacterIndex);
        ExitGames.Client.Photon.Hashtable playerHashtable;
        playerHashtable = photonMessageInfo.Sender.CustomProperties;
        playerHashtable.Add(Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY, currentCharacterIndex);
        photonMessageInfo.Sender.SetCustomProperties(playerHashtable);

        photonView.RPC(UPDATE_AVAILABLE_CHARACTERS_RPC, RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void UpdateAvailableCharacters()
    {
        availableCharacterIDs.Remove(currentCharacterIndex);
        selectedCharacterIDs.Add(currentCharacterIndex);
    }

    #endregion

    public void TryLockInCharacter()
    {
        photonView.RPC(LOCK_IN_CHARACTER_RPC, RpcTarget.MasterClient);
        //check if id is in customproperties
        StartCoroutine(CheckIfCustomPropertyUpdated(Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY));
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
    IEnumerator CheckIfCustomPropertyUpdated(string CUSTOM_PROPERTY_KEY)
    {
        //every 1 seconds check if contains key
        for (int i = 0; i < 3; i++)
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY))
            {
                print("your character is: " + currentCharacterIndex);
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY))
        {
            print("your character is: " + currentCharacterIndex);
        }
        else
        {
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
        //set index ++
        currentCharacterIndex++;
        //check if index ++ == null
        //if null then disable next button
        CheckIfButtonsInteractable();

        Camera.main.transform.DOMoveX(spawnOffset * currentCharacterIndex, 0.5f).SetEase(Ease.OutSine);
    }

    public void MoveToPrevCharacter()
    {
        //set index --
        currentCharacterIndex--;
        //check if index -- == null
        //if null then disable prev button
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

    #endregion
}
