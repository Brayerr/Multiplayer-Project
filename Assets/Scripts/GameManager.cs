using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPun
{
    public List<int> activePlayers = new List<int>();

    //private void Start()
    //{
    //    AddPlayers();
    //}

    //public void AddPlayers()
    //{
    //    int iterator = 0;
    //    foreach (var item in PhotonNetwork.PlayerList)
    //    {
    //        activePlayers.Add(iterator);
    //        iterator++;
    //    }
    //}

    [PunRPC]
    public void AddPlayer(int ID)
    {
        if (PhotonNetwork.IsMasterClient) activePlayers.Add(ID);
    }

    [PunRPC]
    public void RemovePlayer(int Id)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var item in activePlayers)
            {
                if (item == Id) activePlayers.Remove(item);
            }
            //activePlayers.Remove(activePlayers.Count);

            if (activePlayers.Count <= 1) EndGameLoop();
        }
    }

    public void EndGameLoop()
    {
        if (PhotonNetwork.IsMasterClient) photonView.RPC(Constants.END_GAME_RPC, RpcTarget.AllViaServer);
    }


    [PunRPC]
    public void EndGameRPC()
    {
        print("restarting");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Exit the room
        PhotonNetwork.LeaveRoom();
        string[] customToRemove = new string[3];
        customToRemove[0] = Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY;
        customToRemove[1] = Constants.PLAYER_READY_PROPERTY_KEY;
        customToRemove[2] = Constants.PING_HASHTABLE_NAME;
        PhotonNetwork.RemovePlayerCustomProperties(customToRemove);     // the array has to be completely filled with strings
        SceneManager.LoadScene(0);
    }
}
