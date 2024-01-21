using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level : MonoBehaviour
{

    public PracticeComputeScript gooController;
    public Tilemap destructableWalls;
    public GameObject playerModel;
    public Camera mainCam;

    Dictionary<Vector2Int, StaticDestructable> staticDestructables = new Dictionary<Vector2Int, StaticDestructable>();
    List<GameObject> dynamicDestructables = new List<GameObject>();
    Vector2 playerStart;

    private void Start()
    {
        LoadLevel();
    }

    public void InitLevel()
    {
        LoadLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
        UpdateDynamics();
    }

    IEnumerator CheckStaticsLoop()
    {
        UpdateStatics();

        yield return new WaitForSeconds(1.0f);
    }

    void UpdateStatics()
    {
        foreach(KeyValuePair<Vector2Int, StaticDestructable> o in staticDestructables) 
        {
            o.Value.CheckFireDamage();
           
            //assuming a 8x8 goo tile area per 32x32 block, here
            for(int i = 0; i < 8; i++)
            {
                
                float leftBorder = gooController.GetTileValue(o.Value.GetGooPos().x - 1, o.Value.GetGooPos().y + i, GridChannel.TYPE);
                float rightBorder = gooController.GetTileValue(o.Value.GetGooPos().x + 9, o.Value.GetGooPos().y + i, GridChannel.TYPE);
                float topBorder = gooController.GetTileValue(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y - 1, GridChannel.TYPE);
                float bottomBorder = gooController.GetTileValue(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y + 9, GridChannel.TYPE);

                if(leftBorder == (float) GridTileType.GOO_UNSPREADABLE || leftBorder == (float) GridTileType.GOO_SPREADABLE)
                {
                    //the -200.0f is arbitrary: essentially, I'm scaling down the temperature damage, so only stuff above 200 hurts
                    //staticDestructables ignore negative damage - see the damage method
                    o.Value.Damage(gooController.GetTileValue(o.Value.GetGooPos().x - 1, o.Value.GetGooPos().y + i, GridChannel.TEMP) - 200.0f);
                }
                else if(rightBorder == (float)GridTileType.GOO_UNSPREADABLE || rightBorder == (float)GridTileType.GOO_SPREADABLE)
                {
                    o.Value.Damage(gooController.GetTileValue(o.Value.GetGooPos().x +9, o.Value.GetGooPos().y + i, GridChannel.TEMP) - 200.0f);
                }
                else if (topBorder == (float)GridTileType.GOO_UNSPREADABLE || topBorder == (float)GridTileType.GOO_SPREADABLE)
                {
                    o.Value.Damage(gooController.GetTileValue(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y - 1, GridChannel.TEMP) - 200.0f);
                }
                else if (bottomBorder == (float)GridTileType.GOO_UNSPREADABLE || bottomBorder == (float)GridTileType.GOO_SPREADABLE)
                {
                    o.Value.Damage(gooController.GetTileValue(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y + 9, GridChannel.TEMP) - 200.0f);
                }
            }
            
            //check if adjacent to goo'd tile
                //check its temperature
                    //ignite if appropriate
                    //reduce hp if necessary
                //check if on fire
                    //reduce hp if necessary
            //if destroyed ... 
                //replace model with destroyed version
                //change tiles directly beneath to BLANK
                //change adjacent goo tiles to GOO_SPREADABLE
                //give points to player
                //remove from staticDestructables list
        }
    }

    void UpdateDynamics()
    {
        foreach (GameObject o in dynamicDestructables)
        {
            //check if adjacent to goo'd tile
                //check temperature
                //replace with burning/burnt model
                //give points to player
                //remove from dynamicDestructables list
        }
    }



    void LoadLevel()
    {
        //TODO - make this do the level reading please, Alex
        for (int x = destructableWalls.cellBounds.min.x; x < destructableWalls.cellBounds.max.x; x++)
        {
            for (int y = destructableWalls.cellBounds.min.y; y < destructableWalls.cellBounds.max.y; y++)
            {
                if (destructableWalls.GetTile(new Vector3Int(x, y, 0)) != null)
                {
                    Vector2Int graphicalPos = new Vector2Int(x, y);
                    staticDestructables.Add(graphicalPos, new StaticDestructable(100, graphicalPos, null, null));
                }
            }
        }

        //this is temporary, just to make the game work in the absence of the actual level editor
        playerStart = new Vector2Int(0, 0);
    }
}
