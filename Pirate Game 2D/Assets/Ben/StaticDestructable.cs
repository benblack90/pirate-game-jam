using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using static StaticDestructable;

public class StaticDestructable : MonoBehaviour
{
    [SerializeField] int points;
    [SerializeField] string name;
    float hitPoints;
    Vector2Int graphicalPos;
    Vector2Int gooPos;
    int graphicsToGooRatio;
    int sideLength;
    bool onFire;
    public GameObject destructModel;
    public GameObject currentModel;

    public delegate void OnDestructableDestroyed(ObjectScorePair pair);
    public static event OnDestructableDestroyed onDestructableDestroyed; //delegate called when a destructable is destroyed

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

    public void Damage(float damage)
    {
        if (damage < 0) return;
        hitPoints -= damage;
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
        pair.name = name;
        pair.points = points;
        onDestructableDestroyed?.Invoke(pair);
    }

    void TestDelegate()
    {
        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            ObjectDestroy();
        }
    }
}
