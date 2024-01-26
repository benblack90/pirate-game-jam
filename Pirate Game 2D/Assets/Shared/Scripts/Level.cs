using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level : MonoBehaviour
{
    class FireSprite
    {
        public FireSprite(GameObject fireSpritePrefab)
        { this.fireSpritePrefab = fireSpritePrefab; inUse = false; }

        public GameObject fireSpritePrefab;
        public bool inUse;

    }
    [Header("Tile Settings")]
    public GooController gooController;
    public Tilemap destructableWalls;
    public Tilemap destructableCovers;
    public Tilemap baseWalls;
    [Header("Player Settings")]
    [SerializeField] CustomCharacterController playerController;
    [SerializeField] PlayerValuesManager playerValues;
    public Camera mainCam;
    [Header("Fire Settings")]
    public TileBase ashTile;
    [SerializeField] GameObject fireSpritePrefab;
    List<FireSprite> fireSpritePool = new List<FireSprite>();
    [Header("End UI")]
    public LoseGame loseGameRef;


    float timer;
    float levelTime = 180.0f;
    bool gooRelease;
    const int gooPerGraphTile = 8;
    const float gooTempThresholdStatics = 200.0f;
    const float gooTempThresholdDynamics = 180.0f;
    const float gooTempThresholdPlayer = 190.0f;

    Dictionary<Vector2Int, StaticDestructable> staticDestructables = new Dictionary<Vector2Int, StaticDestructable>();
    List<Vector2Int> deathRow = new List<Vector2Int>();
    List<DynamicDestructable> dynamicDestructables = new List<DynamicDestructable>();
    Vector2 playerStart;

    public delegate void OnTimerChange(int timer);
    public static event OnTimerChange onTimerChange;



    public int GetGooPerTile()
    {
        return gooPerGraphTile;
    }
    private void OnEnable()
    {
        StaticDestructable.onStaticDestroyed += OnStaticDestroy;
        DoorBase.onDoorOpenClose += ChangeGooTilesForDoors;
        GooChamber.onGooRelease += SetGooReleaseTrue;
        //subscribe to some events
    }
    private void OnDisable()
    {
        //unsubscribe from events
        StaticDestructable.onStaticDestroyed -= OnStaticDestroy;
        DoorBase.onDoorOpenClose -= ChangeGooTilesForDoors;
        GooChamber.onGooRelease -= SetGooReleaseTrue;
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
            onTimerChange?.Invoke((int)timer);            
        }
        UpdateDynamics();
        UpdatePlayerHealth();
        if (Input.GetKeyDown(KeyCode.K))
        {
            playerValues.SubtractHealth(playerValues.GetHealth());
        }
        if (CheckLoseConditions() && !loseGameRef.IsEnabled())
        {
            playerController._characeterActive = false;
            Debug.Log("LOL U SUCK");
            loseGameRef.EnableScreen();
        }
    }

    void SetGooReleaseTrue()
    {
        gooRelease = true;
    }

    bool CheckLoseConditions()
    {
        if (playerValues.GetHealth() <= 0 || timer <= 0)
            return true;

        else return false;
    }
    

    void UpdatePlayerHealth()
    {

        Vector2Int playerGooPos = playerController.GetPlayerPos() * gooPerGraphTile;
        float temp;
        float maxTemp = 0.0f;
        for(int i = -1; i < 2; i++)
        {
            float tileType = gooController.GetTileValue(playerGooPos.x + i, playerGooPos.y, GridChannel.TYPE);
            if (tileType < 1f || tileType > 2f)continue;
            temp = CheckTempOfPlayerGooTiles(tileType, playerGooPos.x + i, playerGooPos.y);
            maxTemp = (temp > maxTemp) ? temp : maxTemp;
        }
        playerValues.SubtractHealth((int)((maxTemp - gooTempThresholdPlayer) * Time.deltaTime));
    }

    float CheckTempOfPlayerGooTiles(float tileType, int posX, int posY)
    {
        
        float temp = gooController.GetTileValue(posX, posY, GridChannel.TEMP);
        return temp;
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
            float bottomBorder = gooController.GetTileValue(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y - 1, GridChannel.TYPE);
            float topBorder = gooController.GetTileValue(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y + gooPerGraphTile + 1, GridChannel.TYPE);

            IgniteAndDamageStatics(new Vector2Int(o.Value.GetGooPos().x - 1, o.Value.GetGooPos().y + i), o.Value, leftBorder);
            IgniteAndDamageStatics(new Vector2Int(o.Value.GetGooPos().x + gooPerGraphTile + 1, o.Value.GetGooPos().y + i),o.Value, rightBorder);
            IgniteAndDamageStatics(new Vector2Int(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y - 1), o.Value, bottomBorder);
            IgniteAndDamageStatics(new Vector2Int(o.Value.GetGooPos().x + i, o.Value.GetGooPos().y + gooPerGraphTile + 1), o.Value, topBorder);
        }
    }

    void IgniteAndDamageStatics(Vector2Int gooPos, StaticDestructable stObj, float tileType)
    {
        if(tileType == (float)GridTileType.GOO_UNSPREADABLE || tileType == (float)GridTileType.GOO_SPREADABLE)
        {
            float gooTemp = gooController.GetTileValue(gooPos.x, gooPos.y, GridChannel.TEMP);
            stObj.GooDamage(gooTemp - gooTempThresholdStatics);
            if(stObj.destroyed) return;
            stObj.IgnitionFromGooCheck(gooTemp);
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

        for (int i = 0; i < gooPerGraphTile + 1; i++)
        {
            Vector2Int bottomLeft = new Vector2Int(graphicalPos.x * gooPerGraphTile - 1 + i, graphicalPos.y * gooPerGraphTile - 1);
            Vector2Int topLeft = new Vector2Int(graphicalPos.x * gooPerGraphTile - 1, graphicalPos.y * gooPerGraphTile + gooPerGraphTile + 1 - i);
            Vector2Int bottomRight = new Vector2Int(graphicalPos.x * gooPerGraphTile + gooPerGraphTile + 1, graphicalPos.y - 1 + i);
            Vector2Int topRight = new Vector2Int(graphicalPos.x * gooPerGraphTile + gooPerGraphTile + 1 - i, graphicalPos.y * gooPerGraphTile + gooPerGraphTile + 1);

            SetGooAtEdgeToSpreadable(bottomLeft);
            SetGooAtEdgeToSpreadable(topLeft);
            SetGooAtEdgeToSpreadable(bottomRight);
            SetGooAtEdgeToSpreadable(topRight);

        }

        destructableWalls.SetTile(new Vector3Int(graphicalPos.x, graphicalPos.y, 0), null);
        destructableCovers.SetTile(new Vector3Int(graphicalPos.x, graphicalPos.y, 0), null);
        deathRow.Add(graphicalPos);
    }


    void SetGooAtEdgeToSpreadable(Vector2Int corner)
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
        if (!o.Value.onFire) return;

        for (int dist = 1; dist < 3; dist++)
        {
            for (int i = 0; i < dist * 2; i++)
            {
                StaticDestructable n;
                if (staticDestructables.TryGetValue(new Vector2Int(o.Key.x + dist - i, o.Key.y - dist), out n)) n.IgniteFromAdjacency(dist);
                if (staticDestructables.TryGetValue(new Vector2Int(o.Key.x + i - dist, o.Key.y + dist), out n)) n.IgniteFromAdjacency(dist);
                if (staticDestructables.TryGetValue(new Vector2Int(o.Key.x - dist, o.Key.y + i - dist), out n)) n.IgniteFromAdjacency(dist);
                if (staticDestructables.TryGetValue(new Vector2Int(o.Key.x + dist, o.Key.y + dist - i), out n)) n.IgniteFromAdjacency(dist);
            }
        }

    }

    public int AddFireSpriteToLoc(Vector2Int loc)
    {
        for (int i = 0; i < fireSpritePool.Count; i++)
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
        foreach (DynamicDestructable o in dynamicDestructables)
        {
            if (!o.active) continue;
            for (int i = 0; i < o.GetWidth(); i++)
            {
                float type = gooController.GetTileValue(o.GetGooPos().x + i, o.GetGooPos().y - 1, GridChannel.TYPE);
                if (type == 1.0f || type == 2.0f)
                {
                    float gooTemp = gooController.GetTileValue(o.GetGooPos().x + i, o.GetGooPos().y - 1, GridChannel.TEMP);
                    ;
                    if (gooTemp > gooTempThresholdDynamics)
                    {
                        o.GooDamage(gooTemp - gooTempThresholdDynamics);
                        break;
                    }
                }
            }
        }
    }


    void InitFirePool()
    {
        for (int i = 0; i < 40; i++)
        {
            FireSprite fs = new FireSprite(Instantiate(fireSpritePrefab));
            fs.fireSpritePrefab.SetActive(false);
            fireSpritePool.Add(fs);
        }
    }
    void LoadLevel()
    {
        //TODO - make this do the level reading please, Alex
        for (int x = baseWalls.cellBounds.min.x; x < baseWalls.cellBounds.max.x; x++)
        {
            for (int y = baseWalls.cellBounds.min.y; y <baseWalls.cellBounds.max.y; y++)
            {
                if (destructableWalls.GetTile(new Vector3Int(x, y, 0)) != null)
                {
                    Vector2Int graphicalPos = new Vector2Int(x, y);
                    staticDestructables.Add(graphicalPos, new StaticDestructable(100, graphicalPos, this));
                    staticDestructables[graphicalPos].gooController = gooController;
                    TurnGooTilesStatic(graphicalPos);
                }
                TileBase tb = baseWalls.GetTile(new Vector3Int(x, y, 0));
                if (baseWalls.GetTile(new Vector3Int(x, y, 0)) != null)
                {
                    TurnGooTilesStatic(new Vector2Int(x, y));
                }                
            }
        }
        gooController.SendTexToGPU();

        List<GameObject> temp = Enumerable.ToList<GameObject>(GameObject.FindGameObjectsWithTag("Dynamic"));
        foreach (GameObject o in temp)
        {
            dynamicDestructables.Add(o.GetComponent<DynamicDestructable>());
            dynamicDestructables[dynamicDestructables.Count - 1].Init(this);
        }

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
        gooController.WriteToGooTile(graphicalPos.x * gooPerGraphTile, graphicalPos.y * gooPerGraphTile, GridChannel.TYPE, 3.0f);

    }

    void ChangeGooTilesForDoors(Dictionary<Vector2Int, TileBase> doorPositions, GridTileType gridType)
    {
        foreach(Vector2Int pos in doorPositions.Keys)
        {
            for(int i = 0; i < 8; i++)
            {
                for(int j = 0; j < 8; j++)
                {
                    gooController.WriteToGooTile(pos.x * gooPerGraphTile + i, pos.y * gooPerGraphTile + j, GridChannel.TYPE, (float)gridType);
                    
                }
            }
            
        }
        gooController.SendTexToGPU();
    }
}

