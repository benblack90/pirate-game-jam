using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticDestructable : MonoBehaviour
{
    float hitPoints;
    Vector2Int graphicalPos;
    Vector2Int gooPos;
    int graphicsToGooRatio;
    int sideLength;
    bool onFire;
    GameObject destructModel;
    GameObject currentModel;

    public StaticDestructable(float hitPoints, Vector2Int graphicalPos, Vector2Int gooPos, int sideLength, GameObject destructModel, GameObject currentModel)
    {
        this.hitPoints = hitPoints;
        this.graphicalPos = graphicalPos;
        this.gooPos = gooPos;
        this.graphicsToGooRatio = 4;
        this.sideLength = sideLength;
        this.onFire = false;
        this.destructModel = destructModel;
        this.currentModel = currentModel;
    }



    // Start is called before the first frame update
    void Start()
    {
        //current specs say 32 x 32 pixel square graphics, with 16 x 16 goo grid
        /*sideLength = 32;
        graphicsToGooRatio = 2;
        gooPos = graphicalPos / graphicsToGooRatio;
        hitPoints = 100;*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Damage(float damage)
    {
        if (damage < 0) return;
        hitPoints -= damage;
    }

    public void CheckFireDamage()
    {
        if(onFire) hitPoints -= 1.0f * Time.deltaTime;    

        if(hitPoints <= 0) SwapToDestroyedModel();
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
}
