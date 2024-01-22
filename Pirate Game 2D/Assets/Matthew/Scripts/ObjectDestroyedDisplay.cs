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

    bool active = false;
    float fadeTimer = 2.0f;

    private void OnEnable()
    {
        mostRecentObjectDisplay.text = string.Empty;
        middleRecentObjectDisplay.text = string.Empty;
        lastObjectDisplay.text = string.Empty;
    }

    private void Update()
    {
        if (active)
        {
            fadeTimer -= Time.deltaTime;
            Color fadedColour = new Color(1, 1, 1, fadeTimer);
            mostRecentObjectDisplay.color = fadedColour;
            middleRecentObjectDisplay.color = fadedColour;
            lastObjectDisplay.color = fadedColour;

            if (fadeTimer <= 0) active = false;
        }
    }

    public void AddObject(ObjectScorePair pair)
    {
        string newMessage = "Destroyed " + pair.name + "! " + pair.points + "pts!";
        fadeTimer = 2.0f;
        active = true;
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
