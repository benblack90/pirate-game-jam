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
        Debug.Log("Trigger");
        if (other.gameObject.tag == "Player" && !forceLock)
        {
            Debug.Log("Player");
            foreach (Vector2Int position in doorPositionsBase.Keys)
            {
                tileMapRefBase.SetTile(new Vector3Int(position.x, position.y, 0), null);
                tileMapRefCover.SetTile(new Vector3Int(position.x, position.y, 0), null);
            }
        }

    }
    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Trigger");
        if (other.gameObject.tag == "Player" && !forceLock)
        {
            foreach (Vector2Int position in doorPositionsBase.Keys)
            {
                tileMapRefBase.SetTile(new Vector3Int(position.x, position.y, 0), doorPositionsBase[position]);
                tileMapRefCover.SetTile(new Vector3Int(position.x, position.y, 0), doorPositionsCover[position]);
            }
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
