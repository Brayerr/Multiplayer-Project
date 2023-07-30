using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shredder : MonoBehaviourPun
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.ARROW_TAG))
        {
            Destroy(other.gameObject);
        }

        else if (other.CompareTag(Constants.PLAYER_TAG))
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc.currentHP > 0)
            {
                pc.TakeDamage();
                pc.Respawn();
            }

            else
            {
                PhotonView pv = other.gameObject.GetPhotonView();
                if (pv != null && pv.IsMine)
                {
                    OnlineGameManager.Instance.photonView.RPC("RemovePlayer", Photon.Pun.RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
                    print("player died");
                }
            }

        }
    }
}
