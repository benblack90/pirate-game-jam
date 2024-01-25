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

    [SerializeField] 
    public List<BaseCoverTile.Data> baseCoverTiles = new List<BaseCoverTile.Data>();
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
                //Debug.Log(numberOfUpdates);
    
                if (tilemap == tileMapBase)
                {
                    
                    for (int i = 0; i < numUpdates; i++)
                    {
                        TileBase tempBase = tiles[i].tile;
                        TileBase tempCover = CheckIfInBCList(tempBase);
                        if (tiles[i].tile && tempCover)
                        {
                            
                            tileMapBase.SetTile(tiles[i].position, tempBase);
                            tileMapCover.SetTile(tiles[i].position, tempCover);
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

    TileBase CheckIfInBCList(TileBase t)
    {
        foreach(BaseCoverTile.Data bc in baseCoverTiles)
        {
            if (t == bc.BaseTile) return bc.CoverTile;
        }
        return null;
    }
}
