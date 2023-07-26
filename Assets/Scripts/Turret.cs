using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField] MoveArrow arrow;
    void Start()
    {
        StartCoroutine(Shoooot());
    }

    IEnumerator Shoooot()
    {
        while (true)
        {
            MoveArrow shot = Instantiate(arrow, transform.position, Quaternion.identity);
            shot.transform.Rotate(transform.eulerAngles);
            yield return new WaitForSeconds(1);
        }
    }
}
