using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviourPun
{
    [SerializeField] PlayerController control;
    [SerializeField] float sensX;
    [SerializeField] float sensY;

    [SerializeField] Transform orientation;

    float xRotation;
    float yRotation;

    private void Start()
    {
        OnlineGameManager.Instance.SetPlayerCam(this);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //GameObject go = PhotonNetwork.Instantiate($"PlayerPrefab/playerPrefab {PhotonNetwork.LocalPlayer.CustomProperties[Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY]}",Vector3.zero,transform.rotation);
        //if (go.TryGetComponent<PlayerController>(out PlayerController control))
        //{
        //    orientation = control.orientation;
        //}
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


    public void SetOrientation(Transform orientation)
    {
        this.orientation = orientation;
    }
}
