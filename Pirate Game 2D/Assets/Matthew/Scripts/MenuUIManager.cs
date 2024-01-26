using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    [SerializeField] Image _blackScreen;
    [SerializeField] Image _gooOne;
    [SerializeField] Image _gooTwo;
    [SerializeField] Image _gooThree;
    [SerializeField] GameObject _book;
    bool _isLoadingLevel = false;
    bool _isLoadingMenu = true;
    bool _isFlippingPages = false;
    bool _isGoingToDisplayControls = false;
    bool _isGoingToMainMenu = false;
    float _loadTimer = 1;
    [SerializeField] AudioSource _audioSource;

    Color _fadeColour;
    Color _gooFade;

    private void Start()
    {
        _fadeColour = Color.black;
        _gooFade = _gooOne.color;
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
                SceneManager.LoadScene(2);
            }
            return;
        }
        if (_isFlippingPages)
        {
            if (_book.GetComponent<BookAnimatorAlert>().isFinished)
            {
                if (_isGoingToMainMenu)
                {
                    transform.GetChild(1).gameObject.SetActive(true);
                    _isGoingToMainMenu = false;
                    _book.transform.Rotate(new Vector3(0, 180, 0));
                }
                else if (_isGoingToDisplayControls)
                {
                    transform.GetChild(2).gameObject.SetActive(true);
                    _isGoingToDisplayControls = false;
                }
                _isFlippingPages = false;
            }
        }
    }
    public void OnPlayClicked()
    {
        _isLoadingLevel = true;
    }

    public void OnControlsClicked()
    {
        FlipPages();
        _isGoingToDisplayControls = true;
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }

    public void OnBackClicked()
    {
        FlipPages();
        _book.transform.Rotate(new Vector3(0, 180, 0));
        _isGoingToMainMenu = true;
    }

    void FlipPages()
    {
        foreach(Transform child in transform)
        {
            if (child.gameObject.name == "Image") continue;
            child.gameObject.SetActive(false);
        }
        _isFlippingPages = true;
        _audioSource.Play();
        _book.GetComponent<Animator>().SetBool("_isFlipping", true);
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
        _gooFade.a = (1 - _loadTimer) / 2;
        _blackScreen.color = _fadeColour;
        _gooOne.color = _gooFade;
        _gooTwo.color = _gooFade;
        _gooThree.color = _gooFade;
    }
}
