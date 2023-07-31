using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class OnlineScoreManager : MonoBehaviourPun
{
    public static OnlineScoreManager Instance { get; private set; }

    [SerializeField] public GameObject scoreboard;
    [SerializeField] GameObject playerCell;

    [SerializeField] List<PlayerScoreCell> scoreCells = new List<PlayerScoreCell>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CreatePlayerCells();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) ToggleScoreboard();
    }

    public void ToggleScoreboard()
    {
        if (scoreboard.gameObject.activeInHierarchy) scoreboard.SetActive(false);
        else scoreboard.SetActive(true);
    }

    void CreatePlayerCells()
    {
        foreach (var item in PhotonNetwork.PlayerList)
        {
            var go = Instantiate(playerCell, scoreboard.transform);
            var cell = go.GetComponent<PlayerScoreCell>();
            cell.SetNameText(item.NickName);
            cell.SetActorNum(item.ActorNumber);
            cell.SetKillsText("0");
            cell.SetDeathsText("0");
            if (PhotonNetwork.IsMasterClient)
            {
                item.SetCustomProperties(new Hashtable { { Constants.PLAYER_KILLS_KEY, 0 } });
                item.SetCustomProperties(new Hashtable { { Constants.PLAYER_DEATHS_KEY, 0 } });
            }
            scoreCells.Add(cell);
        }
    }


    [PunRPC]
    public void UpdatePlayerKills(int actorNum, int amount)
    {
        foreach (var item in PhotonNetwork.PlayerList)
        {
            if (actorNum == item.ActorNumber)
            {
                item.SetCustomProperties(new Hashtable { { Constants.PLAYER_KILLS_KEY, (int)item.CustomProperties[Constants.PLAYER_KILLS_KEY] + amount } });
            }
        }
        photonView.RPC("UpdateScoreboard", RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void UpdatePlayerDeaths(int actorNum, int amount)
    {
        foreach (var item in PhotonNetwork.PlayerList)
        {
            if (actorNum == item.ActorNumber)
            {
                item.SetCustomProperties(new Hashtable { { Constants.PLAYER_DEATHS_KEY, (int)item.CustomProperties[Constants.PLAYER_DEATHS_KEY] + amount } });
            }
        }
        photonView.RPC("UpdateScoreboard", RpcTarget.AllViaServer);
    }




    #region RPCS

    [PunRPC]
    public void UpdateScoreboard()
    {

        foreach (var item in scoreCells)
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (item.actorNum == player.ActorNumber)
                {
                    item.SetKillsText(player.CustomProperties[Constants.PLAYER_KILLS_KEY].ToString());
                    item.SetDeathsText(player.CustomProperties[Constants.PLAYER_DEATHS_KEY].ToString());
                }
            }
        }

    }

    #endregion
}
