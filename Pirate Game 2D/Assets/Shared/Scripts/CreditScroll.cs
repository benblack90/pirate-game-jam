using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditScroll : MonoBehaviour
{
    [SerializeField] GameObject thankYou;

    private float time = 0;
    private int scrollSpeed = 45;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            scrollSpeed = 150;
        }
        else
        {
            scrollSpeed = 45;
        }
        if((thankYou.transform as RectTransform).position.y <= 300)
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
