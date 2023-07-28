using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    public void CloseApplication()
    {
        Application.Quit();
        Debug.Log("Application Closed");
    }
}
