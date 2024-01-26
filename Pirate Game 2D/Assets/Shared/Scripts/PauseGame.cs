using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseGame : MonoBehaviour
{
    // Update is called once per frame
    public GameObject player;
    public GameObject audioManager;
    public Image blackScreen;

    private bool isLoadingLevel = true;
    private bool isRestartingLevel = false;
    private bool isExitingLevel = false;
    float loadTimer = 1;

    Color fadeColour = Color.black;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && Time.timeScale == 1.0f)
        {
            player.GetComponent<AudioSource>().Pause();
            foreach (AudioSource source in audioManager.GetComponentsInChildren<AudioSource>())
            {
                if (source.gameObject.name == "Background Music") continue;
                if (source.isPlaying)
                {
                    source.Pause();
                }
            }
            foreach(Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
            Time.timeScale = 0.0f;
        }
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
                else if(isRestartingLevel) SceneManager.LoadScene(1);
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

    public void Resume()
    {
        player.GetComponent<AudioSource>().UnPause();
        foreach (AudioSource source in audioManager.GetComponentsInChildren<AudioSource>())
        {
            if (source.gameObject.name == "Background Music") continue;
            if (source.isPlaying)
            {
                source.UnPause();
            }
        }
        TurnOffPauseMenu();
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
