using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviourPun
{
    public static List<PlayerController> activePlayers = new List<PlayerController>();

    private void Start()
    {
        PlayerController.LastManStanding += EndGameLoop;
    }

    private void OnDestroy()
    {
        PlayerController.LastManStanding -= EndGameLoop;
    }


    public void EndGameLoop()
    {
        if(PhotonNetwork.IsMasterClient) photonView.RPC("EndGameRPC", RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void EndGameRPC()
    {
        
    }
}
