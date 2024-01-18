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
    public GameObject destructModel;
    public GameObject currentModel;

    // Start is called before the first frame update
    void Start()
    {
        //current specs say 32 x 32 pixel square graphics, with 16 x 16 goo grid
        sideLength = 32;
        graphicsToGooRatio = 2;
        gooPos = graphicalPos / graphicsToGooRatio;
        hitPoints = 100;
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
