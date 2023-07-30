using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviourPun
{
    [SerializeField] float sensX;
    [SerializeField] float sensY;

    [SerializeField] Transform orientation;

    float xRotation;
    float yRotation;

    private void Start()
    {
        OnlineGameManager.Instance.PlayerInitialized += SendPlayerCam;
        OnlineGameManager.Instance.SetPlayerCam(this);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if(orientation == null)
        {
            return;
        }
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
        print("called set orientation");
        this.orientation = orientation;
    }

    public void SendPlayerCam(int oldActorNumber)
    {
        if(oldActorNumber == photonView.CreatorActorNr)
        {
            OnlineGameManager.Instance.SetPlayerCam(this);
        }
    }
}
