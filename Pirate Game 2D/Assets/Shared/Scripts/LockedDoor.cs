using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static LevelExit;

public class LockedDoor : DoorBase
{
    public Collectable keyReference;

    private string keyName;
    void Start()
    {
        keyName = keyReference.GetName();
        SetTilesForDoor();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger");
        if (!open && other.gameObject.CompareTag("Player"))
        {
            if (PlayerInventory.HasItem(keyName))
            {
                OpenDoor();
            }
        }

    }
    void OnTriggerExit2D(Collider2D other)
    {
        
    }

    void Update()
    {
        
    }
}
