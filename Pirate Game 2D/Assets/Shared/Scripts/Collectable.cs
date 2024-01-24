using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] string _name;
    public delegate void OnGenericCollectable(string name);
    public static event OnGenericCollectable onGenericCollectable;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnPickup();
    }

    virtual protected void OnPickup()
    {
        onGenericCollectable?.Invoke(_name);
        gameObject.SetActive(false);
    }
}
