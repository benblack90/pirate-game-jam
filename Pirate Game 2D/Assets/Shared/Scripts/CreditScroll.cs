using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditScroll : MonoBehaviour
{
    [SerializeField] GameObject thankYou;
    [SerializeField] GameObject canvas;

    private float time = 0;
    [SerializeField] private int baseScrollSpeed = 45;
    private int scrollSpeed;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            scrollSpeed = baseScrollSpeed * 4;
        }
        else
        {
            scrollSpeed = baseScrollSpeed;
        }
        if((thankYou.transform as RectTransform).position.y <= (canvas.transform as RectTransform).position.y)
        {
            transform.position += new Vector3(0, Time.deltaTime * scrollSpeed, 0);
        }
        else
        {
            time += Time.deltaTime;
            if (time >= 6) thankYou.GetComponent<TMPro.TMP_Text>().color = new Color(1, 1, 1, 1 - (time - 6));
            if(time >= 7)
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}
