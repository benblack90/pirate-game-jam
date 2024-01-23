using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPickup : Collectable
{
    [SerializeField] int _pointsToGive;

    public delegate void OnPointPickup(int points);
    public static event OnPointPickup onPointPickup; //delegate called when a destructable is destroyed

    protected override void OnPickup()
    {
        onPointPickup?.Invoke(_pointsToGive);
        Destroy(gameObject);
    }
}
