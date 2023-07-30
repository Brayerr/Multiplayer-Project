using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class MoveArrow : MonoBehaviourPun
{
    float arrowForce = 2000;
    [SerializeField] Rigidbody rb;

    private void Start()
    {
        Shoot();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Vector3 hitPoint = collision.GetContact(0).point;
            PhotonNetwork.Instantiate("Explosion", hitPoint, Quaternion.identity);
            Debug.Log("hit");
        }
            Destroy(gameObject);
    }

    public void Shoot()
    {
        rb.AddForce(transform.forward * arrowForce);
    }


}
