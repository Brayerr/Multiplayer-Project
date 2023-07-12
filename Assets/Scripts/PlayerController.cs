using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviourPun
{
    public bool canControl = false;
    [SerializeField] private int speed;

    // Update is called once per frame
    void Update()
    {
        if (canControl && photonView.IsMine)
        {
            if (Input.GetKey(KeyCode.W))
                transform.Translate(Vector3.forward * Time.deltaTime * speed);
            if (Input.GetKey(KeyCode.S))
                transform.Translate(Vector3.back * Time.deltaTime * speed);
            if (Input.GetKey(KeyCode.A))
                transform.Translate(Vector3.left * Time.deltaTime * speed);
            if (Input.GetKey(KeyCode.D))
                transform.Translate(Vector3.right * Time.deltaTime * speed);
        }
    }
}
