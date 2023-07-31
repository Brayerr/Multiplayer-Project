using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class MoveArrow : MonoBehaviourPun
{
    public string UPDATE_EXPLOSION_ACTOR_NUM = nameof(UpdateActorNum);

    float arrowForce = 2000;
    [SerializeField] Rigidbody rb;
    public int actorNum;

    private void Start()
    {
        Shoot();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Vector3 hitPoint = collision.GetContact(0).point;
            var go = PhotonNetwork.Instantiate("Explosion", hitPoint, Quaternion.identity);
            if (go.TryGetComponent<Explosion>(out Explosion explosion))
            {
                //print("arrow actor num in explosion: " + actorNum);
                //explosion.photonView.RPC(explosion.UPDATE_EXPLOSION_ACTOR_NUM, RpcTarget.All, actorNum);
                explosion.photonView.TransferOwnership(photonView.Owner);
                print($"explosion actor num is {explosion.actorNum}");
            }
            else
            {
                print("coudent get player controller component");
            }
        }
            Destroy(gameObject);
    }

    public void Shoot()
    {
        rb.AddForce(transform.forward * arrowForce);
    }

    [PunRPC]
    public void UpdateActorNum(int newAcNum)
    {
        actorNum = newAcNum;
    }
}
