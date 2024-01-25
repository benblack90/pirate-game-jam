using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static HashSet<string> inventory = new HashSet<string>();

    private void OnEnable()
    {
        Collectable.onGenericCollectable += AddToInventory;
    }

    private void OnDisable()
    {
        Collectable.onGenericCollectable -= AddToInventory;
    }

    public static bool HasItem(string name)
    {
        return inventory.Contains(name);
    }

    void AddToInventory(string name)
    {
        inventory.Add(name);
        //Debug.Log(name);
    }
}
