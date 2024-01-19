using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolSelector : MonoBehaviour
{
    public GameObject tileDropdown;
    public TMPro.TMP_Dropdown toolSelection;
    public LevelEditorComputeScript cs;
    public GameObject spriteImage;
    private List<Vector2Int> tiles;
    private List<Vector2Int> selectedTiles;

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
        selectedTiles = new List<Vector2Int> { new Vector2Int(0, 0) };
    }

    // Update is called once per frame
    void Update()
    {
        switch (toolSelection.value)
        {
            case 1:
                PlaceSelection();
                break;
            case 2:

                break;
        }
    }

    private void PlaceSelection()
    {
        if(Input.GetMouseButtonDown(0))
        {
            cs.SetTypeOfManyTiles(selectedTiles, GridTileType.STATIC);
        }
        Vector2 mouseCoords = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
        mouseCoords += new Vector2(50, 50);
        int xCoord = (int)(2560 * (mouseCoords.x / 100));
        int yCoord = (int)(2560 * (mouseCoords.y / 100));
        List<Vector2Int> gooCoords = new List<Vector2Int>();
        switch (tileDropdown.GetComponent<TMPro.TMP_Dropdown>().value)
        {
            case 0:
                return;
            case 1:
                spriteImage.GetComponent<Image>().sprite = tileDropdown.GetComponent<TMPro.TMP_Dropdown>().options[1].image;
                if (selectedTiles[0].x != xCoord && selectedTiles[0].y != yCoord)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        for (int j = 0; j < 32; j++)
                        {
                            gooCoords.Add(new Vector2Int(xCoord + i, yCoord - j));
                        }
                    }
                }
                break;
        }
        
        if (gooCoords.Count > 0 && cs.IsAreaFree(gooCoords))
        {
            selectedTiles = gooCoords;
            Vector2 screenCoords = Camera.main.WorldToScreenPoint(mouseCoords - new Vector2(50, 50)) - 
                new Vector3(-spriteImage.GetComponent<RectTransform>().rect.width * 3 / 4, spriteImage.GetComponent<RectTransform>().rect.height);
            spriteImage.transform.position = screenCoords;
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