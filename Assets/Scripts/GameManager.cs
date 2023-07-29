using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

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
        if(PhotonNetwork.IsMasterClient) photonView.RPC(Constants.END_GAME_RPC, RpcTarget.AllViaServer);
    }


    [PunRPC]
    public void EndGameRPC()
    {
        print("restarting");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(0);
    }
}
