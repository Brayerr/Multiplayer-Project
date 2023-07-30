using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public int ID { get; private set; }
    public bool isTaken;

    public void AssignID(int id)
    {
        ID = id;
    }
}
