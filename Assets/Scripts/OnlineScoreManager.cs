using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineScoreManager : MonoBehaviourPun
{
    [SerializeField] GameObject scoreboard;
    [SerializeField] GameObject playerCell;

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
        }
    }
}
