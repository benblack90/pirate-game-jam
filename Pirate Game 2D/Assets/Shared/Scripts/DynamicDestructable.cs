using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static StaticDestructable;

public class DynamicDestructable : MonoBehaviour
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
    [SerializeField] float maxHp;
    public bool active;

    public delegate void OnDynamicDestroyed(ObjectScorePair pair, Vector2Int graphicalPos);
    public static event OnDynamicDestroyed onDynamicDestroyed; //delegate called when a destructable is destroyed

    DynamicDestructable() { }

    public void Init(Level l)
    {
        bottomLeft.x = (int)t.position.x - width / 2;
        bottomLeft.y = (int)t.position.y - height / 2;
        gooPos = bottomLeft * 8;
        this.l = l;
        this.hitPoints = maxHp;
        this.active = true;
    }

    public Vector2Int GetGooPos()
    {
        return gooPos;
    }

    public int GetWidth()
    {
        return width;
    }

    public void GooDamage(float damage)
    {
        if (damage <= 0) return;
        
        damage *= Time.deltaTime;
        hitPoints -= damage;
        float percentDmg = hitPoints / maxHp;
        sp.color = new Color(percentDmg,percentDmg, percentDmg, sp.color.a);
        if (hitPoints <= 0) ObjectDestroy();
    }

    void ObjectDestroy()
    {
        ObjectScorePair pair = new ObjectScorePair();
        pair.name = name;
        pair.points = points;
        sp.sprite = null;        
        onDynamicDestroyed?.Invoke(pair, bottomLeft);
        active = false;
    }
}
