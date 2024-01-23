using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnPickup();
    }

    virtual protected void OnPickup()
    {

    }
}
