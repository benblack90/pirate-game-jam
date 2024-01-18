using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticDestructable : MonoBehaviour
{
    float hitPoints;
    Vector2 graphicalPos;
    Vector2 gooPos;
    int sideLength;
    bool onFire;
    public GameObject destructModel;
    public GameObject currentModel;

    // Start is called before the first frame update
    void Start()
    {
        hitPoints = 100;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator CheckGooDamage()
    {
        //am I next to hot goo?
            //take damage
            //ignite if hot enough

        
        yield return new WaitForSeconds(0.03f);
    }

    void CheckFireDamage()
    {
        if (onFire)
        {
            hitPoints -= 1.0f * Time.deltaTime;
        }
    }

    void SwapToDestroyedModel()
    {
        currentModel = destructModel;
        //inform the renderer, somehow!
    }
}
