using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineGenerator : MonoBehaviour
{
    public GameObject linePrefab;

    public List<DrawLine> lines = new List<DrawLine>();

    DrawLine activeLine;

    private float width;
    private float height;

    private void OnDestroy()
    {
        if (Input.GetMouseButton(0) && activeLine)
        {
            lines.Add(activeLine);
        }
        foreach (DrawLine l in lines)
        {
            Destroy(l.gameObject);
        }
        lines.Clear();
    }

    public void Awake()
    {
        width = Mathf.Abs(transform.position.x - (transform as RectTransform).TransformPoint((transform as RectTransform).rect.center).x) * 2;
        height = Mathf.Abs(transform.position.y - (transform as RectTransform).TransformPoint((transform as RectTransform).rect.center).y) * 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (InsideBox())
            {
                GameObject newLine = Instantiate(linePrefab);
                activeLine = newLine.GetComponent<DrawLine>();
            }
        }
        if (Input.GetMouseButtonUp(0))
            {
            if (activeLine)
            {
                lines.Add(activeLine);
                activeLine = null;
            }
        }
        if(Input.GetMouseButton(0))
        {
            if (!InsideBox() && activeLine)
            {
                lines.Add(activeLine);
                activeLine = null;
            }
            else if(InsideBox() && !activeLine)
            {
                GameObject newLine = Instantiate(linePrefab);
                activeLine = newLine.GetComponent<DrawLine>();
            }
        }
        
        if(activeLine != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z)) * -1;
            activeLine.UpdateLine(mousePos);
        }
    }

    private bool InsideBox()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z)) * -1;
        return mousePos.x > transform.position.x && mousePos.x < transform.position.x + width 
            && mousePos.y < transform.position.y && mousePos.y > transform.position.y - height;
    }
}
