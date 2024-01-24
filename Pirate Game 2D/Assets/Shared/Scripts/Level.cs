using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

public class Level : MonoBehaviour
{
    class FireSprite
    {
        public FireSprite(GameObject fireSpritePrefab)
        { this.fireSpritePrefab = fireSpritePrefab; inUse = false; }

        public GameObject fireSpritePrefab;
        public bool inUse;

    }

    public GooController gooController;
    public Tilemap destructableWalls;
    public Tilemap destructableCovers;
    public GameObject playerModel;
    public Camera mainCam;
    public TileBase ashTile;
    [SerializeField] GameObject fireSpritePrefab;
    List<FireSprite> fireSpritePool = new List<FireSprite>();


    float timer;
    float levelTime = 180.0f;
    bool gooRelease;
    int gooPerGraphTile = 8;

    Dictionary<Vector2Int, StaticDestructable> staticDestructables = new Dictionary<Vector2Int, StaticDestructable>();
    List<Vector2Int> deathRow = new List<Vector2Int>();
    List<GameObject> dynamicDestructables = new List<GameObject>();
    Vector2 playerStart;

    public int GetGooPerTile()
    {
        return gooPerGraphTile;
    }
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
        InitLevel();
        StartCoroutine(CheckStaticsLoop());
    }

    public void InitLevel()
    {
        InitFirePool();
        LoadLevel();
    }

    // Update is called once per frame
    void Update()
    {
        if (gooRelease)
        {
            timer -= Time.deltaTime;
        }
        //UpdateDynamics();
    }

    IEnumerator CheckStaticsLoop()
    {
        WaitForSeconds wfs = new WaitForSeconds(1.0f);
        while (true)
        {
            ExecuteDeathRowStatics();
            UpdateStatics();
            gooController.SendTexToGPU();
            yield return wfs;
        }

    }

    void ExecuteDeathRowStatics()
    {
        if (deathRow.Count > 0)
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
        foreach (KeyValuePair<Vector2Int, StaticDestructable> o in staticDestructables)
        { 
            o.Value.CheckFireDamage();
            CheckAdjacentGoo(o);
            AdjacentStaticFireSpread(o);
        }
    }

    void CheckAdjacentGoo(KeyValuePair<Vector2Int, StaticDestructable> o)
    {
        for (int i = 0; i < gooPerGraphTile; i++)
        {

            float leftBorder = gooController.GetTileValue(o.Value.GetGooPos().x - 1, o.Value.GetGooPos().y + i, GridChannel.TYPE);
            float rightBorder = gooController.GetTileValue(o.Value.GetGooPos().x + gooPerGraphTile + 1, o.Value.GetGooPos().y + i, GridChannel.TYPE);
            float topBorder = gooController.GetTileValue(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y - 1, GridChannel.TYPE);
            float bottomBorder = gooController.GetTileValue(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y + gooPerGraphTile + 1, GridChannel.TYPE);
            float gooTemp;

            if (leftBorder == (float)GridTileType.GOO_UNSPREADABLE || leftBorder == (float)GridTileType.GOO_SPREADABLE)
            {
                //the -200.0f is arbitrary: essentially, I'm scaling down the temperature damage, so only stuff above 200 hurts
                //staticDestructables ignore negative damage - see the damage method
                gooTemp = gooController.GetTileValue(o.Value.GetGooPos().x - 1, o.Value.GetGooPos().y + i, GridChannel.TEMP);
                o.Value.Damage(gooTemp - 200.0f);
                o.Value.IgnitionFromGooCheck(gooTemp);
            }
            else if (rightBorder == (float)GridTileType.GOO_UNSPREADABLE || rightBorder == (float)GridTileType.GOO_SPREADABLE)
            {
                gooTemp = gooController.GetTileValue(o.Value.GetGooPos().x + gooPerGraphTile + 1, o.Value.GetGooPos().y + i, GridChannel.TEMP);
                o.Value.Damage(gooTemp - 200.0f);
                o.Value.IgnitionFromGooCheck(gooTemp);
            }
            else if (topBorder == (float)GridTileType.GOO_UNSPREADABLE || topBorder == (float)GridTileType.GOO_SPREADABLE)
            {
                gooTemp = gooController.GetTileValue(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y - 1, GridChannel.TEMP);
                o.Value.Damage(gooTemp - 200.0f);
                o.Value.IgnitionFromGooCheck(gooTemp);
            }
            else if (bottomBorder == (float)GridTileType.GOO_UNSPREADABLE || bottomBorder == (float)GridTileType.GOO_SPREADABLE)
            {
                gooTemp = gooController.GetTileValue(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y + gooPerGraphTile + 1, GridChannel.TEMP);
                o.Value.Damage(gooTemp - 200.0f);
                o.Value.IgnitionFromGooCheck(gooTemp);
            }
        }
    }

    void OnStaticDestroy(ObjectScorePair pair, Vector2Int graphicalPos)
    {

        for (int x = graphicalPos.x * gooPerGraphTile; x < graphicalPos.x * gooPerGraphTile + gooPerGraphTile; x++)
        {
            for (int y = graphicalPos.y * gooPerGraphTile; y < graphicalPos.y * gooPerGraphTile + gooPerGraphTile; y++)
            {             
                gooController.WriteToGooTile(x, y, GridChannel.TYPE, 0.0f);
                
            }        

        }

        for(int i = 0; i < gooPerGraphTile+1; i++)
        {
            Vector2Int bottomLeft = new Vector2Int(graphicalPos.x * gooPerGraphTile - 1 + i, graphicalPos.y * gooPerGraphTile - 1);
            Vector2Int topLeft = new Vector2Int(graphicalPos.x * gooPerGraphTile - 1, graphicalPos.y * gooPerGraphTile + gooPerGraphTile + 1 - i);
            Vector2Int bottomRight = new Vector2Int(graphicalPos.x * gooPerGraphTile + gooPerGraphTile + 1, graphicalPos.y -1 + i);
            Vector2Int topRight = new Vector2Int(graphicalPos.x * gooPerGraphTile + gooPerGraphTile + 1 - i, graphicalPos.y * gooPerGraphTile + gooPerGraphTile + 1);

            WriteToGooAlongEdge(bottomLeft);
            WriteToGooAlongEdge(topLeft);
            WriteToGooAlongEdge(bottomRight);
            WriteToGooAlongEdge(topRight);

        }

        destructableWalls.SetTile(new Vector3Int(graphicalPos.x, graphicalPos.y, 0), null);
        destructableCovers.SetTile(new Vector3Int(graphicalPos.x, graphicalPos.y, 0), null);
        deathRow.Add(graphicalPos);
    }

    void WriteToGooAlongEdge(Vector2Int corner)
    {
        if (gooController.GetTileValue(corner.x, corner.y, GridChannel.TYPE) == (float)GridTileType.GOO_UNSPREADABLE)
        {
            gooController.WriteToGooTile(corner.x, corner.y, GridChannel.TYPE, (float)GridTileType.GOO_SPREADABLE);
        }
    }

    public void ExtinguishFire(int index)
    {
        fireSpritePool[index].inUse = false;
        fireSpritePool[index].fireSpritePrefab.SetActive(false);
    }

    void AdjacentStaticFireSpread(KeyValuePair<Vector2Int, StaticDestructable> o)
    {
        if (o.Value.onFire)
        {
            for(int dist = 1; dist < 3; dist++)
            {
                for (int i = 0; i < dist * 2; i++)
                {
                    StaticDestructable n;
                    if (staticDestructables.TryGetValue(new Vector2Int(o.Key.x + dist - i, o.Key.y - dist), out n)) n.IgniteFromAdjacency(dist);
                    if(staticDestructables.TryGetValue(new Vector2Int(o.Key.x + i - dist, o.Key.y + dist), out n)) n.IgniteFromAdjacency(dist);
                    if(staticDestructables.TryGetValue(new Vector2Int(o.Key.x - dist, o.Key.y + i - dist), out n)) n.IgniteFromAdjacency(dist);
                    if(staticDestructables.TryGetValue(new Vector2Int(o.Key.x + dist, o.Key.y + dist - i), out n)) n.IgniteFromAdjacency(dist);                    
                }
            }      
        }
    }

    public int AddFireSpriteToLoc(Vector2Int loc)
    {
        for(int i = 0; i < fireSpritePool.Count; i++)
        {
            if (!fireSpritePool[i].inUse)
            {
                fireSpritePool[i].inUse = true;
                fireSpritePool[i].fireSpritePrefab.transform.position = new Vector3(loc.x, loc.y, 0);
                fireSpritePool[i].fireSpritePrefab.SetActive(true);
                return i;
            }
        }
        fireSpritePool.Add(new FireSprite(Instantiate(fireSpritePrefab)));
        fireSpritePool[fireSpritePool.Count - 1].fireSpritePrefab.SetActive(true);
        return fireSpritePool.Count - 1;
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


    void InitFirePool()
    {
        for(int i = 0; i < 20; i++)
        {
            FireSprite fs = new FireSprite(Instantiate(fireSpritePrefab));
            fs.fireSpritePrefab.SetActive(false);
            fireSpritePool.Add(fs);
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
                    staticDestructables.Add(graphicalPos, new StaticDestructable(100, graphicalPos, null, null, this));
                    staticDestructables[graphicalPos].gooController = gooController;
                    TurnGooTilesStatic(graphicalPos);
                }
            }
        }
        gooController.SendTexToGPU();

        //this is temporary, just to make the game work in the absence of the actual level editor
        playerStart = new Vector2Int(0, 0);
    }

    void TurnGooTilesStatic(Vector2Int graphicalPos)
    {
        for (int x = graphicalPos.x * gooPerGraphTile; x < graphicalPos.x * gooPerGraphTile + gooPerGraphTile; x++)
        {
            for (int y = graphicalPos.y * gooPerGraphTile; y < graphicalPos.y * gooPerGraphTile + gooPerGraphTile; y++)
            {
                gooController.WriteToGooTile(x, y, GridChannel.TYPE, 3.0f);
            }
        }
        gooController.WriteToGooTile(graphicalPos.x * gooPerGraphTile, graphicalPos.y *gooPerGraphTile, GridChannel.TYPE, 3.0f);
        
    }
}

