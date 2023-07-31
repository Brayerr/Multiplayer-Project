using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerScoreCell : MonoBehaviour
{
    [SerializeField] Image cell;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI killsText;
    [SerializeField] TextMeshProUGUI deathsText;

    public int actorNum;


    public void SetNameText(string name)
    {
        nameText.text = name;
    }
    public void SetKillsText(string name)
    {
        killsText.text = name;
    }
    public void SetDeathsText(string name)
    {
        deathsText.text = name;
    }

    public void SetActorNum(int num)
    {
        actorNum = num;
    }
}
