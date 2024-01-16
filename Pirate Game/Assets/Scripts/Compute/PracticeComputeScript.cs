using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

[System.Serializable]
public struct Tile
{
    public int type;
    public int temp;
    public int timer;
}

enum GridChannel
{
    TYPE,TEMP,GOOAGE,UNUSED
}

enum GridTileType
{
    BLANK,GOO_SPREADABLE,GOO_UNSPREADABLE,STATIC,MAX_TYPE
}

public class PracticeComputeScript : MonoBehaviour
{
    public ComputeShader cs;
    public RenderTexture renderTexture;
    public Texture2D texCopy;
    public Material gooPlaneMaterial;
    public Tile[] data;
    int xSize = 128;
    int ySize = 128;

    void Start()
    {
        StartCoroutine(UpdateGoo());
    }

    private void OnEnable()
    {
        texCopy = new Texture2D(xSize, ySize);
        renderTexture = new RenderTexture(xSize, ySize, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.autoGenerateMips = false;
        renderTexture.Create();
        renderTexture.filterMode = FilterMode.Point;
        gooPlaneMaterial.mainTexture = renderTexture;
    }

    IEnumerator UpdateGoo()
    {
        WaitForSeconds wfs = new WaitForSeconds(1f);
        while (true)
        {

            cs.SetTexture(0, "Result", renderTexture);
            cs.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
            GetGooDataFromGPU(renderTexture);
            yield return wfs;
            WriteToGooTile(5, 5, GridChannel.TEMP, 128);
            SendTexToGPU();
            Debug.Log(GetPixelFromGPU(5, 5,renderTexture));
        }
    }

    private NativeArray<Color32> GetGooDataFromGPU(RenderTexture renderTex)
    {
        RenderTexture.active = renderTex;
        texCopy.ReadPixels(new Rect(0, 0, xSize, ySize), 0, 0, false);
        texCopy.Apply();
        RenderTexture.active = null;
        return texCopy.GetPixelData<Color32>(0);
    }

    private Color32 GetPixelFromGPU(int x,int y, RenderTexture renderTex)
    {
        NativeArray<Color32> data = GetGooDataFromGPU(renderTex);
        return data[xSize * y + x];
    }

    ///<summary>
    /// x and y are DIRECT int coordinates, writes to texture CPU side only
    /// RETURNS: true if successful, false if not
    ///</summary>
    private bool WriteToGooTile(int x, int y,GridChannel targetChannel, float value)
    {
        if (x < 0 || y < 0 || x > xSize || y > ySize) return false;

        Color32 currentTile = texCopy.GetPixel(x, y);
        switch(targetChannel)
        {
            case GridChannel.TYPE:
                {
                    if (value > (int)GridTileType.MAX_TYPE || value < 0) return false;
                    break;
                }
            case GridChannel.TEMP:
                {
                    break;
                }
            case GridChannel.GOOAGE:
                {
                    break;
                }
            case GridChannel.UNUSED:
                {
                    break;
                }

        }
        currentTile[(int)targetChannel] = (byte)value;
        texCopy.SetPixel(x, y, currentTile);
        Debug.Log(GetTileValue(x, y, targetChannel));
        return true;
    }

    private float GetTileValue(int x, int y, GridChannel targetChannel)
    {
        Color32 values = texCopy.GetPixel(x, y);
        return values[(int)targetChannel];
    }

    private void SendTexToGPU()
    {
        texCopy.Apply();
        Graphics.Blit(texCopy, renderTexture);
    }

    /*    void InitialiseTiles()
        {
            data = new Tile[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    data[y * size + x].type = 0;
                    data[y * size + x].temp = 100;
                    data[y * size + x].timer = 100;
                }
            }

            data[size/2 * size + size / 2].type = 1;
            data[size / 2 * size + size / 2].temp = 100;
            data[size / 2 * size + size / 2].timer = 100;
        }

        void TilesToGPU()
        {
            buffer.SetData(data);
            cs.SetBuffer(0, "tiles", buffer);
            cs.Dispatch(0, data.Length/8, 1, 1);
        }

        void GetDataFromGPU()
        {
            buffer.GetData(data);
        }*/
}


/*
 *  This is how to read from the texture!
 *  
 *          NativeArray<Color32> array = GetGooDataFromGPU(renderTexture);
            foreach (Color32 col in array)
            {
                Debug.Log(col);
            }
 * */