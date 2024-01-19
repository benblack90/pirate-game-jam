using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectDestroyedDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mostRecentObjectDisplay;
    [SerializeField] TextMeshProUGUI middleRecentObjectDisplay;
    [SerializeField] TextMeshProUGUI lastObjectDisplay;
    [SerializeField] Animator animator;
    List<string> scoreMessages = new List<string>();

    private void OnEnable()
    {
        mostRecentObjectDisplay.text = string.Empty;
        middleRecentObjectDisplay.text = string.Empty;
        lastObjectDisplay.text = string.Empty;
    }
    public void AddObject(ObjectScorePair pair)
    {
        string newMessage = "Destroyed " + pair.name + "! " + pair.points + "pts!";
        animator.SetTrigger("ActivateTrigger");
        switch (scoreMessages.Count)
        {
            case 0:
                {
                    scoreMessages.Add(newMessage);
                    mostRecentObjectDisplay.text = newMessage;
                    break;
                }
            case 1:
                {
                    scoreMessages.Add(newMessage);
                    middleRecentObjectDisplay.text = mostRecentObjectDisplay.text;
                    mostRecentObjectDisplay.text = newMessage;
                    break;
                }
            case 2:
                {
                    scoreMessages.Add(newMessage);
                    lastObjectDisplay.text = middleRecentObjectDisplay.text;
                    middleRecentObjectDisplay.text = mostRecentObjectDisplay.text;
                    mostRecentObjectDisplay.text = newMessage;
                    break;
                }
            case 3:
                {
                    scoreMessages.Add(newMessage);
                    scoreMessages.RemoveAt(0);
                    lastObjectDisplay.text = middleRecentObjectDisplay.text;
                    middleRecentObjectDisplay.text = mostRecentObjectDisplay.text;
                    mostRecentObjectDisplay.text = newMessage;
                    break;
                }
        }
    }
}
