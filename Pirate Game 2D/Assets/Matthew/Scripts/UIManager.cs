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
    float test = 1;
    float healthBarWidth;

    private void OnEnable()
    {
        StaticDestructable.onStaticDestroyed += ObjectDestroyed;
        PlayerValuesManager.onHealthChanged += SetHealth;
        PlayerValuesManager.onPointsChanged += SetScore;
        DynamicDestructable.onDynamicDestroyed += ObjectDestroyed;
        Level.onTimerChange += SetTimer;
        healthBarWidth = healthBar.rectTransform.rect.width;
        //subscribe to some events
    }


    private void OnDisable()
    {

        //unsubscribe from events
        StaticDestructable.onStaticDestroyed -= ObjectDestroyed;
        PlayerValuesManager.onPointsChanged -= SetScore;
        PlayerValuesManager.onHealthChanged -= SetHealth;
        DynamicDestructable.onDynamicDestroyed -= ObjectDestroyed;
        Level.onTimerChange -= SetTimer;

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            test -= 0.1f;
            SetHealth(test);
        }
    }

    void ObjectDestroyed(ObjectScorePair pair, Vector2Int graphicalPos)
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

    public void SetTimer(int time)
    {
        timeDisplay.text = "TIME: " + time;
    }

    void SetHealth(float healthPercentage)
    {
        healthBar.rectTransform.sizeDelta = new Vector2(healthPercentage * healthBarWidth, healthBar.rectTransform.sizeDelta.y);
    }
}
