using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System;
using System.IO;
using UnityEditor;

public enum ObjectType
{
    Wall,
    MAX
}

public class ToolSelector : MonoBehaviour
{
    public GameObject tileDropdown;
    public TMPro.TMP_Dropdown toolSelection;
    public TMPro.TMP_InputField levelName;
    public LevelEditorComputeScript cs;
    public GameObject spriteImage;
    public Tilemap walls;
    public Tilemap cover;
    public Tilemap floor;
    public GameObject gooPlane;
    public GameObject player;
    private Tile selectedTile;
    private List<Vector2Int> selectedTiles;
    private Dictionary<Vector2Int, ObjectType> staticObjects;

    // Start is called before the first frame update
    void Start()
    {
        selectedTiles = new List<Vector2Int>() { new Vector2Int(0, 0) };
        staticObjects = new Dictionary<Vector2Int, ObjectType>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.mousePosition.x < 250 && Input.mousePosition.y > 350) return;
        switch (toolSelection.value)
        {
            case 1:
                PlaceSelection();
                break;
            case 2:
                DeleteSelection();
                break;
            case 3:
                PlacePlayer();
                break;
        }
    }

    public void SaveLevel()
    {
        LevelData data = new LevelData();
        List<ObjectData> dataList = new List<ObjectData>();
        for(int i = 0; i < 3200; i++)
        {
            for(int j = 0; j < 3200; j++)
            {
                Vector2Int key = new Vector2Int(i, j);
                if(staticObjects.ContainsKey(key))
                {
                    switch(staticObjects[key]) 
                    {
                        case ObjectType.Wall:
                            Vector2Int gooCoord = new Vector2Int((int)(3200 * ((key.x + (gooPlane.transform.localScale.x/2)) / gooPlane.transform.localScale.x)), 
                                (int)(3200 * ((key.y + (gooPlane.transform.localScale.y/2)) / gooPlane.transform.localScale.y)) + 1);
                            dataList.Add(new ObjectData(0, gooCoord, key));
                            break;
                    }
                }
            }
        }
        data.objectData = dataList;
        data.playerPos = player.transform.position;
        string dataDirPath = Application.persistentDataPath;
        string dataFileName = levelName.text + ".json";

        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonUtility.ToJson(data, true);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error when saving data to file: " + fullPath + "\n" + ex);
        }
    }

    public void TileSelection()
    {
        switch (tileDropdown.GetComponent<TMPro.TMP_Dropdown>().value)
        {
            case 0:
                spriteImage.GetComponent<Image>().sprite = null;
                selectedTile = null;
                break;
            case 1:
                spriteImage.GetComponent<Image>().sprite = tileDropdown.GetComponent<TMPro.TMP_Dropdown>().options[1].image;
                selectedTile = Resources.Load("testspritesheet_58") as Tile;
                break;
            case 2:
                spriteImage.GetComponent<Image>().sprite = tileDropdown.GetComponent<TMPro.TMP_Dropdown>().options[2].image;
                selectedTile = Resources.Load("testspritesheet_32") as Tile;
                break;
        }
    }

    private void PlaceSelection()
    {
        Vector2 mouseCoordsFloat = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
        switch (tileDropdown.GetComponent<TMPro.TMP_Dropdown>().value)
        {
            case 0:
                return;
            case 1:
                PlaceWall(new Vector2Int((int)mouseCoordsFloat.x, (int)mouseCoordsFloat.y));
                break;
            case 2:
                PlaceFloor(new Vector2Int((int)mouseCoordsFloat.x, (int)mouseCoordsFloat.y));
                break;
        }
    }

    private void PlaceWall(Vector2Int mouseCoords)
    {
        Vector3Int tileCoords = new Vector3Int(mouseCoords.x, mouseCoords.y, 0);
        if (Input.GetMouseButtonDown(0) && walls.GetTile(tileCoords) == null && floor.GetTile(tileCoords) == null)
        {
            cs.SetTypeOfManyTiles(selectedTiles, GridTileType.STATIC);
            walls.SetTile(tileCoords, selectedTile);
            staticObjects.Add(mouseCoords, ObjectType.Wall);
        }
        mouseCoords += new Vector2Int((int)gooPlane.transform.localScale.x, (int)gooPlane.transform.localScale.y);
        int xCoord = (int)(3200 * (mouseCoords.x / gooPlane.transform.localScale.x));
        int yCoord = (int)(3200 * (mouseCoords.y / gooPlane.transform.localScale.y)) + 1;
        List<Vector2Int> gooCoords = new List<Vector2Int>();
        if (selectedTiles[0].x != xCoord || selectedTiles[0].y != yCoord)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    gooCoords.Add(new Vector2Int(xCoord + i, yCoord + j));
                }
            }
        }
        if (gooCoords.Count > 0)
        {
            Vector2 screenCoords = Camera.main.WorldToScreenPoint(mouseCoords - new Vector2(gooPlane.transform.localScale.x, gooPlane.transform.localScale.y)) +
                new Vector3(spriteImage.GetComponent<RectTransform>().rect.width, spriteImage.GetComponent<RectTransform>().rect.height, 0);
            spriteImage.transform.position = screenCoords;
            selectedTiles = gooCoords;
        }
    }

    private void PlaceFloor(Vector2Int mouseCoords)
    {
        Vector3Int tileCoords = new Vector3Int(mouseCoords.x, mouseCoords.y, 0);
        if (Input.GetMouseButtonDown(0) && walls.GetTile(tileCoords) == null && floor.GetTile(tileCoords) == null)
        {
            floor.SetTile(tileCoords, selectedTile);
        }
        mouseCoords += new Vector2Int((int)gooPlane.transform.localScale.x, (int)gooPlane.transform.localScale.y);
        int xCoord = (int)(3200 * (mouseCoords.x / gooPlane.transform.localScale.x));
        int yCoord = (int)(3200 * (mouseCoords.y / gooPlane.transform.localScale.y)) + 1;
        List<Vector2Int> gooCoords = new List<Vector2Int>();
        if (selectedTiles[0].x != xCoord || selectedTiles[0].y != yCoord)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    gooCoords.Add(new Vector2Int(xCoord + i, yCoord + j));
                }
            }
        }
        if (gooCoords.Count > 0)
        {
            Vector2 screenCoords = Camera.main.WorldToScreenPoint(mouseCoords - new Vector2(gooPlane.transform.localScale.x, gooPlane.transform.localScale.y)) +
                new Vector3(spriteImage.GetComponent<RectTransform>().rect.width, spriteImage.GetComponent<RectTransform>().rect.height, 0);
            spriteImage.transform.position = screenCoords;
            selectedTiles = gooCoords;
        }
    }

    private void DeleteSelection()
    {
        Vector2 mouseCoordsFloat = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
        Vector2Int mouseCoords = new Vector2Int((int)mouseCoordsFloat.x, (int)mouseCoordsFloat.y);
        Vector3Int tileCoords = new Vector3Int(mouseCoords.x, mouseCoords.y, 0);
        if (Input.GetMouseButtonDown(0) && (walls.GetTile(tileCoords) != null || floor.GetTile(tileCoords) != null))
        {
            cs.SetTypeOfManyTiles(selectedTiles, GridTileType.BLANK);
            walls.SetTile(tileCoords, null);
            cover.SetTile(tileCoords, null);
            floor.SetTile(tileCoords, null);
            if (staticObjects.ContainsKey(mouseCoords))
            {
                staticObjects.Remove(mouseCoords);
            }
        }
        mouseCoords += new Vector2Int(200, 200);
        int xCoord = (int)(3200 * (mouseCoords.x / 400.0f));
        int yCoord = (int)(3200 * (mouseCoords.y / 400.0f)) + 1;
        List<Vector2Int> gooCoords = new List<Vector2Int>();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                gooCoords.Add(new Vector2Int(xCoord + i, yCoord + j));
            }
        }
        Vector2 screenCoords = Camera.main.WorldToScreenPoint(mouseCoords - new Vector2(200, 200)) +
                new Vector3(spriteImage.GetComponent<RectTransform>().rect.width, spriteImage.GetComponent<RectTransform>().rect.height, 0);
        spriteImage.transform.position = screenCoords;
        selectedTiles = gooCoords;
    }

    private void PlacePlayer()
    {
        spriteImage.transform.position = Input.mousePosition;
        Vector2 mouseCoordsFloat = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
        if (Input.GetMouseButtonDown(0))
        {
            player.transform.position = mouseCoordsFloat;
        }
    }

    public void SelectTool()
    {
        switch (toolSelection.value)
        {
            case 0:
                tileDropdown.SetActive(false);
                spriteImage.GetComponent<Image>().sprite = null;
                break;
            case 1:
                tileDropdown.SetActive(true);
                break;
            case 2:
                tileDropdown.SetActive(false);
                spriteImage.GetComponent<Image>().sprite = toolSelection.options[2].image;
                break;
            case 3:
                tileDropdown.SetActive(false);
                spriteImage.GetComponent<Image>().sprite = toolSelection.options[3].image;
                break;
        }
    }
}
