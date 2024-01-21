using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level : MonoBehaviour
{

    public PracticeComputeScript gooController;
    public Tilemap destructableWall;
    public GameObject playerModel;
    public Camera mainCam;

    List<StaticDestructable> staticDestructables = new List<StaticDestructable>();
    List<GameObject> dynamicDestructables = new List<GameObject>();
    Vector2 playerStart;

    public void InitLevel()
    {
        LoadLevel();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStatics();
        UpdateDynamics();

    }

    void UpdateStatics()
    {
        foreach(StaticDestructable o in staticDestructables)
        {
            o.CheckFireDamage();
           
            //assuming a 16x16 goo tile area per 32x32 block, here
            for(int i = 0; i < 16; i++)
            {
                
                float leftBorder = gooController.GetTileValue(o.GetGooPos().x - 1, o.GetGooPos().y + i, GridChannel.TYPE);
                float rightBorder = gooController.GetTileValue(o.GetGooPos().x + 17, o.GetGooPos().y + i, GridChannel.TYPE);
                float topBorder = gooController.GetTileValue(o.GetGooPos().x + i, o.GetGooPos().y - 1, GridChannel.TYPE);
                float bottomBorder = gooController.GetTileValue(o.GetGooPos().x + i, o.GetGooPos().y + 17, GridChannel.TYPE);

                if(leftBorder == (float) GridTileType.GOO_UNSPREADABLE || leftBorder == (float) GridTileType.GOO_SPREADABLE)
                {
                    //the -200.0f is arbitrary: essentially, I'm scaling down the temperature damage, so only stuff above 200 hurts
                    //staticDestructables ignore negative damage - see the damage method
                    o.Damage(gooController.GetTileValue(o.GetGooPos().x - 1, o.GetGooPos().y + i, GridChannel.TEMP) - 200.0f);
                }
                else if(rightBorder == (float)GridTileType.GOO_UNSPREADABLE || rightBorder == (float)GridTileType.GOO_SPREADABLE)
                {
                    o.Damage(gooController.GetTileValue(o.GetGooPos().x +17, o.GetGooPos().y + i, GridChannel.TEMP) - 200.0f);
                }
                else if (topBorder == (float)GridTileType.GOO_UNSPREADABLE || topBorder == (float)GridTileType.GOO_SPREADABLE)
                {
                    o.Damage(gooController.GetTileValue(o.GetGooPos().x + i, o.GetGooPos().y - 1, GridChannel.TEMP) - 200.0f);
                }
                else if (bottomBorder == (float)GridTileType.GOO_UNSPREADABLE || bottomBorder == (float)GridTileType.GOO_SPREADABLE)
                {
                    o.Damage(gooController.GetTileValue(o.GetGooPos().x + i, o.GetGooPos().y + 17, GridChannel.TEMP) - 200.0f);
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


        //this is temporary, just to make the game work in the absence of the actual level editor
        playerStart = new Vector2Int(0, 0);
    }
}
