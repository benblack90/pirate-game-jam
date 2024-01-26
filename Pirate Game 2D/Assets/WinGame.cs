using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class WinGame : MonoBehaviour
{
    public GameObject player;
    public GameObject audioManager;
    public Image blackScreen;
    public UIManager manager;
    public Level level;

    [Header("STATS")]
    public TextMeshProUGUI initialScore;
    public TextMeshProUGUI timeRemaining;
    public TextMeshProUGUI wallsDestroyed;
    public TextMeshProUGUI objectsDestroyed;
    public TextMeshProUGUI healthRemaining;
    public TextMeshProUGUI totalScore;

    private bool isEnabled = false;
    private bool isLoadingLevel = true;
    private bool isRestartingLevel = false;
    private bool isExitingLevel = false;
    float loadTimer = 1;

    Color fadeColour = Color.black;
    private int score = 0;
    private float time = 0;
    private int totalItemsDestroyed = 0;
    private int totalWallsDestroyed = 0;
    private int health;

    private int scoreFinal;

    public bool IsEnabled()
    {
        return isEnabled;
    }
    public void EnableScreen()
    {
        isEnabled = true;
        player.GetComponent<AudioSource>().Pause();

        
        foreach (AudioSource source in audioManager.GetComponentsInChildren<AudioSource>())
        {
            /*
            if (source.gameObject.name == "Background Music") continue;
            if (source.isPlaying)
            {
                source.Pause();
            }
            */
        }
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        StartCoroutine(TallyTime());
        Time.timeScale = 0.0f;
    }

    IEnumerator TallyTime()
    {
        score = manager.GetScore();
        time = level.GetTimer();
        health = player.GetComponent<PlayerValuesManager>().GetHealth();

        int multiplierTime = Mathf.RoundToInt(time) * 15;
        int multiplierWall = totalWallsDestroyed * 10;
        int multiplierObject = totalItemsDestroyed * 10;
        int multiplierHealth = health * 20;

        scoreFinal = score + multiplierTime + multiplierWall + multiplierObject + multiplierTime;


        initialScore.text = "End Score: " + score.ToString();
        timeRemaining.text = "Time Remaining: " + time.ToString() + " x 15 = " + multiplierTime.ToString();
        wallsDestroyed.text = "Total Walls Destroyed: " + totalWallsDestroyed.ToString() + " x 10 = " + multiplierWall.ToString();
        objectsDestroyed.text = "Total Objects Destroyed: " + totalItemsDestroyed.ToString() + " x 10 = " + multiplierObject.ToString();
        healthRemaining.text = "Health Remaining: " + health.ToString() + " x 20 = " + multiplierHealth.ToString();
        totalScore.text = "Total Score: " + scoreFinal.ToString() + "!";

        WaitForEndOfFrame wfs = new WaitForEndOfFrame();
        while (true)
        {
            yield return wfs;
        }
    }
    void Update()
    {
        if (isLoadingLevel)
        {
            LoadLevel();
            if (loadTimer <= 0) isLoadingLevel = false;
            return;
        }
        if (isExitingLevel || isRestartingLevel)
        {
            ExitLevel();
            Debug.Log("Exiting");
            if (loadTimer >= 1)
            {
                if (isExitingLevel) SceneManager.LoadScene(0);
                else if (isRestartingLevel) SceneManager.LoadScene(1);
            }
            return;
        }
    }
    public void AddItemDestroyed()
    {
        totalItemsDestroyed++;
    }
    public void AddWallDestroyed()
    {
        totalWallsDestroyed++;
    }
    void ExitLevel()
    {
        FadeBlackScreen(Time.deltaTime);
    }

    void LoadLevel()
    {
        FadeBlackScreen(-Time.deltaTime);
    }

    void FadeBlackScreen(float change)
    {
        loadTimer += change;
        fadeColour.a = loadTimer;
        blackScreen.color = fadeColour;
    }

    public void Restart()
    {
        TurnOffPauseMenu();
        isRestartingLevel = true;
    }

    public void Menu()
    {
        TurnOffPauseMenu();
        isExitingLevel = true;
    }

    private void TurnOffPauseMenu()
    {

        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "BlackScreen") continue;
            child.gameObject.SetActive(false);
        }
        Time.timeScale = 1.0f;
    }
}
