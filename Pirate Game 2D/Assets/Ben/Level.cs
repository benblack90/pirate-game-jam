using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{

    public PracticeComputeScript gooController;
    public GameObject playerModel;
    public Camera mainCam;

    List<StaticDestructable> staticDestructables = new List<StaticDestructable>();
    List<GameObject> dynamicDestructables = new List<GameObject>();
    Vector2 playerStart;

    public void InitLevel()
    {
        ReadLevelFromFile();
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



    void ReadLevelFromFile()
    {
        //TODO - make this do the level reading please, Alex

        //this is temporary, just to make the game work in the absence of the actual level editor
        playerStart = new Vector2(0,0);
    }
}
