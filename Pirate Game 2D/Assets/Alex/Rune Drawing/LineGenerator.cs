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
    public int accuracy;

    public LineInfo(LineTypes lineType, List<Vector3> positions, int accuracy)
    {
        this.lineType = lineType;
        this.positions = positions;
        this.accuracy = accuracy;
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
                LineInfo newInfo = new LineInfo(LineTypes.Dot, linePositions, 100);
                lineTypes.Add(newInfo);
            }
            else if (lines[i].lineRenderer.positionCount >= 3)
            {
                float minGradient = Mathf.Infinity;
                float maxGradient = Mathf.NegativeInfinity;
                float prevGradient = 0;
                float streak = 0;
                bool streakAdded = false;
                List<bool> curves = new List<bool>();
                for(int j = 1; j < positions.Length-1; j++)
                {
                    float dX = positions[j].x - positions[j - 1].x;
                    float dY = positions[j].y - positions[j - 1].y;
                    float gradient = dY / dX;
                    if (j > 1)
                    {
                        if(gradient == prevGradient)
                        {
                            if (streak < 0) streak--;
                            else if (streak > 0) streak++;
                        }
                        if((gradient < prevGradient && !(gradient < 0 && prevGradient < 0)) || (gradient > prevGradient && gradient < 0 && prevGradient < 0))
                        {
                            if (streak < 0)
                            {
                                streak--;
                                if (streak <= -3 && !streakAdded)
                                {
                                    curves.Add(false);
                                    streakAdded = true;
                                }
                            }
                            else if((Mathf.Abs(prevGradient - gradient) > 0.5f) || (prevGradient > 0 && gradient < 0) || (prevGradient < 0 && gradient > 0))
                            {
                                streak = -1;
                                streakAdded = false;
                            }
                        }
                        else if((gradient > prevGradient && !(gradient < 0 && prevGradient < 0)) || (gradient < prevGradient && gradient < 0 && prevGradient < 0))
                        {
                            if(streak > 0)
                            {
                                streak++;
                                if (streak >= 3 && !streakAdded)
                                {
                                    curves.Add(true);
                                    streakAdded = true;
                                }
                            }
                            else if ((Mathf.Abs(gradient - prevGradient) > 0.5f) || (prevGradient > 0 && gradient < 0) || (prevGradient < 0 && gradient > 0))
                            {
                                streak = 1;
                                streakAdded = false;
                            }
                        }
                    }
                    if(gradient < minGradient) minGradient = gradient;
                    if(gradient > maxGradient) maxGradient = gradient;
                    prevGradient = gradient;
                    Debug.Log(dX + ", " + dY + ", " + gradient + ", " + streak + ", " + curves.Count);
                }
                Debug.Log("Min and Max: " + minGradient + ", " + maxGradient);
                if (curves.Count == 2 && curves[0] != curves[1])
                {
                    List<Vector3> linePositions = new List<Vector3>() { positions[0], positions[positions.Length / 2], positions[positions.Length - 1] };
                    LineInfo newInfo = new LineInfo(LineTypes.Curve, linePositions, 100);
                    lineTypes.Add(newInfo);
                }
                else if (Mathf.Abs(minGradient) <= 0.3 || Mathf.Abs(maxGradient) <= 0.3)
                {
                    List<Vector3> linePositions = new List<Vector3>() { positions[0], positions[positions.Length - 1] };
                    LineInfo newInfo = new LineInfo(LineTypes.HorizontalLine, linePositions, 
                        100 - (int)(10 * Mathf.Abs(minGradient)) - (int)(10 * Mathf.Abs(maxGradient)));
                    lineTypes.Add(newInfo);
                    if (!lineLookup.ContainsKey(LineTypes.HorizontalLine)) lineLookup[LineTypes.HorizontalLine] = new List<int>() { 0 };
                    lineLookup[LineTypes.HorizontalLine][0]++;
                    lineLookup[LineTypes.HorizontalLine].Add(i);
                }
                else if(Mathf.Abs(minGradient) > 4 || Mathf.Abs(maxGradient) > 4)
                {
                    List<Vector3> linePositions = new List<Vector3>() { positions[0], positions[positions.Length - 1] };
                    LineInfo newInfo = new LineInfo(LineTypes.VerticalLine, linePositions, 
                        100 - (int)Mathf.Max(0, 15 - Mathf.Abs(minGradient)) - (int)Mathf.Max(0, 15 - Mathf.Abs(maxGradient)));
                    lineTypes.Add(newInfo);
                    if (!lineLookup.ContainsKey(LineTypes.VerticalLine)) lineLookup[LineTypes.VerticalLine] = new List<int>() { 0 };
                    lineLookup[LineTypes.VerticalLine][0]++;
                    lineLookup[LineTypes.VerticalLine].Add(i);
                }
                else if (maxGradient - minGradient < 4)
                {
                    List<Vector3> linePositions = new List<Vector3>() { positions[0], positions[positions.Length - 1] };
                    if (positions[0].x - positions[positions.Length - 1].x > -0.3f && positions[0].x - positions[positions.Length - 1].x < 0.3f)
                    {
                        LineInfo newInfo = new LineInfo(LineTypes.VerticalLine, linePositions,
                    100 - (int)Mathf.Max(0, 15 - Mathf.Abs(minGradient)) - (int)Mathf.Max(0, 15 - Mathf.Abs(maxGradient)));
                        lineTypes.Add(newInfo);
                        if (!lineLookup.ContainsKey(LineTypes.VerticalLine)) lineLookup[LineTypes.VerticalLine] = new List<int>() { 0 };
                        lineLookup[LineTypes.VerticalLine][0]++;
                        lineLookup[LineTypes.VerticalLine].Add(i);
                    }
                    else
                    {
                        LineInfo newInfo = new LineInfo(LineTypes.DiagonalLine, linePositions, 100 - (int)(10 * (maxGradient - minGradient)));
                        lineTypes.Add(newInfo);
                        if (!lineLookup.ContainsKey(LineTypes.DiagonalLine)) lineLookup[LineTypes.DiagonalLine] = new List<int>() { 0 };
                        lineLookup[LineTypes.DiagonalLine][0]++;
                        lineLookup[LineTypes.DiagonalLine].Add(i);
                    }
                }
                else if (curves.Count == 4 && curves[0] != curves[1] && curves[1] != curves[2] && curves[2] != curves[3] && Vector3.Distance(positions[0], positions[positions.Length - 1]) < 0.1f)
                {
                    List<Vector3> linePositions = new List<Vector3>() { positions[0], positions[positions.Length / 4], positions[positions.Length / 2], positions[positions.Length * 3 / 4] };
                    LineInfo newInfo = new LineInfo(LineTypes.VerticalLine, linePositions, 100);
                    lineTypes.Add(newInfo);
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
                    Debug.Log("Horizontal Line: " + info.positions[0] + " " + info.positions[1] + " " + info.accuracy);
                    break;
                case LineTypes.VerticalLine:
                    Debug.Log("Vertical Line: " + info.positions[0] + " " + info.positions[1] + " " + info.accuracy);
                    break;
                case LineTypes.DiagonalLine:
                    Debug.Log("Diagonal Line: " + info.positions[0] + " " + info.positions[1] + " " + info.accuracy);
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
                        float gradient = (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y - info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y) /
                            (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].x - info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].x);
                        float c = info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y - (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].x * gradient);
                        float yInt = (gradient * info[lineLookup[LineTypes.VerticalLine][1]].positions[1].x) + c;
                        if (yInt > minY && yInt < maxY)
                        {
                            if (i == 1)
                            {
                                if(info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].x < info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].x)
                                {
                                    if (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y < info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y)
                                    {
                                        upwardDiagonal = true;
                                    }
                                }
                                else if(info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].x > info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].x)
                                {
                                    if (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y > info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y)
                                    {
                                        upwardDiagonal = true;
                                    }
                                }
                            }
                            else
                            {
                                if (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].x < info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].x)
                                {
                                    if (upwardDiagonal &&
                                    info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y > info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y) 
                                        return IceAccuracyCalculator(info, lineLookup);
                                    else if (!upwardDiagonal &&
                                        info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y < info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y) 
                                        return IceAccuracyCalculator(info, lineLookup);
                                    else return -1;
                                }
                                else if (info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].x > info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].x)
                                {
                                    if (upwardDiagonal &&
                                    info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y < info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y) 
                                        return IceAccuracyCalculator(info, lineLookup);
                                    else if (!upwardDiagonal &&
                                        info[lineLookup[LineTypes.DiagonalLine][i]].positions[0].y > info[lineLookup[LineTypes.DiagonalLine][i]].positions[1].y) 
                                        return IceAccuracyCalculator(info, lineLookup);
                                    else return -1;
                                }
                            }
                        }
                        else return -1;
                    }
                }
            }
        }
        return -1;
    }

    int IceAccuracyCalculator(List<LineInfo> info, Dictionary<LineTypes, List<int>> lineLookup)
    {
        int lineAccuracy = (info[0].accuracy + info[1].accuracy + info[2].accuracy) / 3;
        Debug.Log("Accuracy: " + lineAccuracy);
        float gradientOne = (info[lineLookup[LineTypes.DiagonalLine][1]].positions[0].y - info[lineLookup[LineTypes.DiagonalLine][1]].positions[1].y) /
                            (info[lineLookup[LineTypes.DiagonalLine][1]].positions[0].x - info[lineLookup[LineTypes.DiagonalLine][1]].positions[1].x);
        float cOne = info[lineLookup[LineTypes.DiagonalLine][1]].positions[0].y - (info[lineLookup[LineTypes.DiagonalLine][1]].positions[0].x * gradientOne);
        float yIntOne = (gradientOne * info[lineLookup[LineTypes.VerticalLine][1]].positions[1].x) + cOne;
        float gradientTwo = (info[lineLookup[LineTypes.DiagonalLine][2]].positions[0].y - info[lineLookup[LineTypes.DiagonalLine][2]].positions[1].y) /
                            (info[lineLookup[LineTypes.DiagonalLine][2]].positions[0].x - info[lineLookup[LineTypes.DiagonalLine][2]].positions[1].x);
        float cTwo = info[lineLookup[LineTypes.DiagonalLine][2]].positions[0].y - (info[lineLookup[LineTypes.DiagonalLine][2]].positions[0].x * gradientTwo);
        float yIntTwo = (gradientTwo * info[lineLookup[LineTypes.VerticalLine][1]].positions[1].x) + cTwo;
        lineAccuracy -= (int)Mathf.Abs(100 * (yIntOne - yIntTwo));
        Debug.Log("Accuracy: " + lineAccuracy);
        /*float vertDist = Mathf.Abs(info[lineLookup[LineTypes.VerticalLine][1]].positions[0].y - info[lineLookup[LineTypes.VerticalLine][1]].positions[1].y);
        float yDistOne = Mathf.Abs(info[lineLookup[LineTypes.DiagonalLine][1]].positions[0].y - info[lineLookup[LineTypes.DiagonalLine][1]].positions[1].y);
        float yDistTwo = Mathf.Abs(info[lineLookup[LineTypes.DiagonalLine][2]].positions[0].y - info[lineLookup[LineTypes.DiagonalLine][2]].positions[1].y);
        float xDistOne = Mathf.Abs(info[lineLookup[LineTypes.DiagonalLine][1]].positions[0].x - info[lineLookup[LineTypes.DiagonalLine][1]].positions[1].x);
        float xDistTwo = Mathf.Abs(info[lineLookup[LineTypes.DiagonalLine][2]].positions[0].x - info[lineLookup[LineTypes.DiagonalLine][2]].positions[1].x);
        lineAccuracy -= (int)((Mathf.Abs(vertDist - yDistOne) * 10) + (Mathf.Abs(vertDist - yDistTwo) * 10) 
            + (Mathf.Abs(vertDist - xDistOne) * 10) + (Mathf.Abs(vertDist - xDistTwo) * 10));
        Debug.Log("Accuracy: " + lineAccuracy);*/
        return lineAccuracy;
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
                return FireAccuracyCalculator(info, lineLookup);
            }
        }
        return -1;
    }

    int FireAccuracyCalculator(List<LineInfo> info, Dictionary<LineTypes, List<int>> lineLookup)
    {
        int lineAccuracy = (info[0].accuracy + info[1].accuracy + info[2].accuracy + info[3].accuracy) / 4;
        Debug.Log("Accuracy: " + lineAccuracy);
        float minX = Mathf.Min(info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x,
            Mathf.Min(info[lineLookup[LineTypes.VerticalLine][2]].positions[0].x, info[lineLookup[LineTypes.VerticalLine][3]].positions[0].x));
        float maxX = Mathf.Max(info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x,
            Mathf.Max(info[lineLookup[LineTypes.VerticalLine][2]].positions[0].x, info[lineLookup[LineTypes.VerticalLine][3]].positions[0].x));
        float midX = info[lineLookup[LineTypes.VerticalLine][1]].positions[0].x + info[lineLookup[LineTypes.VerticalLine][2]].positions[0].x +
            info[lineLookup[LineTypes.VerticalLine][3]].positions[0].x - minX - maxX;
        lineAccuracy -= (int)Mathf.Abs((maxX - midX) - (midX - minX)) * 100;
        Debug.Log("Accuracy: " + lineAccuracy);
        float sizeOne = Mathf.Abs(info[lineLookup[LineTypes.VerticalLine][1]].positions[0].y - info[lineLookup[LineTypes.VerticalLine][1]].positions[1].y);
        float sizeTwo = Mathf.Abs(info[lineLookup[LineTypes.VerticalLine][2]].positions[0].y - info[lineLookup[LineTypes.VerticalLine][2]].positions[1].y);
        float sizeThree = Mathf.Abs(info[lineLookup[LineTypes.VerticalLine][3]].positions[0].y - info[lineLookup[LineTypes.VerticalLine][3]].positions[1].y);
        lineAccuracy -= (int)((Mathf.Abs(sizeOne - sizeTwo) * 25) + (Mathf.Abs(sizeOne - sizeThree) * 25));
        Debug.Log("Accuracy: " + lineAccuracy);
        return lineAccuracy;
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
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z))
                    - new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
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
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z))
                - new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
            activeLine.UpdateLine(mousePos);
        }
    }

    private bool InsideBox()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
        return mousePos.x > transform.position.x && mousePos.x < transform.position.x + width 
            && mousePos.y < transform.position.y && mousePos.y > transform.position.y - height;
    }
}
