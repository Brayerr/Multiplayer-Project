using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviourPun
{
    // Start is called before the first frame update
    void Start()
    {
        Explode(transform.position);
    }

    void Explode(Vector3 hitPoint)
    {
        Collider[] hitColliders = Physics.OverlapSphere(hitPoint, 5);
        foreach (var hitCollider in hitColliders)
        {
            hitCollider.attachedRigidbody?.AddExplosionForce(300, hitPoint, 10, 0.05f);
            Debug.Log("boom");
        }
        Destroy(gameObject);
    }
}
