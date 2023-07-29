using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shredder : MonoBehaviour
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
                print("player died");
            }

        }
    }
}
