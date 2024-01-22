using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using static StaticDestructable;

public class StaticDestructable
{
    int points;
    string objectName;
    float hitPoints;
    Vector2Int graphicalPos;
    Vector2Int gooPos;

    Vector2Int bottomLeftGridCoord;
    public Vector2Int topRightGridCoord;

    public PracticeComputeScript gooController;

    bool onFire;
    GameObject destructModel;
    GameObject currentModel;

    public StaticDestructable(float hitPoints, Vector2Int graphicalPos, GameObject destructModel, GameObject currentModel)
    {
        this.hitPoints = hitPoints;
        this.graphicalPos = graphicalPos;
        gooPos = graphicalPos * 8;
        onFire = false;
        this.destructModel = destructModel;
        this.currentModel = currentModel;
    }



    public delegate void OnDestructableDestroyed(ObjectScorePair pair, Vector2Int graphicalPos, Vector2Int topRight);
    public static event OnDestructableDestroyed onDestructableDestroyed; //delegate called when a destructable is destroyed

    public void Damage(float damage)
    {
        if (damage <= 0) return;
        hitPoints -= damage;
        Debug.Log(hitPoints);
        if (hitPoints <= 0) ObjectDestroy();
    }

    public void IgnitionFromGooCheck(float gooTemp)
    {
        if (onFire) return;
        onFire = (gooTemp > 240) ? true : false;
    }

    public void CheckFireDamage()
    {
        if(onFire) hitPoints -= 1.0f * Time.deltaTime;

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
        SwapToDestroyedModel();
        ObjectScorePair pair = new ObjectScorePair();
        pair.name = objectName;
        pair.points = points;
        onDestructableDestroyed?.Invoke(pair, graphicalPos, topRightGridCoord);
    }


    void TestDelegate()
    {
        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            ObjectDestroy();
        }
    }
}
