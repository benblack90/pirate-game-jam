using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolSelector : MonoBehaviour
{
    public GameObject tileDropdown;
    public TMPro.TMP_Dropdown toolSelection;
    public LevelEditorComputeScript cs;
    private List<Vector2Int> tiles;

    // Start is called before the first frame update
    void Start()
    {
        tiles = new List<Vector2Int>();
        for(int i = 0; i < 32; i++)
        {
            for(int j = 0; j < 32; j++)
            {
                tiles.Add(new Vector2Int(i + 1200, j + 1200));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (toolSelection.value)
        {
            case 1:
                Vector2 mouseCoords = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
                mouseCoords += new Vector2(50, 50);
                int xCoord = (int)(2560 * (mouseCoords.x / 100));
                int yCoord = (int)(2560 * (mouseCoords.y / 100));
                List<Vector2Int> gooCoords = new List<Vector2Int>();
                for(int i = 0; i < 1; i++)
                {
                    for(int j = 0; j < 1; j++)
                    {
                        gooCoords.Add(new Vector2Int(xCoord + i, yCoord + j));
                    }
                }
                //Debug.Log(mouseCoords + ", " + xCoord + ", " + yCoord);
                Debug.Log(cs.IsAreaFree(gooCoords) ? "Free" : "Taken");
                break;
            case 2:

                break;
        }
    }
    public void SelectTool()
    {
        switch (toolSelection.value)
        {
            case 0:
                tileDropdown.SetActive(false);
                cs.SetTypeOfManyTiles(tiles, GridTileType.BLANK);
                break;
            case 1:
                tileDropdown.SetActive(true);
                cs.SetTypeOfManyTiles(tiles, GridTileType.STATIC);
                break;
            case 2:
                tileDropdown.SetActive(false);
                cs.SetTypeOfManyTiles(tiles, GridTileType.BLANK);
                break;
        }
    }
}
