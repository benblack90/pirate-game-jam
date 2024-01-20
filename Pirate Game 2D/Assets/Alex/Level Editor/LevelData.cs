using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public List<ObjectData> objectData;
    public Vector2 playerPos;

    public LevelData()
    {
        objectData = new List<ObjectData>();
        playerPos = new Vector2(0, 0);
    }
}
