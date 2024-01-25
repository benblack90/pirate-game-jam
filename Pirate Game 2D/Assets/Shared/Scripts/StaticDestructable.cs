using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using static StaticDestructable;
using System.Runtime.ExceptionServices;

public class StaticDestructable
{
    int points;
    string objectName;
    float hitPoints;
    Vector2Int graphicalPos;
    Vector2Int gooPos;
    Level level;
    int usingFireIndex;
    float timeOnFire;

    public GooController gooController;

    public bool onFire;

    public StaticDestructable(float hitPoints, Vector2Int graphicalPos, Level level)
    {
        this.hitPoints = hitPoints;
        this.graphicalPos = graphicalPos;
        gooPos = graphicalPos * 8;
        onFire = false;
        this.level = level;
        this.timeOnFire = 0.0f;
        this.points = 50;
    }



    public delegate void OnStaticDestroyed(ObjectScorePair pair, Vector2Int graphicalPos);
    public static event OnStaticDestroyed onStaticDestroyed; //delegate called when a destructable is destroyed

    public void GooDamage(float damage)
    {
        if (damage <= 0) return;
        hitPoints -= damage * 0.1f;
        if (hitPoints <= 0) ObjectDestroy();
    }

    public void IgnitionFromGooCheck(float gooTemp)
    {
        if (onFire) return;
        if(gooTemp > 240)
        {
            Ignite();
        }
    }

    public void IgniteFromAdjacency(int dist)
    {
        if (onFire) return;
        float chance = (dist > 1) ? 0.05f : 0.1f;
        if(chance > UnityEngine.Random.Range(0.0f, 1.0f))
        {
            Ignite();
        }        
    }

    void Ignite()
    {
        onFire = true;
        usingFireIndex = level.AddFireSpriteToLoc(graphicalPos);
    }

    public void CheckFireDamage()
    {
        if (onFire)
        {
            hitPoints -= 15.0f;
            timeOnFire += 1.0f;  
            if(timeOnFire > 5.0f)
            {
                timeOnFire = 0.0f;
                onFire = false;
                level.ExtinguishFire(usingFireIndex);
            }
        }
        if (hitPoints <= 0)
        {
            ObjectDestroy();
        }
    }



    public Vector2Int GetGooPos()
    {
        return gooPos;
    }


    void ObjectDestroy()
    {
        level.ExtinguishFire(usingFireIndex);
        ObjectScorePair pair = new ObjectScorePair();
        pair.name = objectName;
        pair.points = points;
        onStaticDestroyed?.Invoke(pair, graphicalPos);
    }
}
