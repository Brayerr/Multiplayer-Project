using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerNameLookAt : MonoBehaviourPun
{
    [SerializeField] TMP_Text gameObjectText;

    public Vector3 Fix;

    private void Start()
    {
        if (photonView  != null)
        {
            UpdatePlayerName(photonView.Owner.NickName);
        }
    }

    void UpdatePlayerName(string nickName)
    {
        gameObjectText.text = nickName;
        if (photonView.IsMine)
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        transform.LookAt(Camera.main.transform.forward);
    }
}
