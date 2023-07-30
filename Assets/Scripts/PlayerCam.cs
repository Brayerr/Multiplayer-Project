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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameObject go = PhotonNetwork.Instantiate($"PlayerPrefabs/playerPrefab {PhotonNetwork.LocalPlayer.CustomProperties[Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY]}", new Vector3(0, 3, -8), transform.rotation);
        if (go.TryGetComponent<PlayerController>(out PlayerController control))
        {
            orientation = control.orientation;

            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY))
            {
                control.ID = (int)PhotonNetwork.LocalPlayer.CustomProperties.GetValueOrDefault(Constants.PLAYER_CHARACTER_ID_PROPERTY_KEY);
                print(control.ID);
                if (control.ID == null) print("ID is null"); 
                photonView.RPC("AddPlayer", RpcTarget.MasterClient, control.ID);
            }

            else print("failed setting ID");
            return;
        }
        print("didnt find controller");
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
