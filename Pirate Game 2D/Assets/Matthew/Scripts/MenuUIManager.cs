using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    [SerializeField] Image _blackScreen;
    bool _isLoadingLevel = false;
    bool _isLoadingMenu = true;
    float _loadTimer = 1;

    Color _fadeColour;

    private void Start()
    {
        _fadeColour = Color.black;
        _blackScreen.color = _fadeColour;
    }
    private void Update()
    {
        if (_isLoadingMenu)
        {
            LoadMenu();
            if (_loadTimer <= 0) _isLoadingMenu = false;
            return;
        }
        if (_isLoadingLevel)
        {
            LoadLevel();
            if (_loadTimer >= 1)
            {
                SceneManager.LoadScene(1);
            }
            return;
        }
    }
    public void OnPlayClicked()
    {
        _isLoadingLevel = true;
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }


    void LoadLevel()
    {
        FadeBlackScreen(Time.deltaTime);
    }

    void LoadMenu()
    {
        FadeBlackScreen(-Time.deltaTime);
    }

    void FadeBlackScreen(float change)
    {
        _loadTimer += change;
        _fadeColour.a = _loadTimer;
        _blackScreen.color = _fadeColour;
    }
}
