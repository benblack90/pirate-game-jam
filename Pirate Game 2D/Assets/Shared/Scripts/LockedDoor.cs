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
    void OnDrawGizmosSelected()
    {
        // Orange
        Gizmos.color = new Color(0.0f, 0.0f, 1.0f);
        Gizmos.DrawLine(this.transform.position, keyReference.transform.position);
        foreach (DoorPositions doorPos in doorTiles)
        {
            Gizmos.DrawLine(this.transform.position, keyReference.transform.position);
           
        }
    }
}
