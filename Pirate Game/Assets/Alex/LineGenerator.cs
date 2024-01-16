using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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
        List<string> lineTypes = new List<string>();
        foreach (DrawLine l in lines)
        {
            if(l.lineRenderer.positionCount == 2)
            {
                lineTypes.Add("Dot");
            }
            else if(l.lineRenderer.positionCount >= 3)
            {
                Vector3[] positions = new Vector3[l.lineRenderer.positionCount];
                l.lineRenderer.GetPositions(positions);
                float totalDX = 0;
                float totalDY = 0;
                float minGradient = 10000;
                float maxGradient = -10000;
                float prevGradient = 0;
                float streak = 0;
                List<bool> curves = new List<bool>();
                for(int i = 1; i < positions.Length-1; i++)
                {
                    float dX = positions[i].x - positions[i - 1].x;
                    float dY = positions[i].y - positions[i - 1].y;
                    totalDX += dX;
                    totalDY += dY;
                    if (Mathf.Abs(dX) < 0.01f) dX = 0;
                    if (Mathf.Abs(dY) < 0.01f) dY = 0;
                    float gradient = 0;
                    if (dX != 0) gradient = dY / dX;
                    if (i > 1)
                    {
                        if((gradient < prevGradient && !(gradient < 0 && prevGradient < 0)) || (gradient > prevGradient && gradient < 0 && prevGradient < 0))
                        {
                            if (streak < 0)
                            {
                                streak--;
                                if (streak == -2)
                                {
                                    curves.Add(false);
                                }
                            }
                            else if((Mathf.Abs(prevGradient - gradient) > 0.5f) || (prevGradient > 0 && gradient < 0) || (prevGradient < 0 && gradient > 0))
                            {
                                streak = -1;
                            }
                        }
                        else if((gradient > prevGradient && !(gradient < 0 && prevGradient < 0)) || (gradient < prevGradient && gradient < 0 && prevGradient < 0))
                        {
                            if(streak > 0)
                            {
                                streak++;
                                if (streak == 2)
                                {
                                    curves.Add(true);
                                }
                            }
                            else if ((Mathf.Abs(gradient - prevGradient) > 0.5f) || (prevGradient > 0 && gradient < 0) || (prevGradient < 0 && gradient > 0))
                            {
                                streak = 1;
                            }
                        }
                    }
                    if(gradient < minGradient) minGradient = gradient;
                    if(gradient > maxGradient) maxGradient = gradient;
                    prevGradient = gradient;
                }
                if (minGradient == 0 || maxGradient == 0)
                {
                    if (Mathf.Abs(totalDX) < Mathf.Abs(totalDY / 2.5f))
                    {
                        lineTypes.Add("Vertical Line");
                    }
                    else if (Mathf.Abs(totalDY) < Mathf.Abs(totalDX / 2.5f))
                    {
                        lineTypes.Add("Horizontal Line");
                    }
                }
                else
                {
                    if(maxGradient - minGradient < 2)
                    {
                        lineTypes.Add("Diagonal Line");
                    }
                    if (curves.Count == 2 && curves[0] != curves[1])
                    {
                        lineTypes.Add("Curve");
                    }
                    else if(curves.Count == 4 && curves[0] != curves[1] && curves[1] != curves[2] && curves[2] != curves[3] && Vector3.Distance(positions[0], positions[positions.Length-1]) < 0.1f)
                    {
                        lineTypes.Add("Circle");
                    }
                }
            }
            Destroy(l.gameObject);
        }
        foreach(string type in lineTypes)
        {
            Debug.Log(type);
        }
        Debug.Log("---------");
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
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z)) * -1;
                activeLine.FinishLine(mousePos);
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
