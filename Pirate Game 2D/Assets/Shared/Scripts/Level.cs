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
    float timer;
    float levelTime = 180.0f;
    bool gooRelease;

    Dictionary<Vector2Int, StaticDestructable> staticDestructables = new Dictionary<Vector2Int, StaticDestructable>();
    List<Vector2Int> deathRow = new List<Vector2Int>();
    List<GameObject> dynamicDestructables = new List<GameObject>();
    Vector2 playerStart;

    private void OnEnable()
    {
        StaticDestructable.onDestructableDestroyed += OnStaticDestroy;
        //subscribe to some events
    }
    private void OnDisable()
    {

        //unsubscribe from events
        StaticDestructable.onDestructableDestroyed -= OnStaticDestroy;

    }

    private void Start()
    {
        gooRelease = false;
        timer = levelTime;
        LoadLevel();
        StartCoroutine(CheckStaticsLoop());
    }

    public void InitLevel()
    {
        LoadLevel();
    }

    // Update is called once per frame
    void Update()
    {        
        if(gooRelease)
        {
            timer -= Time.deltaTime;
        }
        //UpdateDynamics();
    }

    IEnumerator CheckStaticsLoop()
    {
        WaitForSeconds wfs = new WaitForSeconds(1.0f);
        while(true)
        {
            ExecuteDeathRowStatics();
            UpdateStatics();

            yield return wfs;
        }
        
    }

    void ExecuteDeathRowStatics()
    {
        if(deathRow.Count > 0)
        {
            foreach (Vector2Int pos in deathRow)
            {
                staticDestructables.Remove(pos);
            }

            deathRow.Clear();
        }        
    }

    void UpdateStatics()
    {
        foreach(KeyValuePair<Vector2Int, StaticDestructable> o in staticDestructables) 
        {
            o.Value.CheckFireDamage();
            CheckAdjacentGoo(o);
            CheckAdjacentStatics(o);
        }
    }

    void CheckAdjacentGoo(KeyValuePair<Vector2Int, StaticDestructable> o)
    {
        //assuming a 8x8 goo tile area per 32x32 block, here
        for (int i = 0; i < 8; i++)
        {

            float leftBorder = gooController.GetTileValue(o.Value.GetGooPos().x - 1, o.Value.GetGooPos().y + i, GridChannel.TYPE);
            float rightBorder = gooController.GetTileValue(o.Value.GetGooPos().x + 9, o.Value.GetGooPos().y + i, GridChannel.TYPE);
            float topBorder = gooController.GetTileValue(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y - 1, GridChannel.TYPE);
            float bottomBorder = gooController.GetTileValue(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y + 9, GridChannel.TYPE);
            float gooTemp = 0.0f;

            if (leftBorder == (float)GridTileType.GOO_UNSPREADABLE || leftBorder == (float)GridTileType.GOO_SPREADABLE)
            {
                //the -200.0f is arbitrary: essentially, I'm scaling down the temperature damage, so only stuff above 200 hurts
                //staticDestructables ignore negative damage - see the damage method
                gooTemp = gooController.GetTileValue(o.Value.GetGooPos().x - 1, o.Value.GetGooPos().y + i, GridChannel.TEMP);
                o.Value.Damage(gooTemp - 180.0f);
                o.Value.IgnitionFromGooCheck(gooTemp);
            }
            else if (rightBorder == (float)GridTileType.GOO_UNSPREADABLE || rightBorder == (float)GridTileType.GOO_SPREADABLE)
            {
                gooTemp = gooController.GetTileValue(o.Value.GetGooPos().x + 9, o.Value.GetGooPos().y + i, GridChannel.TEMP);
                o.Value.Damage(gooTemp - 180.0f);
                o.Value.IgnitionFromGooCheck(gooTemp);
            }
            else if (topBorder == (float)GridTileType.GOO_UNSPREADABLE || topBorder == (float)GridTileType.GOO_SPREADABLE)
            {
                gooTemp = gooController.GetTileValue(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y - 1, GridChannel.TEMP);
                o.Value.Damage(gooTemp - 180.0f);
                o.Value.IgnitionFromGooCheck(gooTemp);
            }
            else if (bottomBorder == (float)GridTileType.GOO_UNSPREADABLE || bottomBorder == (float)GridTileType.GOO_SPREADABLE)
            {
                gooTemp = gooController.GetTileValue(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y + 9, GridChannel.TEMP);
                o.Value.Damage(gooTemp - 180.0f);
                o.Value.IgnitionFromGooCheck(gooTemp);
            }
        }
    }

    void OnStaticDestroy(ObjectScorePair pair, Vector2Int graphicalPos, Vector2Int topRight)
    {
        //this is TEMPORARY for testing - once the level editor writes to topRight, REMOVE
        topRight.x = graphicalPos.x + 1;
        topRight.y = graphicalPos.y + 1;

        for(int x = graphicalPos.x * 8; x < topRight.x * 8; x++)
        {
            for(int y = graphicalPos.y * 8; y < topRight.y * 8; y++)
            {
                gooController.WriteToGooTile(x, y, GridChannel.TYPE, 0.0f);
            }
        }
        deathRow.Add(graphicalPos);
    }

    void CheckAdjacentStatics(KeyValuePair<Vector2Int, StaticDestructable> o)
    {

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
                    staticDestructables[graphicalPos].gooController = gooController;
                }
            }
        }

        //this is temporary, just to make the game work in the absence of the actual level editor
        playerStart = new Vector2Int(0, 0);
    }
}
