using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;
using Photon.Pun.Demo.PunBasics;

public class Shredder : MonoBehaviourPun
{
    [SerializeField] Transform CameraLock;

    public static event Action<Player> OnPlayerDeath;

    private void OnTriggerExit(Collider other)
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
                if (pc.photonView.IsMine)
                {
                    if (pc.photonView.OwnerActorNr == pc.lastActorHit)
                    {
                        OnlineScoreManager.Instance.photonView.RPC("UpdatePlayerDeaths", RpcTarget.MasterClient, pc.photonView.OwnerActorNr, 1);
                    }
                    else
                    {
                        OnlineScoreManager.Instance.photonView.RPC("UpdatePlayerKills", RpcTarget.MasterClient, pc.lastActorHit, 1);
                        OnlineScoreManager.Instance.photonView.RPC("UpdatePlayerDeaths", RpcTarget.MasterClient, pc.photonView.OwnerActorNr, 1);
                    }
                }

                pc.TakeDamage();
                OnPlayerDeath.Invoke(pc.photonView.Controller);
                //pc.Respawn();
            }

            else
            {
                PhotonView pv = other.gameObject.GetPhotonView();
                if (pv != null && pv.IsMine)
                {
                    CheckPlayerToLookAt(other);
                    OnlineGameManager.Instance.photonView.RPC("RemovePlayer", Photon.Pun.RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
                    print("player died");
                }
            }

        }


    }

    public void CheckPlayerToLookAt(Collider other)
    {
        // Arena Look
        OnlineGameManager.Instance.GetPlayerCamera().SetOrientation(CameraLock);
        

        // Player Kill Cam
#if false
        foreach (var lookAtView in PhotonNetwork.PhotonViews)
        {
            

        }

#endif  
    }
}
