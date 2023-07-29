using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shredder : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow"))
        {
            Destroy(other.gameObject);
        }

        else if (other.CompareTag("Player"))
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc.currentHP > 0)
            {
                pc.TakeDamage();
                pc.Respawn();
            }

            else
            {
                print("player died");
            }

        }
    }
}
