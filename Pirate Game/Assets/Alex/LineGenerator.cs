using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;

enum LineTypes
{
    Dot,
    HorizontalLine,
    VerticalLine,
    DiagonalLine,
    Curve,
    Circle
}

public enum RuneTypes
{
    Ice,
    Fire,
    Invalid
}

public struct RuneInfo
{
    public RuneTypes type;
    public int accuracy;

    public RuneInfo(RuneTypes type, int accuracy)
    {
        this.type = type;
        this.accuracy = accuracy;
    }
}

struct LineInfo
{
    public LineTypes lineType;
    public List<Vector3> positions;

    public LineInfo(LineTypes lineType, List<Vector3> positions)
    {
        this.lineType = lineType;
        this.positions = positions;
    }
}

public class LineGenerator : MonoBehaviour
{
    public static event Action<RuneInfo> OnRuneComplete;

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
        List<LineInfo> lineTypes = new List<LineInfo>();
        var lineLookup = new Dictionary<LineTypes, List<int>>();
        for(int i = 0; i < lines.Count; i++)
        {
            Vector3[] positions = new Vector3[lines[i].lineRenderer.positionCount];
            lines[i].lineRenderer.GetPositions(positions);
            if (lines[i].lineRenderer.positionCount == 2)
            {
                List<Vector3> linePositions = new List<Vector3>() { positions[0] };
                LineInfo newInfo = new LineInfo(LineTypes.Dot, linePositions);
                lineTypes.Add(newInfo);
            }
            else if (lines[i].lineRenderer.positionCount >= 3)
            {
                float minGradient = 10000;
                float maxGradient = -10000;
                float prevGradient = 0;
                float streak = 0;
                List<bool> curves = new List<bool>();
                for(int j = 1; j < positions.Length-1; j++)
                {
                    float dX = positions[j].x - positions[j - 1].x;
                    float dY = positions[j].y - positions[j - 1].y;
                    if (Mathf.Abs(dX) < 0.01f) dX = 0;
                    if (Mathf.Abs(dY) < 0.01f) dY = 0;
                    float gradient = 0;
                    if (dX != 0) gradient = dY / dX;
                    if (j > 1)
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
                    Debug.Log(dX + ", " + dY);
                }
                Debug.Log("Min and Max: " + minGradient + ", " + maxGradient);
                if (Mathf.Abs(minGradient) <= 0.3 || Mathf.Abs(maxGradient) <= 0.3)
                {
                    if (Mathf.Abs(positions[0].y - positions[positions.Length-1].y) > Mathf.Abs(positions[0].x - positions[positions.Length - 1].x))
                    {
                        List<Vector3> linePositions = new List<Vector3>() { positions[0], positions[positions.Length-1] };
                        LineInfo newInfo = new LineInfo(LineTypes.VerticalLine, linePositions);
                        lineTypes.Add(newInfo);
                        if (!lineLookup.ContainsKey(LineTypes.VerticalLine)) lineLookup[LineTypes.VerticalLine] = new List<int>() { 0 };
                        lineLookup[LineTypes.VerticalLine][0]++;
                        lineLookup[LineTypes.VerticalLine].Add(i);
                    }
                    else if (Mathf.Abs(positions[0].y - positions[positions.Length - 1].y) < Mathf.Abs(positions[0].x - positions[positions.Length - 1].x))
                    {
                        List<Vector3> linePositions = new List<Vector3>() { positions[0], positions[positions.Length - 1] };
                        LineInfo newInfo = new LineInfo(LineTypes.HorizontalLine, linePositions);
                        lineTypes.Add(newInfo);
                        if (!lineLookup.ContainsKey(LineTypes.HorizontalLine)) lineLookup[LineTypes.HorizontalLine] = new List<int>() { 0 };
                        lineLookup[LineTypes.HorizontalLine][0]++;
                        lineLookup[LineTypes.HorizontalLine].Add(i);
                    }
                }
                else
                {
                    if(maxGradient - minGradient < 3)
                    {
                        List<Vector3> linePositions = new List<Vector3>() { positions[0], positions[positions.Length - 1] };
                        LineInfo newInfo = new LineInfo(LineTypes.DiagonalLine, linePositions);
                        lineTypes.Add(newInfo);
                        if (!lineLookup.ContainsKey(LineTypes.DiagonalLine)) lineLookup[LineTypes.DiagonalLine] = new List<int>() { 0 };
                        lineLookup[LineTypes.DiagonalLine][0]++;
                        lineLookup[LineTypes.DiagonalLine].Add(i);
                    }
                    else if (curves.Count == 2 && curves[0] != curves[1])
                    {
                        List<Vector3> linePositions = new List<Vector3>() { positions[0], positions[positions.Length/2], positions[positions.Length - 1] };
                        LineInfo newInfo = new LineInfo(LineTypes.Curve, linePositions);
                        lineTypes.Add(newInfo);
                    }
                    else if(curves.Count == 4 && curves[0] != curves[1] && curves[1] != curves[2] && curves[2] != curves[3] && Vector3.Distance(positions[0], positions[positions.Length-1]) < 0.1f)
                    {
                        List<Vector3> linePositions = new List<Vector3>() { positions[0], positions[positions.Length/4], positions[positions.Length/2], positions[positions.Length * 3 / 4] };
                        LineInfo newInfo = new LineInfo(LineTypes.VerticalLine, linePositions);
                        lineTypes.Add(newInfo);
                    }
                }
            }
            Destroy(lines[i].gameObject);
        }
        foreach(LineInfo info in lineTypes)
        {
            switch (info.lineType)
            {
                case LineTypes.Dot:
                    Debug.Log("Dot: " + info.positions[0]);
                    break;
                case LineTypes.HorizontalLine:
                    Debug.Log("Horizontal Line: " + info.positions[0] + " " + info.positions[1]);
                    break;
                case LineTypes.VerticalLine:
                    Debug.Log("Vertical Line: " + info.positions[0] + " " + info.positions[1]);
                    break;
                case LineTypes.DiagonalLine:
                    Debug.Log("Diagonal Line: " + info.positions[0] + " " + info.positions[1]);
                    break;
                case LineTypes.Curve:
                    Debug.Log("Curve: " );
                    break;
                case LineTypes.Circle:
                    Debug.Log("Circle: ");
                    break;
            }
        }
        RuneInfo rInfo = new RuneInfo(RuneTypes.Invalid, -1);
        int count = 0;
        do
        {
            switch (count)
            {
                case 0:
                    rInfo.accuracy = CheckIce(lineTypes, lineLookup);
                    if (rInfo.accuracy != -1)
                    {
                        rInfo.type = RuneTypes.Ice;
                    }
                    break;
                case 1:
                    rInfo.accuracy = CheckFire(lineTypes, lineLookup);
                    if (rInfo.accuracy != -1)
                    {
                        rInfo.type = RuneTypes.Fire;
                    }
                    break;
            }
            count++;
        }
        while (count < 2 && rInfo.type == RuneTypes.Invalid);
        OnRuneComplete?.Invoke(rInfo);
        Debug.Log("---------");
        lines.Clear();
    }

    int CheckIce(List<LineInfo> info, Dictionary<LineTypes, List<int>> lineLookup)
    {
        if(lineLookup.ContainsKey(LineTypes.DiagonalLine) && lineLookup.ContainsKey(LineTypes.VerticalLine))
        {
            if (lineLookup[LineTypes.DiagonalLine][0] == 2 && lineLookup[LineTypes.VerticalLine][0] == 1)
            {
                float minY = Mathf.Min(info[lineLookup[LineTypes.VerticalLine][1]].positions[0].y, info[lineLookup[LineTypes.VerticalLine][1]].positions[1].y);
                float maxY = Mathf.Max(info[lineLookup[LineTypes.VerticalLine][1]].positions[0].y, info[lineLookup[LineTypes.VerticalLine][1]].positions[1].y);
                bool upwardDiagonal = false;
                for(int i = 1; i < 3; i++)
                {
                    if ((info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].x < info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x &&
                    info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].x > info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x) ||
                    (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].x > info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x &&
                    info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].x < info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x))
                    {
                        if ((info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y < maxY &&
                        info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y > minY) ||
                        (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y > minY &&
                        info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y < maxY))
                        {
                            if (i == 1)
                            {
                                if (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y < info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y)
                                {
                                    upwardDiagonal = true;
                                }
                            }
                            else
                            {
                                if (upwardDiagonal && 
                                    info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y > info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y) return 100;
                                else if (!upwardDiagonal && 
                                    info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y < info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y) return 100;
                                else return -1;
                            }
                        }
                        else return -1;
                    }
                }
            }
        }
        return -1;
    }

    int CheckFire(List<LineInfo> info, Dictionary<LineTypes, List<int>> lineLookup)
    {
        if (lineLookup.ContainsKey(LineTypes.HorizontalLine) && lineLookup.ContainsKey(LineTypes.VerticalLine))
        {
            if (lineLookup[LineTypes.HorizontalLine][0] == 1 && lineLookup[LineTypes.VerticalLine][0] == 3)
            {
                float minX = Mathf.Min(info[lineLookup[LineTypes.HorizontalLine][1]].positions[0].x, info[lineLookup[LineTypes.HorizontalLine][1]].positions[1].x);
                float maxX = Mathf.Max(info[lineLookup[LineTypes.HorizontalLine][1]].positions[0].x, info[lineLookup[LineTypes.HorizontalLine][1]].positions[1].x);
                for(int i = 1; i < 4; i++)
                {
                    float minY = Mathf.Min(info[lineLookup[LineTypes.VerticalLine][i]].positions[0].y, info[lineLookup[LineTypes.VerticalLine][i]].positions[1].y);
                    if(minY < info[lineLookup[LineTypes.HorizontalLine][1]].positions[0].y ||
                        minX > info[lineLookup[LineTypes.VerticalLine][i]].positions[0].x ||
                        maxX < info[lineLookup[LineTypes.VerticalLine][i]].positions[0].x)
                    {
                        return -1;
                    }
                }
                return 100;
            }
        }
        return -1;
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
