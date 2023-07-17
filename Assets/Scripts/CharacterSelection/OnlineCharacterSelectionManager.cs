using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlineCharacterSelectionManager : MonoBehaviourPun
{
    private const string LOCK_IN_CHARACTER_RPC = nameof(LockInCharacter);

    [Header("Selection Buttons")]
    [SerializeField] Button selectButton;
    [SerializeField] Button NextCharacterButton;
    [SerializeField] Button PreviousCharacterButton;
    [Space]

    [Header("Characters")]
    [SerializeField] GameObject[] availableCharacters;
    [SerializeField] Transform ViewSpawnLocation;
    [SerializeField] float spawnOffset = 20f;

    int currentCharacterIndex = 0;

    private void Awake()
    {
        CheckIfButtonsInteractable();
    }

    private void Start()
    {
        LoadCharactersInScene();
    }

    private void Update()
    {
        print(PhotonNetwork.LocalPlayer.CustomProperties[Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY]);
    }

    #region RPC

    [PunRPC]
    public void LockInCharacter(PhotonMessageInfo photonMessageInfo)
    {
        //for each player in room
        //if player character id exists
        //check if id = current character index

        ExitGames.Client.Photon.Hashtable playerHashtable;
        playerHashtable = PhotonNetwork.LocalPlayer.CustomProperties;
        playerHashtable.Add(Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY, currentCharacterIndex);
        photonMessageInfo.Sender.SetCustomProperties(playerHashtable);
    }

    #endregion

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
        if (availableCharacters.Length == 0)
        {
            PreviousCharacterButton.interactable = false;
            NextCharacterButton.interactable = false;
        }
        else if (currentCharacterIndex == 0 && currentCharacterIndex == availableCharacters.Length - 1)
        {
            PreviousCharacterButton.interactable = false;
            NextCharacterButton.interactable = false;
        }
        else if (currentCharacterIndex > 0 && currentCharacterIndex < availableCharacters.Length - 1)
        {
            PreviousCharacterButton.interactable = true;
            NextCharacterButton.interactable = true;
        }
        else if (currentCharacterIndex - 1 <= 0)
        {
            NextCharacterButton.interactable = true;
            PreviousCharacterButton.interactable = false;
        }
        else if (currentCharacterIndex + 1 >= availableCharacters.Length - 1)
        {
            PreviousCharacterButton.interactable = true;
            NextCharacterButton.interactable = false;
        }
    }

    public void LoadCharactersInScene()
    {
        int counter = 0;
        foreach (GameObject character in availableCharacters)
        {
            Vector3 spawnPosition = new Vector3(ViewSpawnLocation.position.x + spawnOffset * counter, ViewSpawnLocation.position.y, ViewSpawnLocation.position.z);
            counter++;
            GameObject newCharacter = Instantiate(character, spawnPosition, Quaternion.identity);
        }
    }
}
