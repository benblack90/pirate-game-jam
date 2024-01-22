using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public struct ObjectScorePair
{
    public string name;
    public int points;
}
public class UIManager : MonoBehaviour
{
    [SerializeField] Image healthBar;
    [SerializeField] TextMeshProUGUI scoreDisplay;
    [SerializeField] TextMeshProUGUI timeDisplay;
    [SerializeField] ObjectDestroyedDisplay objectDestroyedDisplay;

    int scoreCache = 0;

    private void OnEnable()
    {
        StaticDestructable.onDestructableDestroyed += ObjectDestroyed;
        //subscribe to some events
    }
    private void OnDisable()
    {

        //unsubscribe from events
        StaticDestructable.onDestructableDestroyed -= ObjectDestroyed;

    }
    private void Update()
    {
/*        if (Input.GetKeyDown(KeyCode.Space))
        {
            ObjectScorePair pair = new ObjectScorePair();
            pair.name = "TEST";
            pair.points = Random.Range(0,500);
            ObjectDestroyed(pair);
        }*/
    }

    void ObjectDestroyed(ObjectScorePair pair, Vector2Int graphicalPos, Vector2Int topRight)
    {
        objectDestroyedDisplay.AddObject(pair);
        scoreCache += pair.points;
        SetScore(scoreCache);
        //maybe particle effects?
    }

    void SetScore(int score)
    {
        scoreCache = score;
        scoreDisplay.text = "SCORE: " + score;
    }

    void SetTimer(int time)
    {
        timeDisplay.text = "TIME: " + time;
    }

    void SetHealth(float healthPercentage)
    {
        healthBar.rectTransform.localScale = new Vector3(healthPercentage,1,1);
    }
}
