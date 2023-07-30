using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MyRoomInfo : MonoBehaviour
{
    public RoomInfo RoomInfo { get; private set; }

    PunMultiManagerScript m_Script;

    public Button m_Button;

    private void Awake()
    {
        m_Script = GetComponentInParent<PunMultiManagerScript>();

        if (m_Script != null)
        {
            m_Button = GetComponent<Button>();
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
        m_Button.onClick.AddListener(SendMe);
    }

    public void SendMe()
    {
        m_Script.RoomPicked(RoomInfo);
    }
}
