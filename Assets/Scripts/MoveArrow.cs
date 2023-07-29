using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class MoveArrow : MonoBehaviourPun
{
    float arrowForce = 2000;
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject arrowHead;

    private void Start()
    {
        Shoot();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 hitPoint = collision.GetContact(0).point;
        photonView.RPC("ArrowExplosion", RpcTarget.MasterClient, hitPoint);
        //Explode(hitPoint);
        Debug.Log("hit");

    }

    void Explode(Vector3 hitPoint)
    {
        Collider[] hitColliders = Physics.OverlapSphere(hitPoint, 5);
        foreach (var hitCollider in hitColliders)
        {
            hitCollider.attachedRigidbody?.AddExplosionForce(300, hitPoint, 10, 0.05f);
        }
        Destroy(gameObject);
        Debug.Log("boom");
    }

    public void Shoot()
    {
        rb.AddForce(transform.forward * arrowForce);
    }

    #region RPC
    [PunRPC]
    public void ArrowExplosion(Vector3 hitPoint)
    {
        
    }
    #endregion
}
