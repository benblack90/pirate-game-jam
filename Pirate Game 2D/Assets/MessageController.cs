using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MessageController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI quote;
    [SerializeField] TextMeshProUGUI gooCouncil;
    [SerializeField] AudioSource windSound;
    float sceneTime;
    float quoteTime;

    // Start is called before the first frame update
    void Start()
    {
        sceneTime = 0.0f;
        quoteTime = 2.0f;
        quote.color = new Color(1f, 1f, 1f, 0);
        gooCouncil.color = new Color(1f, 1f, 1f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        sceneTime += Time.deltaTime;
        if (sceneTime < 1.0f) windSound.volume = sceneTime;
        quote.color = new Color(1f, 1f, 1f, sceneTime);
        if(sceneTime > quoteTime) gooCouncil.color = new Color(1f, 1f, 1f, (sceneTime - quoteTime) * 0.5f);
        if(sceneTime > 6.0f) SceneManager.LoadScene(1);
    }
}
