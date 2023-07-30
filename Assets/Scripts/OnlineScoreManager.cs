using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class OnlineScoreManager : MonoBehaviourPun
{
    public static OnlineScoreManager Instance { get; private set; }

    [SerializeField] GameObject scoreboard;
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
            cell.SetKillsText("0");
            cell.SetDeathsText("0");
            cell.SetActorNum(item.ActorNumber);
            item.SetCustomProperties(new Hashtable { { Constants.PLAYER_KILLS_KEY, 0 } });
            item.SetCustomProperties(new Hashtable { { Constants.PLAYER_DEATHS_KEY, 0 } });
            scoreCells.Add(cell);
        }
    }

    public void UpdatePlayerKills(float actorNum, int amount)
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

    public void UpdatePlayerDeaths(float actorNum, int amount)
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
        if (PhotonNetwork.IsMasterClient)
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
    }

    #endregion
}