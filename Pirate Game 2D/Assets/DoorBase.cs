using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DoorBase : MonoBehaviour
{
    public bool forceLock = false;
    protected bool open = false;

    public Tilemap tileMapRefBase = null;
    public Tilemap tileMapRefCover = null;
    public List<DoorPositions> doorTiles = new List<DoorPositions>();


    protected Dictionary<Vector2Int, TileBase> doorPositionsBase = new Dictionary<Vector2Int, TileBase>();
    protected Dictionary<Vector2Int, TileBase> doorPositionsCover = new Dictionary<Vector2Int, TileBase>();

    public delegate void OnDoorOpenClose(Dictionary<Vector2Int, TileBase> doorPositions, GridTileType gridType);
    public static event OnDoorOpenClose onDoorOpenClose;


    void Start()
    {
        
    }

    protected void SetTilesForDoor()
    {
        foreach (DoorPositions doorPos in doorTiles)
        {
            int xDif = doorPos.doorEnd.x - doorPos.doorStart.x;
            int xDifDirection = xDif >= 0 ? 1 : -1;
            int yDif = doorPos.doorEnd.y - doorPos.doorStart.y;
            int yDifDirection = yDif >= 0 ? 1 : -1;

            for (int x = 0; x != xDif + xDifDirection; x += xDifDirection)
            {
                for (int y = 0; y != yDif + yDifDirection; y += yDifDirection)
                {
                    Vector2Int position = new Vector2Int(doorPos.doorStart.x + x, doorPos.doorStart.y + y + 1);
                    doorPositionsBase.Add(position, tileMapRefBase.GetTile(new Vector3Int(position.x, position.y, 0)));
                    doorPositionsCover.Add(position, tileMapRefCover.GetTile(new Vector3Int(position.x, position.y, 0)));
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Orange
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f);
        foreach (DoorPositions doorPos in doorTiles)
        {
            Gizmos.DrawWireCube(new Vector3(doorPos.doorStart.x + 0.5f, doorPos.doorStart.y + 0.5f + 1, 0), new Vector3(1, 1, 0.01f));
            Gizmos.DrawWireCube(new Vector3(doorPos.doorEnd.x + 0.5f, doorPos.doorEnd.y + 0.5f + 1, 0), new Vector3(1, 1, 0.01f));

            int xDif = doorPos.doorEnd.x - doorPos.doorStart.x;
            int xDifDirection = xDif >= 0 ? 1 : -1;
            int yDif = doorPos.doorEnd.y - doorPos.doorStart.y;
            int yDifDirection = yDif >= 0 ? 1 : -1;

            for (int x = 0; x != xDif + xDifDirection; x += xDifDirection)
            {
                for (int y = 0; y != yDif + yDifDirection; y += yDifDirection)
                {
                    Gizmos.DrawWireCube(new Vector3(doorPos.doorStart.x + 0.5f + x, doorPos.doorStart.y + 0.5f + y + 1, 0), new Vector3(1, 1, 0.01f));
                }
            }
        }
    }

    protected void OpenDoor()
    {
        open = true;
        onDoorOpenClose?.Invoke(doorPositionsBase, GridTileType.BLANK);
        foreach (Vector2Int position in doorPositionsBase.Keys)
        {
            tileMapRefBase.SetTile(new Vector3Int(position.x, position.y, 0), null);
            tileMapRefCover.SetTile(new Vector3Int(position.x, position.y, 0), null);
        }
        
    }

    protected void CloseDoor()
    {
        open = false;
        onDoorOpenClose?.Invoke(doorPositionsBase, GridTileType.STATIC);
        foreach (Vector2Int position in doorPositionsBase.Keys)
        {
            tileMapRefBase.SetTile(new Vector3Int(position.x, position.y, 0), doorPositionsBase[position]);
            tileMapRefCover.SetTile(new Vector3Int(position.x, position.y, 0), doorPositionsCover[position]);
        }
        
    }
}

[Serializable]
public class DoorPositions
{
    public Vector2Int doorStart;
    public Vector2Int doorEnd;
}
