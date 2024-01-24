using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class BaseCoverTile
{

    [Serializable]
    public class Data
    {
        public TileBase BaseTile;
        public TileBase CoverTile;
    }
}
