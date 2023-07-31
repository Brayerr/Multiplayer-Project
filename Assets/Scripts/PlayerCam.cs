using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerCam : MonoBehaviourPun
{
    [SerializeField] float sensX;
    [SerializeField] float sensY;

    [SerializeField] Transform orientation;

    float xRotation;
    float yRotation;

    [SerializeField] bool SICKO_MODE = false;
    public bool setNewOwner = false;

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
            if (OnlineGameManager.Instance.GetLocalPlayerController() != null)
            {
                SetOrientation(OnlineGameManager.Instance.GetLocalPlayerController().orientation);
            }
            return;
        }
        transform.position = orientation.position;

        if (SICKO_MODE)
        {
            return;
        }
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
            setNewOwner = true;
            OnlineGameManager.Instance.SetPlayerCam(this);
            SetOrientation(OnlineGameManager.Instance.GetLocalPlayerController().orientation);
        }
    }

    public void ActivateSickoMode()
    {
        SICKO_MODE = true;
    }
}
