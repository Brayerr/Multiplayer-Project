using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    [SerializeField]float sensX;
    [SerializeField]float sensY;

    [SerializeField] Transform orientation;

    float xRotation;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameObject go = PhotonNetwork.Instantiate("playerPrefab",Vector3.zero,transform.rotation);
        if (go.TryGetComponent<PlayerController>(out PlayerController control))
        {
            orientation = control.orientation;
            if(PhotonNetwork.IsMasterClient) GameManager.activePlayers.Add(control);
        }
    }

    private void Update()
    {
        transform.position = orientation.position;

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }
}
