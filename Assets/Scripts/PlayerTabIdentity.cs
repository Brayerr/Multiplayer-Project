using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTabIdentity : MonoBehaviourPun
{
    [SerializeField] Button kickButton;

    Player player;

    public void SetPlayer(Player player)
    {
        this.player = player;
    }

    public Player GetPlayer()
    {
        if (player == null) return null;
        return player;
    }

    public void KickButtonVisable(bool isVisable)
    {
        if (isVisable) { kickButton.gameObject.SetActive(true); 
            if (player == PhotonNetwork.MasterClient)
            {
                // kickButton.interactable = false; 
            }

            kickButton.interactable = false; // delete this after fixing player kicking
        }
        else { kickButton.gameObject.SetActive(false); }
    }

}
