using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseGame : MonoBehaviour
{
    public GameObject player;
    public GameObject audioManager;
    public Image blackScreen;
    private bool isEnabled = false;
    private bool isLoadingLevel = true;
    private bool isRestartingLevel = false;
    private bool isExitingLevel = false;
    float loadTimer = 1;

    Color fadeColour = Color.black;

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
        Time.timeScale = 0.0f;
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
