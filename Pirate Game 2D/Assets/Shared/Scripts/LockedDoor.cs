using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedDoor : DoorBase
{
    public GameObject keyReference;
    void Start()
    {
        SetTilesForDoor();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger");
        if (other.gameObject.tag == "Player" && !forceLock)
        {
            
        }

    }
    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Trigger");
        if (other.gameObject.tag == "Player" && !forceLock)
        {
            
        }

    }

    void Update()
    {
        
    }
}
