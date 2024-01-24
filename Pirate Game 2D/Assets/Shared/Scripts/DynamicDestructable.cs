using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static StaticDestructable;

public class DynamicDestructable
{
    [SerializeField] SpriteRenderer sp;
    [SerializeField] Transform t;
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] int points;
    [SerializeField] string name;
    List<GameObject> neighbours;
    Level l;
    Vector2Int bottomLeft;
    Vector2Int gooPos;
    float hitPoints;

    public delegate void OnDynamicDestroyed(ObjectScorePair pair, Vector2Int graphicalPos);
    public static event OnDynamicDestroyed onDynamicDestroyed; //delegate called when a destructable is destroyed

    DynamicDestructable(Level l, Vector2Int graphicalPos)
    {
        bottomLeft.x = (int) t.position.x - width / 2;
        bottomLeft.y = (int) t.position.y - height / 2;
        gooPos = bottomLeft * 8;
        this.l = l;
        this.hitPoints = 100.0f;
    }

    Vector2Int GetGooPos()
    {
        return gooPos;
    }

    public void GooDamage(float damage)
    {
        if (damage <= 0) return;
        hitPoints -= damage * 0.5f;
        sp.color = new Color(sp.color.r-damage, sp.color.g-damage, sp.color.b-damage, sp.color.a);
        if (hitPoints <= 0) ObjectDestroy();
    }

    void ObjectDestroy()
    {
        ObjectScorePair pair = new ObjectScorePair();
        pair.name = name;
        pair.points = points;
        onDynamicDestroyed?.Invoke(pair, bottomLeft);
    }
}
