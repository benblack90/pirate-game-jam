using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;


public enum GridChannel
{
    TYPE,TEMP,GOOAGE,TARGET_TEMP
}

public enum GridTileType
{
    BLANK,GOO_SPREADABLE,GOO_UNSPREADABLE,STATIC,MAX_TYPE
}

public class PracticeComputeScript : MonoBehaviour
{
    public ComputeShader cs;
    public ComputeShader entropyShader;
    public RenderTexture renderTexture;
    public Texture2D texCopy;
    public Material gooPlaneMaterial;
    int xSize = 2560;
    int ySize = 2560;

    void Start()
    {
        StartCoroutine(UpdateGoo());
        StartCoroutine(UpdateEntropy());
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.H))
        {
            for (int i = 500; i < 700; i++)
            {
                for (int j = 900; j < 1000; j++)
                {
                    WriteToGooTile(i, j, GridChannel.TEMP, 255);
                }
            }
            SendTexToGPU();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            for (int i = 600; i < 700; i++)
            {
                for (int j = 900; j < 1000; j++)
                {
                    WriteToGooTile(i, j, GridChannel.TEMP, 0);
                }
            }
            SendTexToGPU();
        }
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
        DoImportantBullshit();
        WriteToGooTile(700, 1100, GridChannel.TYPE, 1);
        WriteToGooTile(700, 1100, GridChannel.TEMP, 255);
        WriteToGooTile(700, 1100, GridChannel.GOOAGE, 0);
        WriteToGooTile(700, 1100, GridChannel.TARGET_TEMP, 255);

        WriteToGooTile(700, 1000, GridChannel.TYPE, 1);
        WriteToGooTile(700, 1000, GridChannel.TEMP, 20);
        WriteToGooTile(700, 1000, GridChannel.GOOAGE, 0);
        WriteToGooTile(700, 1000, GridChannel.TARGET_TEMP, 20);


        /*        WriteToGooTile(2000, 2000, GridChannel.TYPE, 1);
                WriteToGooTile(2000, 2000, GridChannel.TEMP, 100);
                WriteToGooTile(2000, 2000, GridChannel.GOOAGE, 0);
                WriteToGooTile(2000, 2000, GridChannel.TARGET_TEMP, 255);

                WriteToGooTile(1000, 1000, GridChannel.TYPE, 1);
                WriteToGooTile(1000, 1000, GridChannel.TEMP, 100);
                WriteToGooTile(1000, 1000, GridChannel.GOOAGE, 0);
                WriteToGooTile(1000, 1000, GridChannel.TARGET_TEMP, 0);*/

        for (int i = 701; i < 760; i++)
        {
            for(int j = 1101; j < 1190; j++)
            {
                WriteToGooTile(i, j, GridChannel.TYPE, 3);
            }
        }
        SendTexToGPU();

        Debug.Log(GetPixelFromGPU(2000, 2000));
        
        WaitForSeconds wfs = new WaitForSeconds(0.03f);
        cs.SetInt("aspectX", xSize);
        cs.SetInt("aspectY", ySize);
       
        while (true)
        {

            cs.SetTexture(0, "Result", renderTexture);
            cs.Dispatch(0, renderTexture.width / 16, renderTexture.height / 16, 1);
            GetGooDataFromGPU();
            yield return wfs;
        }
    }

    IEnumerator UpdateEntropy()
    {
        WaitForSeconds wfs = new WaitForSeconds(0.2f);
        entropyShader.SetInt("aspectX", xSize);
        entropyShader.SetInt("aspectY", ySize);

        while (true)
        {
            entropyShader.SetTexture(0, "Result", renderTexture);
            entropyShader.Dispatch(0, renderTexture.width / 16, renderTexture.height / 16, 1);
            GetGooDataFromGPU();
            yield return wfs;
        }
    }

    private void DoImportantBullshit()
    {
        RenderTexture.active = renderTexture;
        texCopy.ReadPixels(new Rect(0, 0, xSize, ySize), 0, 0, false);
        RenderTexture.active = null;
    }
    private NativeArray<Color32> GetGooDataFromGPU()
    {
        RenderTexture.active = renderTexture;
        texCopy.ReadPixels(new Rect(0, 0, xSize, ySize), 0, 0, false);
        RenderTexture.active = null;
        return texCopy.GetPixelData<Color32>(0);
    }

    private Color32 GetPixelFromGPU(int x,int y)
    {
        NativeArray<Color32> data = GetGooDataFromGPU();
        return data[xSize * y + x];
    }

    ///<summary>
    /// x and y are DIRECT int coordinates, writes to texture CPU side only
    /// RETURNS: true if successful, false if not
    ///</summary>
    public bool WriteToGooTile(int x, int y,GridChannel targetChannel, float value)
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
            case GridChannel.TARGET_TEMP:
                {
                    break;
                }

        }
        currentTile[(int)targetChannel] = (byte)value;
        texCopy.SetPixel(x, y, currentTile);
        //Debug.Log(GetTileValue(x, y, targetChannel));
        return true;
    }

    public float GetTileValue(int x, int y, GridChannel targetChannel)
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