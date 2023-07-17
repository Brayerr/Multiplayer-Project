using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveArrow : MonoBehaviour
{
    float speed = 1000;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 hitPoint = collision.GetContact(0).point;
        Explode(hitPoint, collision);
        
    }

    void Explode(Vector3 hitPoint, Collision coll)
    {
        coll.rigidbody.AddExplosionForce(20, hitPoint, 10, .01f, ForceMode.Impulse);
        Destroy(this.gameObject);
        Debug.Log("boom");
    }

    public void Shoot(Vector3 dir)
    {
        rb.AddForce(dir * speed);
    }
}
