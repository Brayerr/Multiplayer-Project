using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Explosion : MonoBehaviourPun
{
    // Start is called before the first frame update
    void Start()
    {
        Explode(transform.position);
    }

    void Explode(Vector3 hitPoint)
    {
        Grow();
        Collider[] hitColliders = Physics.OverlapSphere(hitPoint, 5);
        foreach (var hitCollider in hitColliders)
        {
            //hitCollider.attachedRigidbody?.AddExplosionForce(500, hitPoint, 5, 0.05f);
            hitCollider.attachedRigidbody?.AddForce((hitPoint - hitCollider.transform.position) * -20, ForceMode.Impulse);
            Debug.Log("boom");
        }
        Invoke("Destroye", 1f);
    }

    void Grow()
    {
        Tween tween = transform.DOScale(5, 1);
        tween.OnComplete(() =>
        {
            tween.Kill();
        });
    }

    void Destroye()
    {
        Destroy(gameObject);
    }
}
