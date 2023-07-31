using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public int ID;
    public bool isTaken;

    public void AssignID(int id)
    {
        ID = id;
    }
}
