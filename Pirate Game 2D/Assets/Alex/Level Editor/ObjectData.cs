using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectData
{
    public int objectType;
    public Vector2Int gooPos;
    public Vector2 graphicsPos;

    public ObjectData(int objectType, Vector2Int gooPos, Vector2 graphicsPos)
    {
        this.objectType = objectType;
        this.gooPos = gooPos;
        this.graphicsPos = graphicsPos;
    }
}
