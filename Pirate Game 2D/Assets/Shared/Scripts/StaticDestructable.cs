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

    public PracticeComputeScript gooController;

    public bool onFire;
    GameObject destructModel;
    GameObject currentModel;

    public StaticDestructable(float hitPoints, Vector2Int graphicalPos, GameObject destructModel, GameObject currentModel, Level level)
    {
        this.hitPoints = hitPoints;
        this.graphicalPos = graphicalPos;
        gooPos = graphicalPos * 8;
        onFire = false;
        this.destructModel = destructModel;
        this.currentModel = currentModel;
        this.level = level;
        this.timeOnFire = 0.0f;
    }



    public delegate void OnDestructableDestroyed(ObjectScorePair pair, Vector2Int graphicalPos);
    public static event OnDestructableDestroyed onDestructableDestroyed; //delegate called when a destructable is destroyed

    public void Damage(float damage)
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
            hitPoints -= 1.0f;
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

    void SwapToDestroyedModel()
    {
        currentModel = destructModel;
        //inform the renderer, somehow!
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
        onDestructableDestroyed?.Invoke(pair, graphicalPos);
    }


    void TestDelegate()
    {
        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            ObjectDestroy();
        }
    }
}
