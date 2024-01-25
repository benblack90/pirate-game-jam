using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnlockedDoor : DoorBase
{
  
    
    void Start()
    {
        SetTilesForDoor();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && !forceLock)
        {
            OpenDoor();
        }

    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && !forceLock)
        {
            CloseDoor();
        }

    }

    
    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnEnable()
    {
        GooChamber.onGooRelease += LockDoor;
    }
    private void OnDisable()
    {
        GooChamber.onGooRelease -= LockDoor;
    }
    void LockDoor()
    {
        forceLock = true;
    }
    
}
