using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineGameManager : MonoBehaviourPun
{
    int amountOfCharacters = 6;

    List<GameObject> playerPrefabs = new List<GameObject>();


    private void Start()
    {
        if(PhotonNetwork.IsConnectedAndReady)
        {
            GameObject go = PhotonNetwork.Instantiate($"PlayerPrefabs/playerPrefab {PhotonNetwork.LocalPlayer.CustomProperties[Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY]}", Vector3.zero, transform.rotation);
        }
    }
}
