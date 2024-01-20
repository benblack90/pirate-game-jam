using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInputs : MonoBehaviour
{
    public GameObject runePage;
    public Transform canvas;
    public Vector2 canvasOffset;
    private GameObject activePage;

    private void OnEnable()
    {
        LineGenerator.OnRuneComplete += CastRune;
    }

    private void OnDisable()
    {
        LineGenerator.OnRuneComplete -= CastRune;
    }

    public void CastRune(RuneInfo info)
    {
        switch (info.type)
        {
            case RuneTypes.Ice:
                Debug.Log("ICE");
                break;
            case RuneTypes.Fire:
                Debug.Log("FIRE");
                break;
            case RuneTypes.Invalid:
                Debug.Log("INVALID");
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            GameObject page = Instantiate(runePage, canvas);
            Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
            
            page.transform.position = new Vector3(position.x + canvasOffset.x, position.y + canvasOffset.y, 0);
            if (page.GetComponent<RectTransform>().anchoredPosition.x > canvas.GetComponent<RectTransform>().rect.width - 200f)
            {
                page.GetComponent<RectTransform>().anchoredPosition = 
                    new Vector2(canvas.GetComponent<RectTransform>().rect.width - 200f, page.GetComponent<RectTransform>().anchoredPosition.y);
            }
            if (page.GetComponent<RectTransform>().anchoredPosition.y < -canvas.GetComponent<RectTransform>().rect.height + 175f)
            {
                page.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(page.GetComponent<RectTransform>().anchoredPosition.x, -canvas.GetComponent<RectTransform>().rect.height + 175f);
            }
            activePage = page;
        }
        if(Input.GetMouseButtonUp(1))
        {
            Destroy(activePage);
            activePage = null;
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Destroy(activePage);
            activePage = null;
        }
    }
}
