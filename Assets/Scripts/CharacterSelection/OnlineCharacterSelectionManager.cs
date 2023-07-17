using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlineCharacterSelectionManager : MonoBehaviourPun
{
    [SerializeField] Button selectButton;
    [SerializeField] Button NextCharacterButton;
    [SerializeField] Button PreviousCharacterButton;

    [SerializeField] GameObject[] availableCharacters;

    private void Start()
    {
        if(PhotonNetwork.IsConnectedAndReady)
        {



        }
    }
}
