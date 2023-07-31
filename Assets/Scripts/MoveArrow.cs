using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class MoveArrow : MonoBehaviourPun
{   
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
                explosion.actorNum = actorNum;
                print($"explosion actor num is {explosion.actorNum}");
            }
            Debug.Log("hit");
        }
            Destroy(gameObject);
    }

    public void Shoot()
    {
        rb.AddForce(transform.forward * arrowForce);
    }


}
