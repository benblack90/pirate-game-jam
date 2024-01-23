using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

[ExecuteInEditMode]
public class AutoPlaceTile : MonoBehaviour
{
    // Start is called before the first frame update

    public Tilemap tileMapCover;
    public Tilemap tileMapBase;
    //public Tilemap tileMapFloor;

    public TileBase coverTile;
    public TileBase baseTile;

    private int numberOfUpdates = 0;

    // Update is called once per frame
    void Update()
    {
        bool detectUpdates = false;
        Tilemap.tilemapTileChanged += delegate (Tilemap tilemap, Tilemap.SyncTile[] tiles)
        {
            //Debug.Log("EDITED");
            int numUpdates = tiles.Length;
            if (numberOfUpdates != numUpdates)
            {
                Debug.Log(numberOfUpdates);
    
                if (tilemap == tileMapBase)
                {
                    Debug.Log(tiles.Length);
                    for (int i = 0; i < numUpdates; i++)
                    {
                        if (tiles[i].tile)
                        {
                            Debug.Log(tiles[i].tile.ToString());
                            tileMapBase.SetTile(tiles[i].position, baseTile);
                            tileMapCover.SetTile(tiles[i].position, coverTile);
                            //tileMapFloor.SetTile(tiles[i].position, null);
                        }
                        else
                        {
                            //Debug.Log(tiles[i].tile.ToString());
                            tileMapCover.SetTile(tiles[i].position, null);
                            tileMapBase.SetTile(tiles[i].position, null);
                            
                        }

                    }
                }
                numberOfUpdates = numUpdates;
            }
            detectUpdates = true;
        };
        if (!detectUpdates) numberOfUpdates = 0;
        
    }

}
