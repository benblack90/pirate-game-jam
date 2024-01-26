using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerArea : MonoBehaviour
{
    [SerializeField] string triggerName;

    public delegate void OnPlayerEnterTrigger(string triggerName);
    public static event OnPlayerEnterTrigger onPlayerEnterTrigger;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            onPlayerEnterTrigger(triggerName);
        }
    }
}
