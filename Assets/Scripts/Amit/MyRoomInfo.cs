using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Photon.Realtime;

public class MyRoomInfo : MonoBehaviour
{
    public static RoomInfo RoomInfo { get; private set; }

    PunMultiManagerScript m_Script;

    Button m_Button;


    private void Start()
    {
        m_Script = GetComponentInParent<PunMultiManagerScript>();
        if (m_Script != null)
        {
            m_Button = GetComponent<Button>();
            m_Button.onClick.AddListener(SendMe);
        }

        

        else
        {
            Destroy(this);
            Debug.LogWarning($"{this.gameObject.name} Does not have the desired parent, You are fucked!");
        }
    }

    public void SetRoomInfo(RoomInfo roominfo)
    {
        RoomInfo = roominfo;
    }

    void SendMe()
    {
        m_Script.RoomPicked(RoomInfo);
    }
}
