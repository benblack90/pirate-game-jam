using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] string _name;
    public delegate void OnGenericCollectable(string name);
    public static event OnGenericCollectable onGenericCollectable;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            OnPickup();
        }
    }
    public string GetName()
    {
        return _name;
    }
    virtual protected void OnPickup()
    {
        onGenericCollectable?.Invoke(_name);
        gameObject.SetActive(false);
    }
}
