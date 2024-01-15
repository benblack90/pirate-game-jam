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

public class PracticeComputeScript : MonoBehaviour
{
    public ComputeShader cs;
    public RenderTexture renderTexture;
    public Texture2D texCopy;
    public Material gooPlaneMaterial;
    public Tile[] data;
    ComputeBuffer buffer;
    int size = 128;

    void Start()
    {
        StartCoroutine(UpdateGoo());
    }

    private void OnEnable()
    {
        texCopy = new Texture2D(size, size);
        renderTexture = new RenderTexture(size, size,0);
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
            NativeArray<Color32> array = GetGooDataFromGPU(renderTexture);
            foreach (Color32 col in array)
            {
                Debug.Log(col);
            }
            yield return wfs;
        }
    }

    private NativeArray<Color32> GetGooDataFromGPU(RenderTexture renderTex)
    {
        RenderTexture.active = renderTex;
        texCopy.ReadPixels(new Rect(0, 0, size, size), 0, 0, false);
        texCopy.Apply();
        RenderTexture.active = null;
        return texCopy.GetPixelData<Color32>(0);
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
