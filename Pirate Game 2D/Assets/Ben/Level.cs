using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{

    public PracticeComputeScript gooController;
    public Player player;
    public Camera mainCam;

    List<StaticDestructable> staticDestructables = new List<StaticDestructable>();
    List<GameObject> dynamicDestructables = new List<GameObject>();
    Vector2 playerStart;

    public void InitLevel()
    {
        ReadLevelFromFile();
        InitPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCamera(player.playerModel.transform.position);
        UpdateStatics();
        UpdateDynamics();
    }

    void InitPlayer()
    {
        player.SetStartPos(playerStart);
    }

    void UpdateStatics()
    {
        foreach(StaticDestructable o in staticDestructables)
        {
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

    void UpdateCamera(Vector3 playerPos)
    {
        mainCam.transform.position = playerPos - new Vector3(0,0,1);
    }

    void ReadLevelFromFile()
    {
        //TODO - make this do the level reading please, Alex

        //this is temporary, just to make the game work in the absence of the actual level editor
        playerStart = new Vector2(0,0);
    }
}
