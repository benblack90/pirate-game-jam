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

    public TileBase coverTile;
    public TileBase baseTile;

    private int numberOfUpdates = 0;
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        bool detectUpdates = false;
        Tilemap.tilemapTileChanged += delegate (Tilemap tilemap, Tilemap.SyncTile[] tiles)
        {
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
