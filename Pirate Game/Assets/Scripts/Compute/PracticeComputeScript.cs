using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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
        renderTexture = new RenderTexture(size, size, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        renderTexture.filterMode = FilterMode.Point;
        gooPlaneMaterial.mainTexture = renderTexture;
        InitialiseTiles();
        int dataSize = sizeof(int) * 3;
        buffer = new ComputeBuffer(data.Length, dataSize);
    }
    void OnDisable()
    {
        buffer.Release();
    }

    IEnumerator UpdateGoo()
    {
        WaitForSeconds wfs = new WaitForSeconds(1f);
        while (true)
        {
            cs.SetTexture(0, "Result", renderTexture);
            TilesToGPU();
            GetDataFromGPU();
            yield return wfs;
        }
    }

    void InitialiseTiles()
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
    }
}
